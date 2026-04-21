using Cinemachine;
using UnityEngine;

public class CameraImpulseManager : MonoBehaviour
{
    public static CameraImpulseManager Instance { get; private set; }

    [SerializeField] private CinemachineImpulseSource _impulseSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerImpulse()
    {
        if (_impulseSource == null)
        {
            Debug.LogWarning("CameraImpulseManager: No impulse source assigned.");
            return;
        }

        _impulseSource.GenerateImpulse();
    }

    public void TriggerImpulse(Vector3 velocity)
    {
        if (_impulseSource == null)
        {
            Debug.LogWarning("CameraImpulseManager: No impulse source assigned.");
            return;
        }

        _impulseSource.GenerateImpulse(velocity);
    }
}