using System.Collections;
using UnityEngine;

/// <summary>
/// Exposes IMU rotation data
/// </summary>
public class IMUInput : MonoBehaviour
{
    public enum RotationAxis { x, y, z }

    [Header("Debugging")]
    [SerializeField] private bool showIMUData = false;

    /// <summary>~
    /// Returns IMU rotation in ~~degrees~~
    /// </summary>
    public Vector3 Rotation { get; private set; } = Vector3.zero;

    /// <summary>
    /// Invoked when new IMU data is received.
    /// </summary>
    public event System.Action<BleApi.BLEData> DataReceived;

    /// <summary>
    /// Begins the process of reading IMU data.
    /// </summary>
    public void StartReadingIMUData(string characteristicUUID)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() => StartCoroutine(ReadIMUData(characteristicUUID)));
    }

    private IEnumerator ReadIMUData(string characteristicUuid)
    {
        while (true)
        {
            bool hasData = BleApi.PollData(out BleApi.BLEData data, false);

            if (hasData && data.characteristicUuid.ToLower().Contains(characteristicUuid.ToLower()))
            {
                //Debug.Log(data.buf);
                if (data.size == 1)
                {
                    if (showIMUData) Debug.Log($"Ping response from Arduino: {data.buf[0]}");
                    DataReceived.Invoke(data);
                }
                else if (data.size >= 6)
                {
                    short x = System.BitConverter.ToInt16(data.buf, 0);
                    short y = System.BitConverter.ToInt16(data.buf, 2);
                    short z = System.BitConverter.ToInt16(data.buf, 4);

                    // Do calcs
                    Rotation = new Vector3(-x / 1000f, -y / 1000f, -z / 1000f);
                    DataReceived.Invoke(data);

                    if (showIMUData) Debug.Log($"Processed IMU Rotation: {Rotation}");
                }
            }

            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
}
