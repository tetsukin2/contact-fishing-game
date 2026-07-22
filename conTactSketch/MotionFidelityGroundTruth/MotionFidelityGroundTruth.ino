#include <Arduino_LSM6DS3.h>

// =====================================================
// ConTact IMU Motion Fidelity Prompted Test
// Arduino-only, no BLE
// Records Unity-style IMU values for each prompted action
// Also compares against classifier prediction
// =====================================================

const unsigned long SAMPLE_INTERVAL_MS = 100;  // 10 Hz, less spam
const int SAMPLES_PER_ACTION = 50;             // 5 seconds per action

const int ACTION_COUNT = 7;

const char* actions[ACTION_COUNT] = {
  "Neutral",
  "RadialDeviation",
  "UlnarDeviation",
  "Flexion",
  "Extension",
  "Pronation",
  "Supination"
};

int currentActionIndex = 0;
int sampleCount = 0;
bool recording = false;

unsigned long lastSampleTime = 0;

void setup() {
  Serial.begin(115200);
  while (!Serial) {}

  if (!IMU.begin()) {
    Serial.println("ERROR: IMU failed to start.");
    while (1)
      ;
  }

  Serial.println("ConTact IMU Prompted Motion Fidelity Test");
  Serial.println("This test records the prompted action as ground truth.");
  Serial.println("It also predicts the motion using axis-combination rules.");
  Serial.println();
  Serial.println("Type s then press Enter to start each action recording.");
  Serial.println("Each action records for 5 seconds.");
  Serial.println();

  printCurrentPrompt();

  Serial.println();
  Serial.println("sample,action,UnityX,UnityY,UnityZ,detected_motion");
}

void loop() {
  handleSerialCommand();

  if (!recording) {
    return;
  }

  unsigned long now = millis();

  if (now - lastSampleTime < SAMPLE_INTERVAL_MS) {
    return;
  }

  lastSampleTime = now;

  float ax, ay, az;

  if (!IMU.accelerationAvailable()) {
    return;
  }

  IMU.readAcceleration(ax, ay, az);

  // Match Unity mapping:
  // CurrentX = -ax
  // CurrentY = -ay
  // CurrentZ = -az
  float unityX = -ax;
  float unityY = -ay;
  float unityZ = -az;

  String detectedMotion = classifyMotion(unityX, unityY, unityZ);

  Serial.print(sampleCount + 1);
  Serial.print(",");
  Serial.print(actions[currentActionIndex]);   // ground truth / prompted action
  Serial.print(",");
  Serial.print(unityX, 4);
  Serial.print(",");
  Serial.print(unityY, 4);
  Serial.print(",");
  Serial.print(unityZ, 4);
  Serial.print(",");
  Serial.println(detectedMotion);              // classifier prediction

  sampleCount++;

  if (sampleCount >= SAMPLES_PER_ACTION) {
    recording = false;
    sampleCount = 0;

    Serial.print("DONE: ");
    Serial.println(actions[currentActionIndex]);

    currentActionIndex++;

    if (currentActionIndex >= ACTION_COUNT) {
      Serial.println();
      Serial.println("TEST COMPLETE.");
      Serial.println("Copy the CSV rows above into Excel or Google Sheets.");
      while (1) {}
    }

    printCurrentPrompt();
  }
}

void handleSerialCommand() {
  if (!Serial.available()) {
    return;
  }

  char c = Serial.read();

  if (c == 's' || c == 'S') {
    if (!recording) {
      recording = true;
      sampleCount = 0;
      lastSampleTime = millis();

      Serial.print("START: ");
      Serial.println(actions[currentActionIndex]);
    }
  }
}

void printCurrentPrompt() {
  Serial.println();
  Serial.print("Prepare action: ");
  Serial.println(actions[currentActionIndex]);
  Serial.println("Hold the posture, then type s and press Enter.");
}

// =====================================================
// Classifier based on your prompted IMU test results
// =====================================================

String classifyMotion(float x, float y, float z) {
  // Neutral:
  // Your neutral samples were around:
  // X = 0.02, Y = 0.06, Z = 0.99
  if (abs(x) < 0.20 && abs(y) < 0.20 && z > 0.85) {
    return "Neutral";
  }

  // Pronation / Supination:
  // These are dominated by UnityY.
  if (y < -0.80) {
    return "Pronation";
  }

  if (y > 0.80) {
    return "Supination";
  }

  // Flexion / Extension:
  // These overlap with radial/ulnar on X,
  // so Z is used to separate them.
  if (x > 0.75 && z < 0.40) {
    return "Flexion";
  }

  if (x < -0.70 && z < 0.00) {
    return "Extension";
  }

  // Radial / Ulnar deviation:
  // These also use X, but Z remains positive.
  if (x < -0.50 && z > 0.40) {
    return "RadialDeviation";
  }

  if (x > 0.45 && z > 0.50) {
    return "UlnarDeviation";
  }

  return "Unclear";
}