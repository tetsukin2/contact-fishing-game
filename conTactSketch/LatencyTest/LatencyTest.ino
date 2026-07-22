#include <ArduinoBLE.h>
#include <Arduino_LSM6DS3.h>

// =====================================================
// ConTact BLE Tactile Pattern Communication Latency Test
// Separate test sketch. Does not replace gameplay sketch.
//
// Unity sends tactile-pattern-style payloads:
//
//   <AAABBBCCCDDD#ID>
//
// Example:
//
//   <255255255255#000>
//
// Arduino:
//   1. Parses the tactile payload
//   2. Sends it to the dual P20 pin arrays
//   3. Sends ACKID back to Unity
//
// Unity measures:
//   Unity send time -> ACK received time
// =====================================================

const char* DEVICE_NAME = "FishingRodIMU";

// === BLE Setup ===
BLEService imuService("19B10000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic imuCharacteristic(
  "19B10001-E8F2-537E-4F6C-D104768A1214",
  BLERead | BLENotify,
  12
);

BLEService joystickService("19B20000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic joystickCharacteristic(
  "19B20001-E8F2-537E-4F6C-D104768A1214",
  BLERead | BLENotify,
  5
);

BLEService brailleService("19B30000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic brailleCharacteristic(
  "19B30001-E8F2-537E-4F6C-D104768A1214",
  BLEWriteWithoutResponse,
  20
);

// New latency ACK notify characteristic
BLEService latencyService("19B40000-E8F2-537E-4F6C-D104768A1214");
BLECharacteristic latencyCharacteristic(
  "19B40001-E8F2-537E-4F6C-D104768A1214",
  BLERead | BLENotify,
  20
);

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

// Normal message: <AAABBBCCCDDD> = 14 chars including markers
// Latency message: <AAABBBCCCDDD#000> = 18 chars including markers
char receivedChars[24];

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
  pinMode(9, INPUT_PULLUP);
  pinMode(10, INPUT_PULLUP);

  digitalWrite(ON, LOW); // Booster ON

  if (!BLE.begin()) {
    Serial.println("❌ BLE failed to start!");
    while (1);
  }

  BLE.setLocalName(DEVICE_NAME);
  BLE.setDeviceName(DEVICE_NAME);

  imuService.addCharacteristic(imuCharacteristic);
  joystickService.addCharacteristic(joystickCharacteristic);
  brailleService.addCharacteristic(brailleCharacteristic);
  latencyService.addCharacteristic(latencyCharacteristic);

  BLE.addService(imuService);
  BLE.addService(joystickService);
  BLE.addService(brailleService);
  BLE.addService(latencyService);

  if (!IMU.begin()) {
    Serial.println("❌ IMU failed to start!");
    while (1);
  }

  // Initial dummy values so Unity can discover/read characteristics.
  int16_t neutralImu[3] = {0, 0, 0};
  imuCharacteristic.writeValue((uint8_t*)neutralImu, sizeof(neutralImu));

  uint8_t neutralJoy[5] = {0, 0, 0, 0, 0};
  joystickCharacteristic.writeValue(neutralJoy, sizeof(neutralJoy));

  latencyCharacteristic.writeValue((const uint8_t*)"READY", 5);

  // Reset both braille cells
  cells1[0] = cells1[1] = 0;
  cells2[0] = cells2[1] = 0;
  FlushDualP20();
  Serial.println("🛑 Initial state: Dual P20 reset");

  BLE.advertise();
  Serial.println("✅ ConTact tactile pattern latency firmware is advertising!");
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
      BLE.poll();

      // Keep the normal IMU/joystick notifications available so Unity's
      // existing InputDeviceManager connection flow continues to work.
      if (millis() - lastSendTime >= minSendInterval) {
        lastSendTime = millis();

        float x, y, z;

        if (IMU.accelerationAvailable()) {
          IMU.readAcceleration(x, y, z);
          x -= x_offset;
          y -= y_offset;
          z -= z_offset;

          int16_t ix = (int16_t)(x * 1000);
          int16_t iy = (int16_t)(y * 1000);
          int16_t iz = (int16_t)(z * 1000);

          if (
            abs(ix - last_x) > imuNoiseThreshold ||
            abs(iy - last_y) > imuNoiseThreshold ||
            abs(iz - last_z) > imuNoiseThreshold
          ) {
            int16_t imuData[3] = {ix, iy, iz};
            imuCharacteristic.writeValue((uint8_t*)imuData, sizeof(imuData));

            last_x = ix;
            last_y = iy;
            last_z = iz;
          }
        }

        int vrx = analogRead(VRX_PIN);
        int vry = analogRead(VRY_PIN);
        int sw = digitalRead(SW_PIN);

        int button1 = digitalRead(9) == LOW ? 1 : 0;
        int button2 = digitalRead(10) == LOW ? 1 : 0;

        uint8_t joyData[5] = {
          (uint8_t)map(vrx, 0, 1023, 0, 255),
          (uint8_t)map(vry, 0, 1023, 0, 255),
          (uint8_t)(sw == LOW ? 1 : 0),
          (uint8_t)button1,
          (uint8_t)button2
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
      if (ndx < sizeof(receivedChars) - 1) {
        receivedChars[ndx++] = rc;
      }
    } else {
      receivedChars[ndx] = '\0';
      recvInProgress = false;

      processBraillePacket(receivedChars);
    }
  }
}

void processBraillePacket(char* packet) {
  /*
   * Supported formats:
   *
   * Normal:
   *   AAABBBCCCDDD
   *
   * Latency-tracked:
   *   AAABBBCCCDDD#000
   *
   * Note: recvWithStartEndMarkers removes < and >.
   */

  int len = strlen(packet);

  if (len != 12 && len != 16) {
    Serial.print("Bad Braille packet length: ");
    Serial.print(len);
    Serial.print(" packet=");
    Serial.println(packet);
    return;
  }

  // First 12 characters must be digits.
  for (int i = 0; i < 12; i++) {
    if (packet[i] < '0' || packet[i] > '9') {
      Serial.print("Bad Braille packet: non-digit in tactile payload. packet=");
      Serial.println(packet);
      return;
    }
  }

  bool hasLatencyId = false;
  char id[4] = {'\0', '\0', '\0', '\0'};

  if (len == 16) {
    if (packet[12] != '#') {
      Serial.print("Bad latency packet: missing #. packet=");
      Serial.println(packet);
      return;
    }

    if (
      packet[13] < '0' || packet[13] > '9' ||
      packet[14] < '0' || packet[14] > '9' ||
      packet[15] < '0' || packet[15] > '9'
    ) {
      Serial.print("Bad latency packet: invalid id. packet=");
      Serial.println(packet);
      return;
    }

    id[0] = packet[13];
    id[1] = packet[14];
    id[2] = packet[15];
    id[3] = '\0';

    hasLatencyId = true;
  }

  char buf0[4], buf1[4], buf2[4], buf3[4];

  memcpy(buf0, &packet[0], 3);  buf0[3] = '\0';
  memcpy(buf1, &packet[3], 3);  buf1[3] = '\0';
  memcpy(buf2, &packet[6], 3);  buf2[3] = '\0';
  memcpy(buf3, &packet[9], 3);  buf3[3] = '\0';

  int c0 = atoi(buf0);
  int c1 = atoi(buf1);
  int c2 = atoi(buf2);
  int c3 = atoi(buf3);

  cells1[0] = (byte)constrain(c0, 0, 255);
  cells1[1] = (byte)constrain(c1, 0, 255);
  cells2[0] = (byte)constrain(c2, 0, 255);
  cells2[1] = (byte)constrain(c3, 0, 255);

  // Actual tactile output processing.
  FlushDualP20();

  // Send ACK after the firmware has processed and issued the tactile command.
  if (hasLatencyId) {
    sendLatencyAck(id);
  }
}

void sendLatencyAck(const char* id) {
  char ackMessage[8];

  snprintf(ackMessage, sizeof(ackMessage), "ACK%s", id);

  latencyCharacteristic.writeValue((uint8_t*)ackMessage, strlen(ackMessage));

  Serial.print("ACK sent: ");
  Serial.println(ackMessage);
}

void FlushDualP20() {
  digitalWrite(STROBE, LOW);
  delayMicroseconds(5);

  for (int byteIndex = 0; byteIndex < 2; byteIndex++) {
    for (int bitIndex = 0; bitIndex < 8; bitIndex++) {
      int bit = bitOrder[bitIndex];

      digitalWrite(CLOCK, LOW);

      bool bit1 = bitRead(cells1[byteIndex], bit);
      bool bit2 = bitRead(cells2[byteIndex], bit);

      digitalWrite(DATA_1, bit1 ? LOW : HIGH);
      digitalWrite(DATA_2, bit2 ? LOW : HIGH);

      delayMicroseconds(5);

      digitalWrite(CLOCK, HIGH);

      delayMicroseconds(5);
    }
  }

  digitalWrite(STROBE, HIGH);
  delayMicroseconds(5);
}