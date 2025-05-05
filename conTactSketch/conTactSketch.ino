#include <ArduinoBLE.h>
#include <Arduino_LSM6DS3.h>

BLEService imuService("19B10000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic imuCharacteristic("19B10001-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify, 12);

BLEService joystickService("19B20000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic joystickCharacteristic("19B20001-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify, 3);

BLEService brailleService("19B30000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic brailleCharacteristic("19B30001-E8F2-537E-4F6C-D104768A1214", BLEWriteWithoutResponse, 20);

const int VRX_PIN = A1;
const int VRY_PIN = A2;
const int SW_PIN  = 8;

const int ON        = 2;
const int LATCH_PIN = 4;
const int CLOCK_PIN = 5;
const int DATA_PIN_INDEX  = 6;
const int DATA_PIN_THUMB  = 7;

char receivedChars[15];
bool newData = false;

void setup() {
  Serial.begin(115200);
  pinMode(SW_PIN, INPUT_PULLUP);

  pinMode(ON, OUTPUT);
  pinMode(LATCH_PIN, OUTPUT);
  pinMode(CLOCK_PIN, OUTPUT);
  pinMode(DATA_PIN_INDEX, OUTPUT);
  pinMode(DATA_PIN_THUMB, OUTPUT);
  digitalWrite(ON, LOW); // Enable booster

  if (!BLE.begin()) {
    Serial.println("❌ BLE failed to start!");
    while (1);
  }

  BLE.setLocalName("FishingRodIMU");

  imuService.addCharacteristic(imuCharacteristic);
  joystickService.addCharacteristic(joystickCharacteristic);
  brailleService.addCharacteristic(brailleCharacteristic);

  BLE.addService(imuService);
  BLE.addService(joystickService);
  BLE.addService(brailleService);

  if (!IMU.begin()) {
    Serial.println("❌ IMU failed to start!");
    while (1);
  }

  // Default state = unactuated
  byte reset[4] = {0, 0, 0, 0};
  writeBrailleToP20(reset);
  Serial.println("🛑 Initial state: P20 reset (000000000000)");

  BLE.advertise();
  Serial.println("✅ BLE is now advertising!");
}

void loop() {
  BLEDevice central = BLE.central();

  if (central) {
    Serial.println("🔗 Connected to Unity!");

    // Reset to default state again on connection
    byte reset[4] = {0, 0, 0, 0};
    writeBrailleToP20(reset);
    Serial.println("🛑 P20 reset on connection");

    static unsigned long lastSendTime = 0;
    const int minSendInterval = 200;
    const int imuNoiseThreshold = 5;
    const int joyDeadzone = 8;

    static int16_t last_x = 0, last_y = 0, last_z = 0;
    static int last_vrx = 0, last_vry = 0, last_sw = 1;

    while (central.connected()) {
      if (millis() - lastSendTime >= minSendInterval) {
        lastSendTime = millis();

        float x, y, z;
        if (IMU.accelerationAvailable()) {
          IMU.readAcceleration(x, y, z);
        }

        int16_t ix = (int16_t)(x * 1000);
        int16_t iy = (int16_t)(y * 1000);
        int16_t iz = (int16_t)(z * 1000);

        if (abs(ix - last_x) > imuNoiseThreshold || abs(iy - last_y) > imuNoiseThreshold || abs(iz - last_z) > imuNoiseThreshold) {
          int16_t imuData[3] = {ix, iy, iz};
          imuCharacteristic.writeValue((uint8_t*)imuData, sizeof(imuData));
          last_x = ix;
          last_y = iy;
          last_z = iz;
        }

        int vrx = analogRead(VRX_PIN);
        int vry = analogRead(VRY_PIN);
        int sw = digitalRead(SW_PIN);

        uint8_t joyData[3] = {
          (uint8_t)map(vrx, 0, 1023, 0, 255),
          (uint8_t)map(vry, 0, 1023, 0, 255),
          (uint8_t)(sw == LOW ? 1 : 0)
        };

        bool xChanged = abs(vrx - last_vrx) > joyDeadzone;
        bool yChanged = abs(vry - last_vry) > joyDeadzone;
        bool swChanged = sw != last_sw;

        if (xChanged || yChanged || swChanged) {
          joystickCharacteristic.writeValue(joyData, sizeof(joyData));
          last_vrx = vrx;
          last_vry = vry;
          last_sw = sw;
        }
      }

      if (brailleCharacteristic.written()) {
        const uint8_t* rawData = brailleCharacteristic.value();
        int len = brailleCharacteristic.valueLength();
        for (int i = 0; i < len; i++) {
          recvWithStartEndMarkers(rawData[i]);
        }
      }
    }

    // Reset again after disconnect
    byte blank[4] = {0, 0, 0, 0};
    writeBrailleToP20(blank);
    Serial.println("🔌 BLE disconnected → P20 reset");
  }
}

void recvWithStartEndMarkers(char rc) {
  static boolean recvInProgress = false;
  static byte ndx = 0;
  char startMarker = '<';
  char endMarker = '>';

  if (recvInProgress) {
    if (rc != endMarker) {
      receivedChars[ndx] = rc;
      ndx++;
      if (ndx >= 13) ndx = 12;
    } else {
      receivedChars[ndx] = '\0';
      recvInProgress = false;
      ndx = 0;
      newData = true;

      // Parse format: <AAABBBCCCDDD>
      int t0 = String(receivedChars).substring(0, 3).toInt();
      int t1 = String(receivedChars).substring(3, 6).toInt();
      int i0 = String(receivedChars).substring(6, 9).toInt();
      int i1 = String(receivedChars).substring(9, 12).toInt();

      byte data[4] = { (byte)t0, (byte)t1, (byte)i0, (byte)i1 };

      writeBrailleToP20(data);

      Serial.print("Thumb P20: ");
      Serial.print(t0); Serial.print(" "); Serial.println(t1);
      Serial.print("Index P20: ");
      Serial.print(i0); Serial.print(" "); Serial.println(i1);
    }
  } else if (rc == startMarker) {
    recvInProgress = true;
  }
}

void writeBrailleToP20(const uint8_t* data) {
  digitalWrite(LATCH_PIN, LOW); // Thumb LATCH
  shiftOut(DATA_PIN_THUMB, CLOCK_PIN, MSBFIRST, data[1]);
  shiftOut(DATA_PIN_THUMB, CLOCK_PIN, MSBFIRST, data[0]);
  shiftOut(DATA_PIN_INDEX, CLOCK_PIN, MSBFIRST, data[3]);
  shiftOut(DATA_PIN_INDEX, CLOCK_PIN, MSBFIRST, data[2]);
  digitalWrite(LATCH_PIN, HIGH);
}
