using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;

public class InputCommunicationLatencyTestManager : MonoBehaviour
{
    private class LatencyResult
    {
        public int Id;
        public float RoundTripMs;
        public float EstimatedOneWayMs;
        public bool Received;
    }

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    [Header("Test Settings")]
    [SerializeField] private int commandCount = 100;
    [SerializeField] private float commandIntervalSeconds = 0.2f;

    [Header("Controls")]
    [SerializeField] private KeyCode startKey = KeyCode.L;
    [SerializeField] private KeyCode exportKey = KeyCode.K;

    private readonly Stopwatch stopwatch = new();
    private readonly Dictionary<int, long> pendingCommands = new();
    private readonly List<LatencyResult> results = new();

    private bool isRunning = false;
    private int nextCommandId = 0;
    private float nextCommandTimer = 0f;

    private LatencyCommandOutput latencyCommandOutput;

    private void Awake()
    {
        latencyCommandOutput = GetComponent<LatencyCommandOutput>();

        if (latencyCommandOutput == null)
        {
            latencyCommandOutput = FindObjectOfType<LatencyCommandOutput>();
        }
    }

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

        if (!isRunning)
        {
            UpdateStatusText();
            return;
        }

        nextCommandTimer -= Time.deltaTime;

        if (nextCommandTimer <= 0f && nextCommandId < commandCount)
        {
            SendLatencyCommand(nextCommandId);
            nextCommandId++;
            nextCommandTimer = commandIntervalSeconds;
        }

        if (nextCommandId >= commandCount && pendingCommands.Count == 0)
        {
            isRunning = false;
            UnityEngine.Debug.Log("Input communication latency test complete. Press E to export.");
        }

        UpdateStatusText();
    }

    private void StartTest()
    {
        if (latencyCommandOutput == null)
        {
            UnityEngine.Debug.LogError("InputCommunicationLatencyTestManager: LatencyCommandOutput is missing.");
            return;
        }

        if (InputDeviceManager.Instance == null ||
            InputDeviceManager.Instance.BLEDevice == null ||
            !InputDeviceManager.Instance.BLEDevice.IsConnected)
        {
            UnityEngine.Debug.LogWarning("InputCommunicationLatencyTestManager: BLE is not connected.");
            return;
        }

        pendingCommands.Clear();
        results.Clear();

        nextCommandId = 0;
        nextCommandTimer = 0f;
        isRunning = true;

        UnityEngine.Debug.Log("Input communication latency test started.");
    }

    private void SendLatencyCommand(int id)
    {
        pendingCommands[id] = stopwatch.ElapsedTicks;

        latencyCommandOutput.SendLatencyCommand(id);

        UnityEngine.Debug.Log($"Latency command sent: ID={id:D3}");
    }

    public void OnLatencyAckReceived(string message)
    {
        // Expected format: ACK000, ACK001, etc.
        if (!message.StartsWith("ACK"))
        {
            return;
        }

        string idText = message.Substring(3);

        if (!int.TryParse(idText, out int id))
        {
            UnityEngine.Debug.LogWarning($"Invalid latency ACK format: {message}");
            return;
        }

        if (!pendingCommands.TryGetValue(id, out long sentTicks))
        {
            // ACK from an old test or duplicate ACK.
            return;
        }

        long receivedTicks = stopwatch.ElapsedTicks;
        pendingCommands.Remove(id);

        float roundTripMs = (receivedTicks - sentTicks) * 1000f / Stopwatch.Frequency;

        results.Add(new LatencyResult
        {
            Id = id,
            RoundTripMs = roundTripMs,
            EstimatedOneWayMs = roundTripMs / 2f,
            Received = true
        });

        UnityEngine.Debug.Log(
            $"Latency ACK received: ID={id:D3}, RTT={roundTripMs:F2} ms, OneWay≈{roundTripMs / 2f:F2} ms"
        );
    }

    private void ExportCsv()
    {
        if (results.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No latency results to export.");
            return;
        }

        StringBuilder csv = new();

        csv.AppendLine("Id,RoundTripLatencyMs,EstimatedOneWayLatencyMs,Received");

        foreach (var result in results)
        {
            csv.AppendLine(
                $"{result.Id}," +
                $"{result.RoundTripMs:F4}," +
                $"{result.EstimatedOneWayMs:F4}," +
                $"{result.Received}"
            );
        }

        string path = Path.Combine(
            Application.persistentDataPath,
            $"InputCommunicationLatency_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv"
        );

        File.WriteAllText(path, csv.ToString());

        UnityEngine.Debug.Log($"Input communication latency CSV exported to: {path}");
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        float latestRtt = results.Count > 0 ? results[^1].RoundTripMs : 0f;

        statusText.text =
            $"INPUT COMMUNICATION LATENCY TEST\n\n" +
            $"Running: {isRunning}\n" +
            $"Sent: {nextCommandId}/{commandCount}\n" +
            $"Received: {results.Count}/{commandCount}\n" +
            $"Pending: {pendingCommands.Count}\n" +
            $"Latest RTT: {latestRtt:F2} ms\n" +
            $"Latest One-way Estimate: {latestRtt / 2f:F2} ms\n\n" +
            $"Press L to start.\n" +
            $"Press E to export.";
    }
}