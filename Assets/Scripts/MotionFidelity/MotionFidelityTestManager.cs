using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;

public class MotionFidelityTestManager : MonoBehaviour
{
    private enum TestState
    {
        WaitingToStartTrial,
        Countdown,
        Sampling,
        Complete
    }

    private class MotionTrial
    {
        public int GlobalTrialIndex;
        public int MotionTrialIndex;
        public InputDeviceRotationHelper.MotionClassification ExpectedMotion;
    }

    private class TrialResult
    {
        public int GlobalTrialIndex;
        public int MotionTrialIndex;
        public string ExpectedMotion;
        public int OverallCorrectSamples;
        public int OverallTotalSamples;
        public float OverallAccuracy;
        public int EvaluatedCorrectSamples;
        public int EvaluatedTotalSamples;
        public float SteadyStateAccuracy;
        public float TimeToFirstDetection;
        public float ExpectedDetectionSeconds;
        public string Result;
    }

    private class RawSample
    {
        public int GlobalTrialIndex;
        public int MotionTrialIndex;
        public string ExpectedMotion;
        public float Time;
        public string DetectedMotion;
        public float X;
        public float Y;
        public float Z;
        public bool IsEvaluatedSample;
        public bool IsCorrect;
    }

    [Header("UI References")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text statusText;

    [Header("Test Settings")]
    [SerializeField] private int trialsPerMotion = 20;
    [SerializeField] private float countdownSeconds = 1.0f;
    [SerializeField] private float sampleWindowSeconds = 3.0f;
    [SerializeField] private float sampleIntervalSeconds = 0.1f;
    [SerializeField] private float transitionAllowanceSeconds = 0.5f;
    [SerializeField] private float passThreshold = 0.80f;

    [Header("Controls")]
    [SerializeField] private KeyCode startTrialKey = KeyCode.Space;
    [SerializeField] private KeyCode exportKey = KeyCode.E;
    [SerializeField] private KeyCode reshuffleKey = KeyCode.R;

    private readonly List<MotionTrial> trials = new();
    private readonly List<TrialResult> trialResults = new();
    private readonly List<RawSample> rawSamples = new();

    private TestState state = TestState.WaitingToStartTrial;

    private int currentTrialIndex = 0;

    private float countdownTimer = 0f;
    private float sampleTimer = 0f;
    private float nextSampleTime = 0f;

    private int overallTotalSamples = 0;
    private int overallCorrectSamples = 0;
    private int evaluatedTotalSamples = 0;
    private int evaluatedCorrectSamples = 0;

    private bool expectedDetected = false;
    private float timeToFirstDetection = -1f;

    private void Start()
    {
        BuildRandomizedTrials();
        UpdateInstructionText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(startTrialKey) && state == TestState.WaitingToStartTrial)
        {
            StartCountdown();
        }

        if (Input.GetKeyDown(exportKey))
        {
            ExportResults();
        }

        if (Input.GetKeyDown(reshuffleKey) && state != TestState.Sampling && state != TestState.Countdown)
        {
            ResetAndReshuffle();
        }

        if (state == TestState.Countdown)
        {
            RunCountdown();
        }
        else if (state == TestState.Sampling)
        {
            RunSamplingWindow();
        }

        UpdateStatusText();
    }

    private void BuildRandomizedTrials()
    {
        trials.Clear();
        trialResults.Clear();
        rawSamples.Clear();
        currentTrialIndex = 0;

        var motions = new List<InputDeviceRotationHelper.MotionClassification>
        {
            InputDeviceRotationHelper.MotionClassification.RadialDeviation,
            InputDeviceRotationHelper.MotionClassification.UlnarDeviation,
            InputDeviceRotationHelper.MotionClassification.Flexion,
            InputDeviceRotationHelper.MotionClassification.Extension,
            InputDeviceRotationHelper.MotionClassification.Pronation,
            InputDeviceRotationHelper.MotionClassification.Supination
        };

        int globalIndex = 1;

        foreach (var motion in motions)
        {
            for (int i = 1; i <= trialsPerMotion; i++)
            {
                trials.Add(new MotionTrial
                {
                    GlobalTrialIndex = globalIndex,
                    MotionTrialIndex = i,
                    ExpectedMotion = motion
                });

                globalIndex++;
            }
        }

        ShuffleTrials();

        // Reassign global index after shuffle so the CSV reflects actual test order.
        for (int i = 0; i < trials.Count; i++)
        {
            trials[i].GlobalTrialIndex = i + 1;
        }

        state = TestState.WaitingToStartTrial;
    }

    private void ShuffleTrials()
    {
        for (int i = 0; i < trials.Count; i++)
        {
            int randomIndex = Random.Range(i, trials.Count);
            (trials[i], trials[randomIndex]) = (trials[randomIndex], trials[i]);
        }
    }

    private void ResetAndReshuffle()
    {
        BuildRandomizedTrials();
        UpdateInstructionText();
        Debug.Log("MotionFidelityTest: Test reset and trials reshuffled.");
    }

    private void StartCountdown()
    {
        if (currentTrialIndex >= trials.Count)
        {
            state = TestState.Complete;
            UpdateInstructionText();
            return;
        }

        countdownTimer = countdownSeconds;
        state = TestState.Countdown;

        Debug.Log($"MotionFidelityTest: Countdown started for {GetCurrentTrialName()}");
        UpdateInstructionText();
    }

    private void RunCountdown()
    {
        countdownTimer -= Time.deltaTime;

        if (countdownTimer <= 0f)
        {
            StartSampling();
        }
    }

    private void StartSampling()
    {
        state = TestState.Sampling;

        sampleTimer = 0f;
        nextSampleTime = 0f;

        overallTotalSamples = 0;
        overallCorrectSamples = 0;
        evaluatedTotalSamples = 0;
        evaluatedCorrectSamples = 0;

        expectedDetected = false;
        timeToFirstDetection = -1f;

        Debug.Log($"MotionFidelityTest: Sampling started for {GetCurrentTrialName()}");
        UpdateInstructionText();
    }

    private void RunSamplingWindow()
    {
        sampleTimer += Time.deltaTime;

        if (sampleTimer >= nextSampleTime)
        {
            SampleCurrentMotion();
            nextSampleTime += sampleIntervalSeconds;
        }

        if (sampleTimer >= sampleWindowSeconds)
        {
            FinishCurrentTrial();
        }
    }

    private void SampleCurrentMotion()
    {
        var trial = trials[currentTrialIndex];
        var rotationHelper = InputDeviceManager.Instance.RotationHelper;
        var detectedMotion = rotationHelper.GetCurrentMotionClassification();

        bool isCorrect = detectedMotion == trial.ExpectedMotion;
        bool isEvaluatedSample = sampleTimer >= transitionAllowanceSeconds;

        overallTotalSamples++;

        if (isCorrect)
        {
            overallCorrectSamples++;
        }

        if (isEvaluatedSample)
        {
            evaluatedTotalSamples++;

            if (isCorrect)
            {
                evaluatedCorrectSamples++;
            }
        }

        if (!expectedDetected && isCorrect)
        {
            expectedDetected = true;
            timeToFirstDetection = sampleTimer;
        }

        rawSamples.Add(new RawSample
        {
            GlobalTrialIndex = trial.GlobalTrialIndex,
            MotionTrialIndex = trial.MotionTrialIndex,
            ExpectedMotion = trial.ExpectedMotion.ToString(),
            Time = sampleTimer,
            DetectedMotion = detectedMotion.ToString(),
            X = rotationHelper.CurrentX,
            Y = rotationHelper.CurrentY,
            Z = rotationHelper.CurrentZ,
            IsEvaluatedSample = isEvaluatedSample,
            IsCorrect = isCorrect
        });

        Debug.Log(
            $"MotionFidelitySample: Trial={trial.GlobalTrialIndex}, " +
            $"Expected={trial.ExpectedMotion}, Detected={detectedMotion}, " +
            $"Evaluated={isEvaluatedSample}, Correct={isCorrect}, " +
            $"X={rotationHelper.CurrentX:F3}, Y={rotationHelper.CurrentY:F3}, Z={rotationHelper.CurrentZ:F3}"
        );
    }

    private void FinishCurrentTrial()
    {
        var trial = trials[currentTrialIndex];

        float overallAccuracy = overallTotalSamples > 0
            ? (float)overallCorrectSamples / overallTotalSamples
            : 0f;

        float steadyStateAccuracy = evaluatedTotalSamples > 0
            ? (float)evaluatedCorrectSamples / evaluatedTotalSamples
            : 0f;

        float expectedDetectionSeconds = evaluatedCorrectSamples * sampleIntervalSeconds;

        bool passed = expectedDetected && steadyStateAccuracy >= passThreshold;

        trialResults.Add(new TrialResult
        {
            GlobalTrialIndex = trial.GlobalTrialIndex,
            MotionTrialIndex = trial.MotionTrialIndex,
            ExpectedMotion = trial.ExpectedMotion.ToString(),
            OverallCorrectSamples = overallCorrectSamples,
            OverallTotalSamples = overallTotalSamples,
            OverallAccuracy = overallAccuracy,
            EvaluatedCorrectSamples = evaluatedCorrectSamples,
            EvaluatedTotalSamples = evaluatedTotalSamples,
            SteadyStateAccuracy = steadyStateAccuracy,
            TimeToFirstDetection = timeToFirstDetection,
            ExpectedDetectionSeconds = expectedDetectionSeconds,
            Result = passed ? "Pass" : "Fail"
        });

        Debug.Log(
            $"MotionFidelityResult: Trial={trial.GlobalTrialIndex}, " +
            $"Expected={trial.ExpectedMotion}, " +
            $"Overall={overallCorrectSamples}/{overallTotalSamples} ({overallAccuracy:P1}), " +
            $"Steady={evaluatedCorrectSamples}/{evaluatedTotalSamples} ({steadyStateAccuracy:P1}), " +
            $"FirstDetection={timeToFirstDetection:F2}s, " +
            $"Result={(passed ? "Pass" : "Fail")}"
        );

        currentTrialIndex++;

        if (currentTrialIndex >= trials.Count)
        {
            state = TestState.Complete;
        }
        else
        {
            state = TestState.WaitingToStartTrial;
        }

        UpdateInstructionText();
    }

    private string GetCurrentTrialName()
    {
        if (currentTrialIndex >= trials.Count)
        {
            return "Complete";
        }

        var trial = trials[currentTrialIndex];
        return $"Trial {trial.GlobalTrialIndex}/{trials.Count}: {trial.ExpectedMotion}";
    }

    private void UpdateInstructionText()
    {
        if (instructionText == null)
        {
            return;
        }

        if (state == TestState.Complete)
        {
            instructionText.text =
                $"MOTION FIDELITY TEST COMPLETE\n\n" +
                $"Trials completed: {trialResults.Count}/{trials.Count}\n\n" +
                $"Press E to export results.\n" +
                $"Press R to reset and reshuffle.";
            return;
        }

        if (currentTrialIndex >= trials.Count)
        {
            return;
        }

        var trial = trials[currentTrialIndex];

        if (state == TestState.WaitingToStartTrial)
        {
            instructionText.text =
                $"MOTION FIDELITY TEST\n\n" +
                $"Trial {trial.GlobalTrialIndex}/{trials.Count}\n" +
                $"Target Motion:\n{trial.ExpectedMotion}\n\n" +
                $"Start from Neutral.\n" +
                $"Press SPACE, then perform the target motion after the countdown.";
        }
        else if (state == TestState.Countdown)
        {
            instructionText.text =
                $"GET READY\n\n" +
                $"Target Motion:\n{trial.ExpectedMotion}\n\n" +
                $"Start from Neutral.\n" +
                $"Perform when timer reaches 0.\n\n" +
                $"Starting in: {Mathf.CeilToInt(countdownTimer)}";
        }
        else if (state == TestState.Sampling)
        {
            instructionText.text =
                $"PERFORM NOW\n\n" +
                $"Target Motion:\n{trial.ExpectedMotion}\n\n" +
                $"Sampling: {sampleTimer:F1}/{sampleWindowSeconds:F1}s";
        }
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        if (InputDeviceManager.Instance == null || InputDeviceManager.Instance.RotationHelper == null)
        {
            statusText.text = "InputDeviceManager or RotationHelper missing.";
            return;
        }

        var rotationHelper = InputDeviceManager.Instance.RotationHelper;
        var detectedMotion = rotationHelper.GetCurrentMotionClassification();

        string currentTrialText = currentTrialIndex < trials.Count
            ? $"{trials[currentTrialIndex].ExpectedMotion}"
            : "Complete";

        statusText.text =
            $"State: {state}\n" +
            $"Current Trial: {currentTrialIndex + 1}/{trials.Count}\n" +
            $"Target: {currentTrialText}\n\n" +
            $"Detected: {detectedMotion}\n" +
            $"X: {rotationHelper.CurrentX:F3}\n" +
            $"Y: {rotationHelper.CurrentY:F3}\n" +
            $"Z: {rotationHelper.CurrentZ:F3}\n\n" +
            $"Overall: {overallCorrectSamples}/{overallTotalSamples}\n" +
            $"Evaluated: {evaluatedCorrectSamples}/{evaluatedTotalSamples}\n" +
            $"Completed: {trialResults.Count}/{trials.Count}\n\n" +
            $"SPACE: Start Trial\n" +
            $"E: Export\n" +
            $"R: Reset";
    }

    private void ExportResults()
    {
        if (trialResults.Count == 0)
        {
            Debug.LogWarning("MotionFidelityTest: No results to export.");
            return;
        }

        ExportTrialResultsCsv();
        ExportRawSamplesCsv();
        ExportMotionSummaryCsv();
    }

    private void ExportTrialResultsCsv()
    {
        StringBuilder csv = new StringBuilder();

        csv.AppendLine(
            "GlobalTrialIndex,MotionTrialIndex,ExpectedMotion," +
            "OverallCorrectSamples,OverallTotalSamples,OverallAccuracy," +
            "EvaluatedCorrectSamples,EvaluatedTotalSamples,SteadyStateAccuracy," +
            "TimeToFirstDetection,ExpectedDetectionSeconds,Result"
        );

        foreach (var result in trialResults)
        {
            csv.AppendLine(
                $"{result.GlobalTrialIndex}," +
                $"{result.MotionTrialIndex}," +
                $"{EscapeCsv(result.ExpectedMotion)}," +
                $"{result.OverallCorrectSamples}," +
                $"{result.OverallTotalSamples}," +
                $"{result.OverallAccuracy:F4}," +
                $"{result.EvaluatedCorrectSamples}," +
                $"{result.EvaluatedTotalSamples}," +
                $"{result.SteadyStateAccuracy:F4}," +
                $"{result.TimeToFirstDetection:F4}," +
                $"{result.ExpectedDetectionSeconds:F4}," +
                $"{result.Result}"
            );
        }

        WriteCsvFile("MotionFidelity_TrialResults", csv);
    }

    private void ExportRawSamplesCsv()
    {
        StringBuilder csv = new StringBuilder();

        csv.AppendLine(
            "GlobalTrialIndex,MotionTrialIndex,ExpectedMotion,Time," +
            "DetectedMotion,UnityX,UnityY,UnityZ,IsEvaluatedSample,IsCorrect"
        );

        foreach (var sample in rawSamples)
        {
            csv.AppendLine(
                $"{sample.GlobalTrialIndex}," +
                $"{sample.MotionTrialIndex}," +
                $"{EscapeCsv(sample.ExpectedMotion)}," +
                $"{sample.Time:F4}," +
                $"{EscapeCsv(sample.DetectedMotion)}," +
                $"{sample.X:F4}," +
                $"{sample.Y:F4}," +
                $"{sample.Z:F4}," +
                $"{sample.IsEvaluatedSample}," +
                $"{sample.IsCorrect}"
            );
        }

        WriteCsvFile("MotionFidelity_RawSamples", csv);
    }

    private void ExportMotionSummaryCsv()
    {
        StringBuilder csv = new StringBuilder();

        csv.AppendLine(
            "Motion,TrialsPassed,TrialsFailed,TotalTrials,PassRate," +
            "MeanSteadyStateAccuracy,MinSteadyStateAccuracy,MaxSteadyStateAccuracy," +
            "MeanOverallAccuracy,MeanTimeToFirstDetection,MeanExpectedDetectionSeconds,Result"
        );

        var motionNames = new List<string>
        {
            "RadialDeviation",
            "UlnarDeviation",
            "Flexion",
            "Extension",
            "Pronation",
            "Supination"
        };

        foreach (var motionName in motionNames)
        {
            int totalTrials = 0;
            int passedTrials = 0;

            float steadySum = 0f;
            float overallSum = 0f;
            float firstDetectionSum = 0f;
            float expectedDetectionSecondsSum = 0f;

            float minSteady = float.MaxValue;
            float maxSteady = float.MinValue;

            int validFirstDetectionCount = 0;

            foreach (var result in trialResults)
            {
                if (result.ExpectedMotion != motionName)
                {
                    continue;
                }

                totalTrials++;

                steadySum += result.SteadyStateAccuracy;
                overallSum += result.OverallAccuracy;
                expectedDetectionSecondsSum += result.ExpectedDetectionSeconds;

                if (result.SteadyStateAccuracy < minSteady)
                {
                    minSteady = result.SteadyStateAccuracy;
                }

                if (result.SteadyStateAccuracy > maxSteady)
                {
                    maxSteady = result.SteadyStateAccuracy;
                }

                if (result.TimeToFirstDetection >= 0f)
                {
                    firstDetectionSum += result.TimeToFirstDetection;
                    validFirstDetectionCount++;
                }

                if (result.Result == "Pass")
                {
                    passedTrials++;
                }
            }

            int failedTrials = totalTrials - passedTrials;

            float passRate = totalTrials > 0
                ? (float)passedTrials / totalTrials
                : 0f;

            float meanSteady = totalTrials > 0
                ? steadySum / totalTrials
                : 0f;

            float meanOverall = totalTrials > 0
                ? overallSum / totalTrials
                : 0f;

            float meanFirstDetection = validFirstDetectionCount > 0
                ? firstDetectionSum / validFirstDetectionCount
                : -1f;

            float meanExpectedDetectionSeconds = totalTrials > 0
                ? expectedDetectionSecondsSum / totalTrials
                : 0f;

            if (totalTrials == 0)
            {
                minSteady = 0f;
                maxSteady = 0f;
            }

            // Motion-level pass: at least 80% of that motion's trials pass.
            bool motionPassed = totalTrials > 0 && passRate >= 0.80f;

            csv.AppendLine(
                $"{motionName}," +
                $"{passedTrials}," +
                $"{failedTrials}," +
                $"{totalTrials}," +
                $"{passRate:F4}," +
                $"{meanSteady:F4}," +
                $"{minSteady:F4}," +
                $"{maxSteady:F4}," +
                $"{meanOverall:F4}," +
                $"{meanFirstDetection:F4}," +
                $"{meanExpectedDetectionSeconds:F4}," +
                $"{(motionPassed ? "Pass" : "Fail")}"
            );
        }

        WriteCsvFile("MotionFidelity_MotionSummary", csv);
    }

    private void WriteCsvFile(string prefix, StringBuilder csv)
    {
        string fileName = $"{prefix}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(filePath, csv.ToString());

        Debug.Log($"MotionFidelityTest: Exported {filePath}");
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}