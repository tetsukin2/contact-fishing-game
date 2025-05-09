#include <ArduinoBLE.h>
#include <Arduino_LSM6DS3.h>

// === BLE Setup ===
BLEService imuService("19B10000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic imuCharacteristic("19B10001-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify, 12);

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
const int DATA_1 = 6; // P20-1 DATA
const int DATA_2 = 7; // P20-2 DATA

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

  // === IMU Calibration (Set current position as neutral) ===
  float sum_x = 0, sum_y = 0, sum_z = 0;
  int samples = 100;
  for (int i = 0; i < samples; i++) {
    float x, y, z;
    if (IMU.accelerationAvailable()) {
      IMU.readAcceleration(x, y, z);
      sum_x += x;
      sum_y += y;
      sum_z += z;
    }
    delay(10);
  }
  x_offset = sum_x / samples;
  y_offset = sum_y / samples;
  z_offset = sum_z / samples;

  Serial.print("📏 Calibrated Offset: ");
  Serial.print(x_offset); Serial.print(", ");
  Serial.print(y_offset); Serial.print(", ");
  Serial.println(z_offset);

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

bool recvInProgress = false;
byte ndx = 0;

void recvWithStartEndMarkers(char rc) {
  const char startMarker = '<';
  const char endMarker = '>';

  if (recvInProgress) {
    if (rc != endMarker) {
      if (ndx < 14) {
        receivedChars[ndx++] = rc;
      } else {
        // Overflow: abort and wait for next '<'
        recvInProgress = false;
        ndx = 0;
        Serial.println("⚠️ Overflow in BLE message. Waiting for new packet.");
      }
    } else {
      // End marker found, finalize message
      receivedChars[ndx] = '\0';
      recvInProgress = false;
      ndx = 0;

      // Parse values safely
      if (strlen(receivedChars) == 12) {
        int c0 = atoi(&receivedChars[0]);  // 0–2
        int c1 = atoi(&receivedChars[3]);  // 3–5
        int c2 = atoi(&receivedChars[6]);  // 6–8
        int c3 = atoi(&receivedChars[9]);  // 9–11

        cells1[0] = (byte)c0;
        cells1[1] = (byte)c1;
        cells2[0] = (byte)c2;
        cells2[1] = (byte)c3;
        FlushDualP20();

        Serial.print("📩 Received: ");
        Serial.print(c0); Serial.print(", ");
        Serial.print(c1); Serial.print(", ");
        Serial.print(c2); Serial.print(", ");
        Serial.println(c3);
      } else {
        Serial.println("⚠️ Invalid message length. Ignored.");
      }
    }
  } else if (rc == startMarker) {
    recvInProgress = true;
    ndx = 0;
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
        Serial.print("Cell2 Byte ");
        Serial.print(byteIndex);
        Serial.print(": ");
        Serial.println(cells2[byteIndex], BIN);
    }
  }

  digitalWrite(STROBE, HIGH);
}