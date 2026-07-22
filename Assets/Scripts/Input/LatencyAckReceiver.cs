using System.Collections;
using System.Text;
using UnityEngine;

public class LatencyAckReceiver : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool showLatencyAckData = false;

    private string _latencyCharacteristicUuid;

    public void StartReadingLatencyAckData(string characteristicUUID)
    {
        _latencyCharacteristicUuid = characteristicUUID;
        UnityMainThreadDispatcher.Instance.Enqueue(() => StartCoroutine(ReadLatencyAckData()));
    }

    private IEnumerator ReadLatencyAckData()
    {
        while (true)
        {
            bool hasData = BleApi.PollData(out BleApi.BLEData data, false);

            if (hasData &&
                data.characteristicUuid.ToLower().Contains(_latencyCharacteristicUuid.ToLower()))
            {
                if (data.size > 0)
                {
                    string message = Encoding.ASCII.GetString(data.buf, 0, data.size);

                    if (showLatencyAckData)
                    {
                        Debug.Log($"Latency ACK received raw: {message}");
                    }

                    // Existing simple command latency manager, if present.
                    InputCommunicationLatencyTestManager commandLatencyManager =
                        FindObjectOfType<InputCommunicationLatencyTestManager>();

                    if (commandLatencyManager != null)
                    {
                        commandLatencyManager.OnLatencyAckReceived(message);
                    }

                    // New tactile pattern latency manager.
                    TactilePatternLatencyTestManager patternLatencyManager =
                        FindObjectOfType<TactilePatternLatencyTestManager>();

                    if (patternLatencyManager != null)
                    {
                        patternLatencyManager.OnLatencyAckReceived(message);
                    }

                    if (commandLatencyManager == null && patternLatencyManager == null)
                    {
                        Debug.LogWarning("LatencyAckReceiver: No latency test manager found.");
                    }
                }
            }

            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
}