#include <ArduinoBLE.h>
#include <Arduino_LSM6DS3.h>

// === BLE Setup ===
BLEService imuService("19B10000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic imuCharacteristic("19B10001-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify | BLEWriteWithoutResponse, 12);

BLEService joystickService("19B20000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic joystickCharacteristic("19B20001-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify, 3);

BLEService brailleService("19B30000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic brailleCharacteristic("19B30001-E8F2-537E-4F6C-D104768A1214", BLEWriteWithoutResponse, 20);

// === Pin Setup ===
const int VRX_PIN = A1;
const int VRY_PIN = A2;
const int SW_PIN  = 8;

const int ON     = 2;
const int STROBE = 4; // LATCH
const int CLOCK  = 5; // CLOCK
const int DATA_1 = 7; // P20-1 DATA
const int DATA_2 = 6; // P20-2 DATA

const int bitOrder[8] = {6, 7, 2, 1, 0, 5, 4, 3};

// === Globals ===
float x_offset = 0, y_offset = 0, z_offset = 0;
char receivedChars[15]; // "<AAABBBCCCDDD>"
bool newData = false;

byte cells1[2]; // P20-1
byte cells2[2]; // P20-2

void setup() {
  Serial.begin(115200);
  pinMode(SW_PIN, INPUT_PULLUP);

  pinMode(ON, OUTPUT);
  pinMode(STROBE, OUTPUT);
  pinMode(CLOCK, OUTPUT);
  pinMode(DATA_1, OUTPUT);
  pinMode(DATA_2, OUTPUT);
  digitalWrite(ON, LOW); // Booster ON

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

  // Reset both braille cells
  cells1[0] = cells1[1] = 0;
  cells2[0] = cells2[1] = 0;
  FlushDualP20();
  Serial.println("🛑 Initial state: Dual P20 reset");

  BLE.advertise();
  Serial.println("✅ BLE is now advertising!");
}

void loop() {
  BLEDevice central = BLE.central();

  if (central) {
    Serial.println("🔗 Connected to Unity!");

    // Reset both P20s on connect
    cells1[0] = cells1[1] = 0;
    cells2[0] = cells2[1] = 0;
    FlushDualP20();

    static unsigned long lastSendTime = 0;
    const int minSendInterval = 200;
    const int imuNoiseThreshold = 5;
    const int joyDeadzone = 8;

    static int16_t last_x = 0, last_y = 0, last_z = 0;
    static int last_vrx = 0, last_vry = 0, last_sw = 1;

    while (central.connected()) {
      if (millis() - lastSendTime >= minSendInterval) {
        lastSendTime = millis();

        // Heartbeat
        if (imuCharacteristic.written()) {
          int len = imuCharacteristic.valueLength();

          if (len == 1) {
            imuCharacteristic.writeValue((uint8_t*)1, 1);
            Serial.println("Responded to ping.");
          }
        }

        float x, y, z;
        if (IMU.accelerationAvailable()) {
          IMU.readAcceleration(x, y, z);
          x -= x_offset;
          y -= y_offset;
          z -= z_offset;
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

          // 🔍 Print calibrated values in raw units and Gs
          Serial.print("📈 Δ IMU Calibrated: ");
          Serial.print("ix: "); Serial.print(ix); Serial.print("\t");
          Serial.print("iy: "); Serial.print(iy); Serial.print("\t");
          Serial.print("iz: "); Serial.print(iz); Serial.print("\t → ");

          Serial.print("x: "); Serial.print(ix / 1000.0, 3); Serial.print("g\t");
          Serial.print("y: "); Serial.print(iy / 1000.0, 3); Serial.print("g\t");
          Serial.print("z: "); Serial.print(iz / 1000.0, 3); Serial.println("g");
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

    // Reset on disconnect
    cells1[0] = cells1[1] = 0;
    cells2[0] = cells2[1] = 0;
    FlushDualP20();
    Serial.println("🔌 BLE disconnected → Dual P20 reset");
  }
}

void recvWithStartEndMarkers(char rc) {
  static bool recvInProgress = false;
  static byte ndx = 0;
  const char startMarker = '<';
  const char endMarker = '>';

  if (rc == startMarker) {
    recvInProgress = true;
    ndx = 0;
    return;
  }

  if (recvInProgress) {
    if (rc != endMarker) {
      if (ndx < 12) {
        receivedChars[ndx++] = rc;
      }
    } else {
      // End marker received
      receivedChars[ndx] = '\0';
      recvInProgress = false;

      // Parse 4 chunks of 3 digits each: AAA BBB CCC DDD
      char buf0[4], buf1[4], buf2[4], buf3[4];
      strncpy(buf0, &receivedChars[0], 3); buf0[3] = '\0';
      strncpy(buf1, &receivedChars[3], 3); buf1[3] = '\0';
      strncpy(buf2, &receivedChars[6], 3); buf2[3] = '\0';
      strncpy(buf3, &receivedChars[9], 3); buf3[3] = '\0';

      int c0 = atoi(buf0);
      int c1 = atoi(buf1);
      int c2 = atoi(buf2);
      int c3 = atoi(buf3);

      cells1[0] = (byte)c0;
      cells1[1] = (byte)c1;
      cells2[0] = (byte)c2;
      cells2[1] = (byte)c3;

      FlushDualP20();

      Serial.print("📨 Raw receivedChars: <"); Serial.print(receivedChars); Serial.println(">");
      Serial.print("📩 Parsed Dual P20: ");
      Serial.print(c0); Serial.print(", ");
      Serial.print(c1); Serial.print(", ");
      Serial.print(c2); Serial.print(", ");
      Serial.print(c3); Serial.println();
    }
  }
}

void FlushDualP20() {
  digitalWrite(STROBE, LOW);
  for (int byteIndex = 0; byteIndex < 2; byteIndex++) {
    for (int bitIndex = 0; bitIndex < 8; bitIndex++) {
      int bit = bitOrder[bitIndex];

      digitalWrite(CLOCK, LOW);
      digitalWrite(DATA_1, bitRead(cells1[byteIndex], bit) ? LOW : HIGH);
      digitalWrite(DATA_2, bitRead(cells2[byteIndex], bit) ? LOW : HIGH);
      digitalWrite(CLOCK, HIGH);
    }
  }
  digitalWrite(STROBE, HIGH);
}