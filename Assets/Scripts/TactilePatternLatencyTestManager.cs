using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class TactilePatternLatencyTestManager : MonoBehaviour
{
    private enum Finger
    {
        THUMB,
        INDEX
    }

    private class EncodedPattern
    {
        public int Value1;
        public int Value2;
    }

    private class PendingMessage
    {
        public int Id;
        public string PatternName;
        public int TrialIndex;
        public int FrameIndex;
        public string Payload;
        public long SentTicks;
        public float SendTimeSeconds;
    }

    private class LatencyResult
    {
        public int Id;
        public string PatternName;
        public int TrialIndex;
        public int FrameIndex;
        public string Payload;
        public float RoundTripMs;
        public float EstimatedOneWayMs;
        public bool Received;
    }

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    [Header("Test Settings")]
    [SerializeField] private int trialsPerPattern = 10;
    [SerializeField] private float frameIntervalSeconds = 0.15f;
    [SerializeField] private float delayBetweenTrialsSeconds = 0.5f;
    [SerializeField] private float ackTimeoutSeconds = 1.0f;

    [Header("Pattern Names From Resources/BraillePatternSequences")]
    [SerializeField]
    private List<string> patternNames = new()
    {
        "BasicPulse",
        "WaveOut",
        "WaveIn",
        "Ripple",
        "RotateCircle"
    };

    [Header("Controls")]
    [SerializeField] private KeyCode startKey = KeyCode.L;
    [SerializeField] private KeyCode exportKey = KeyCode.K;

    private readonly Stopwatch stopwatch = new();
    private readonly Dictionary<int, PendingMessage> pendingMessages = new();
    private readonly List<LatencyResult> results = new();

    private bool isRunning = false;

    private int nextMessageId = 0;
    private int currentPatternIndex = 0;
    private int currentTrialIndex = 0;
    private int currentFrameIndex = 0;
    private string currentPatternName = "";

    private Coroutine testCoroutine;

    private void Start()
    {
        stopwatch.Start();
        UpdateStatusText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(startKey) && !isRunning)
        {
            StartTest();
        }

        if (Input.GetKeyDown(exportKey))
        {
            ExportCsv();
        }

        CheckTimeouts();
        UpdateStatusText();
    }

    private void StartTest()
    {
        if (InputDeviceManager.Instance == null ||
            InputDeviceManager.Instance.BLEDevice == null ||
            !InputDeviceManager.Instance.BLEDevice.IsConnected)
        {
            Debug.LogWarning("TactilePatternLatencyTestManager: BLE is not connected.");
            return;
        }

        if (testCoroutine != null)
        {
            StopCoroutine(testCoroutine);
            testCoroutine = null;
        }

        pendingMessages.Clear();
        results.Clear();

        nextMessageId = 0;
        currentPatternIndex = 0;
        currentTrialIndex = 0;
        currentFrameIndex = 0;
        currentPatternName = "";

        isRunning = true;
        testCoroutine = StartCoroutine(RunPatternLatencyTest());

        Debug.Log("Tactile pattern latency test started.");
    }

    private IEnumerator RunPatternLatencyTest()
    {
        for (int p = 0; p < patternNames.Count; p++)
        {
            currentPatternIndex = p;
            currentPatternName = patternNames[p];

            BraillePinPatternSequence sequence =
                Resources.Load<BraillePinPatternSequence>(
                    $"BraillePatternSequences/{currentPatternName}"
                );

            if (sequence == null)
            {
                Debug.LogWarning($"Pattern sequence not found: {currentPatternName}");
                continue;
            }

            if (sequence.Sequence == null || sequence.Sequence.Count == 0)
            {
                Debug.LogWarning($"Pattern sequence is empty: {currentPatternName}");
                continue;
            }

            for (int trial = 1; trial <= trialsPerPattern; trial++)
            {
                currentTrialIndex = trial;

                for (int frame = 0; frame < sequence.Sequence.Count; frame++)
                {
                    currentFrameIndex = frame;

                    string payload = ConvertPatternToPayload(sequence.Sequence[frame]);

                    SendTrackedPatternPayload(
                        currentPatternName,
                        trial,
                        frame,
                        payload
                    );

                    yield return new WaitForSecondsRealtime(frameIntervalSeconds);
                }

                yield return new WaitForSecondsRealtime(delayBetweenTrialsSeconds);
            }
        }

        isRunning = false;
        testCoroutine = null;

        Debug.Log("Tactile pattern latency test complete. Press K to export.");
    }

    private string ConvertPatternToPayload(BraillePinPatternSequence.BraillePattern pattern)
    {
        EncodedPattern thumb = EncodePattern(pattern, Finger.THUMB);
        EncodedPattern index = EncodePattern(pattern, Finger.INDEX);

        return $"{thumb.Value1:D3}{thumb.Value2:D3}{index.Value1:D3}{index.Value2:D3}";
    }

    private EncodedPattern EncodePattern(
        BraillePinPatternSequence.BraillePattern pattern,
        Finger finger
    )
    {
        string row1 = NormalizeRow(pattern.Row1);
        string row2 = NormalizeRow(pattern.Row2);
        string row3 = NormalizeRow(pattern.Row3);
        string row4 = NormalizeRow(pattern.Row4);

        int[,] v = new int[4, 4];

        for (int i = 0; i < 4; i++)
        {
            v[0, i] = row1[i] == '0' ? 0 : 1;
            v[1, i] = row2[i] == '0' ? 0 : 1;
            v[2, i] = row3[i] == '0' ? 0 : 1;
            v[3, i] = row4[i] == '0' ? 0 : 1;
        }

        /*
         * This mapping matches the normal BraillePatternPlayer encoding.
         */
        if (finger == Finger.THUMB)
        {
            return new EncodedPattern
            {
                Value1 =
                    1 * v[0, 0] + 8 * v[0, 1] +
                    2 * v[1, 0] + 16 * v[1, 1] +
                    4 * v[2, 0] + 32 * v[2, 1] +
                    64 * v[3, 0] + 128 * v[3, 1],

                Value2 =
                    1 * v[0, 2] + 8 * v[0, 3] +
                    2 * v[1, 2] + 16 * v[1, 3] +
                    4 * v[2, 2] + 32 * v[2, 3] +
                    64 * v[3, 2] + 128 * v[3, 3],
            };
        }

        return new EncodedPattern
        {
            Value1 =
                64 * v[0, 0] + 4 * v[0, 1] + 2 * v[0, 2] + v[0, 3] +
                128 * v[1, 0] + 32 * v[1, 1] + 16 * v[1, 2] + 8 * v[1, 3],

            Value2 =
                64 * v[2, 0] + 4 * v[2, 1] + 2 * v[2, 2] + v[2, 3] +
                128 * v[3, 0] + 32 * v[3, 1] + 16 * v[3, 2] + 8 * v[3, 3],
        };
    }

    private string NormalizeRow(string row)
    {
        if (string.IsNullOrWhiteSpace(row))
        {
            return "0000";
        }

        row = row.Trim();

        if (row.Length >= 4)
        {
            return row.Substring(0, 4);
        }

        return row.PadRight(4, '0');
    }

    private void SendTrackedPatternPayload(
        string patternName,
        int trialIndex,
        int frameIndex,
        string payload
    )
    {
        int id = nextMessageId++;
        string message = $"<{payload}#{id:D3}>";

        /*
         * Register before sending.
         * Some BLE plugin builds return false even when the packet is actually sent.
         * The Arduino ACK is the real confirmation.
         */
        pendingMessages[id] = new PendingMessage
        {
            Id = id,
            PatternName = patternName,
            TrialIndex = trialIndex,
            FrameIndex = frameIndex,
            Payload = payload,
            SentTicks = stopwatch.ElapsedTicks,
            SendTimeSeconds = Time.realtimeSinceStartup
        };

        byte[] bytes = Encoding.ASCII.GetBytes(message);

        BleApi.BLEData bleData = new()
        {
            buf = new byte[512],
            size = (short)bytes.Length,
            deviceId = InputDeviceManager.Instance.BLEDevice.ConnectedDeviceID,
            serviceUuid = BLEDevice.BRAILLE_SERVICE_UUID,
            characteristicUuid = BLEDevice.BRAILLE_CHARACTERISTIC_UUID
        };

        System.Array.Copy(bytes, bleData.buf, bytes.Length);

        bool success = BleApi.SendData(in bleData, false);

        if (!success)
        {
            BleApi.GetError(out BleApi.ErrorMessage error);

            Debug.LogWarning(
                $"SendData returned false for {message}: {error.msg}. " +
                $"Waiting for ACK to confirm delivery."
            );
        }

        Debug.Log(
            $"PatternLatency Sent ID={id:D3}, Pattern={patternName}, " +
            $"Trial={trialIndex}, Frame={frameIndex}, Payload={message}"
        );
    }

    public void OnLatencyAckReceived(string message)
    {
        if (!message.StartsWith("ACK"))
        {
            return;
        }

        string idText = message.Substring(3);

        if (!int.TryParse(idText, out int id))
        {
            Debug.LogWarning($"PatternLatency: Invalid ACK format: {message}");
            return;
        }

        if (!pendingMessages.TryGetValue(id, out PendingMessage pending))
        {
            return;
        }

        long receivedTicks = stopwatch.ElapsedTicks;
        pendingMessages.Remove(id);

        float roundTripMs =
            (receivedTicks - pending.SentTicks) * 1000f / Stopwatch.Frequency;

        results.Add(new LatencyResult
        {
            Id = id,
            PatternName = pending.PatternName,
            TrialIndex = pending.TrialIndex,
            FrameIndex = pending.FrameIndex,
            Payload = pending.Payload,
            RoundTripMs = roundTripMs,
            EstimatedOneWayMs = roundTripMs / 2f,
            Received = true
        });

        Debug.Log(
            $"PatternLatency ACK ID={id:D3}, Pattern={pending.PatternName}, " +
            $"Trial={pending.TrialIndex}, Frame={pending.FrameIndex}, RTT={roundTripMs:F2} ms"
        );
    }

    private void CheckTimeouts()
    {
        if (pendingMessages.Count == 0)
        {
            return;
        }

        List<int> timedOutIds = new();

        foreach (var pair in pendingMessages)
        {
            PendingMessage pending = pair.Value;

            if (Time.realtimeSinceStartup - pending.SendTimeSeconds >= ackTimeoutSeconds)
            {
                timedOutIds.Add(pair.Key);
            }
        }

        foreach (int id in timedOutIds)
        {
            PendingMessage pending = pendingMessages[id];
            pendingMessages.Remove(id);

            results.Add(new LatencyResult
            {
                Id = id,
                PatternName = pending.PatternName,
                TrialIndex = pending.TrialIndex,
                FrameIndex = pending.FrameIndex,
                Payload = pending.Payload,
                RoundTripMs = -1f,
                EstimatedOneWayMs = -1f,
                Received = false
            });

            Debug.LogWarning(
                $"PatternLatency ACK timeout for ID={id:D3}, " +
                $"Pattern={pending.PatternName}, Trial={pending.TrialIndex}, Frame={pending.FrameIndex}"
            );
        }
    }

    private void ExportCsv()
    {
        if (results.Count == 0)
        {
            Debug.LogWarning("No tactile pattern latency results to export.");
            return;
        }

        ExportDetailedCsv();
        ExportSummaryCsv();
    }

    private void ExportDetailedCsv()
    {
        StringBuilder csv = new();

        csv.AppendLine(
            "Id,PatternName,TrialIndex,FrameIndex,Payload,RoundTripLatencyMs,EstimatedOneWayLatencyMs,Received"
        );

        foreach (LatencyResult result in results)
        {
            csv.AppendLine(
                $"{result.Id}," +
                $"{EscapeCsv(result.PatternName)}," +
                $"{result.TrialIndex}," +
                $"{result.FrameIndex}," +
                $"{result.Payload}," +
                $"{result.RoundTripMs:F4}," +
                $"{result.EstimatedOneWayMs:F4}," +
                $"{result.Received}"
            );
        }

        string path = Path.Combine(
            Application.persistentDataPath,
            $"TactilePatternLatency_Detailed_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv"
        );

        File.WriteAllText(path, csv.ToString());
        Debug.Log($"Tactile pattern latency detailed CSV exported to: {path}");
    }

    private void ExportSummaryCsv()
    {
        StringBuilder csv = new();

        csv.AppendLine(
            "PatternName,MessagesReceived,MessagesTotal,PacketLossRate,MeanRoundTripMs,MedianRoundTripMs,MinRoundTripMs,MaxRoundTripMs,StdDevRoundTripMs,MeanEstimatedOneWayMs"
        );

        foreach (string patternName in patternNames)
        {
            List<float> rtts = new();
            int total = 0;
            int received = 0;

            foreach (LatencyResult result in results)
            {
                if (result.PatternName != patternName)
                {
                    continue;
                }

                total++;

                if (result.Received)
                {
                    received++;
                    rtts.Add(result.RoundTripMs);
                }
            }

            float packetLossRate = total > 0 ? 1f - ((float)received / total) : 0f;

            float mean = 0f;
            float median = 0f;
            float min = 0f;
            float max = 0f;
            float stdDev = 0f;

            if (rtts.Count > 0)
            {
                rtts.Sort();

                min = rtts[0];
                max = rtts[^1];

                foreach (float rtt in rtts)
                {
                    mean += rtt;
                }

                mean /= rtts.Count;

                int mid = rtts.Count / 2;

                if (rtts.Count % 2 == 0)
                {
                    median = (rtts[mid - 1] + rtts[mid]) / 2f;
                }
                else
                {
                    median = rtts[mid];
                }

                float variance = 0f;

                foreach (float rtt in rtts)
                {
                    float diff = rtt - mean;
                    variance += diff * diff;
                }

                variance /= rtts.Count;
                stdDev = Mathf.Sqrt(variance);
            }

            csv.AppendLine(
                $"{patternName}," +
                $"{received}," +
                $"{total}," +
                $"{packetLossRate:F4}," +
                $"{mean:F4}," +
                $"{median:F4}," +
                $"{min:F4}," +
                $"{max:F4}," +
                $"{stdDev:F4}," +
                $"{mean / 2f:F4}"
            );
        }

        string path = Path.Combine(
            Application.persistentDataPath,
            $"TactilePatternLatency_Summary_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv"
        );

        File.WriteAllText(path, csv.ToString());
        Debug.Log($"Tactile pattern latency summary CSV exported to: {path}");
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text =
            $"TACTILE PATTERN LATENCY TEST\n\n" +
            $"Running: {isRunning}\n" +
            $"Current Pattern: {currentPatternName}\n" +
            $"Pattern: {currentPatternIndex + 1}/{patternNames.Count}\n" +
            $"Trial: {currentTrialIndex}/{trialsPerPattern}\n" +
            $"Frame: {currentFrameIndex}\n" +
            $"Sent Messages: {nextMessageId}\n" +
            $"Received ACKs: {CountReceivedResults()}\n" +
            $"Pending: {pendingMessages.Count}\n\n" +
            $"Press L to start.\n" +
            $"Press K to export.";
    }

    private int CountReceivedResults()
    {
        int count = 0;

        foreach (LatencyResult result in results)
        {
            if (result.Received)
            {
                count++;
            }
        }

        return count;
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