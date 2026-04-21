using System.Collections;
using Cinemachine;
using UnityEngine;

/// <summary>
/// Handles camera views
/// </summary>
public class CameraController : MonoBehaviour
{
    public enum CameraView
    {
        Gameplay,
        Menu,
        FishSelect,
        BaitPrep
    }

    public static CameraController Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera _gameplayVCam;
    [SerializeField] private CinemachineVirtualCamera _menuVCam;
    [SerializeField] private CinemachineVirtualCamera _fishSelectVCam;
    [SerializeField] private CinemachineVirtualCamera _baitPrepVCam;

    [Header("Camera Pulse")]
    [SerializeField] private float _defaultPulseAmplitude = 1.2f;
    [SerializeField] private float _defaultPulseFrequency = 2.2f;
    [SerializeField] private float _defaultPulseDuration = 0.2f;

    [Header("Bite Focus")]
    [SerializeField] private float _biteFocusHeightOffset = 0.2f;
    [SerializeField] [Range(0f, 1f)] private float _biteFocusTowardFish = 0.75f;

    private int _previousMenuPriority = 0;
    private Coroutine _pulseRoutine;
    private Coroutine _zoomRoutine;
    private Coroutine _biteFocusRoutine;

    private float _gameplayDefaultFov;
    private float _menuDefaultFov;
    private float _fishSelectDefaultFov;
    private float _baitPrepDefaultFov;

    private Transform _originalFishSelectLookAt;

    private GameObject _biteFocusTargetObject;
    private Transform _biteFocusTarget;

    public CinemachineVirtualCamera FishSelectVCam => _fishSelectVCam;

    private void Awake()
    {
        Instance = this;

        if (_gameplayVCam != null)
            _gameplayDefaultFov = _gameplayVCam.m_Lens.FieldOfView;

        if (_menuVCam != null)
            _menuDefaultFov = _menuVCam.m_Lens.FieldOfView;

        if (_fishSelectVCam != null)
            _fishSelectDefaultFov = _fishSelectVCam.m_Lens.FieldOfView;

        if (_baitPrepVCam != null)
            _baitPrepDefaultFov = _baitPrepVCam.m_Lens.FieldOfView;
    }

    public void SetPriorityMenuView(bool enable)
    {
        if (_menuVCam == null)
            return;

        if (enable)
        {
            _previousMenuPriority = _menuVCam.Priority;
            _menuVCam.Priority = 10;
        }
        else
        {
            _menuVCam.Priority = _previousMenuPriority;
        }
    }

    public void SetCameraView(CameraView view)
    {
        if (_gameplayVCam != null) _gameplayVCam.Priority = 0;
        if (_menuVCam != null) _menuVCam.Priority = 0;
        if (_fishSelectVCam != null) _fishSelectVCam.Priority = 0;
        if (_baitPrepVCam != null) _baitPrepVCam.Priority = 0;

        switch (view)
        {
            case CameraView.Gameplay:
                if (_gameplayVCam != null) _gameplayVCam.Priority = 5;
                break;

            case CameraView.Menu:
                if (_menuVCam != null) _menuVCam.Priority = 5;
                break;

            case CameraView.FishSelect:
                if (_fishSelectVCam != null) _fishSelectVCam.Priority = 5;
                break;

            case CameraView.BaitPrep:
                if (_baitPrepVCam != null) _baitPrepVCam.Priority = 5;
                break;
        }
    }

    public void PulseCurrentCamera()
    {
        PulseCurrentCamera(_defaultPulseAmplitude, _defaultPulseFrequency, _defaultPulseDuration);
    }

    public void PulseCurrentCamera(float amplitude, float frequency, float duration)
    {
        CinemachineVirtualCamera activeCamera = GetActiveCamera();
        if (activeCamera == null)
            return;

        CinemachineBasicMultiChannelPerlin noise =
            activeCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogWarning($"Camera {activeCamera.name} does not have CinemachineBasicMultiChannelPerlin.");
            return;
        }

        if (_pulseRoutine != null)
            StopCoroutine(_pulseRoutine);

        _pulseRoutine = StartCoroutine(PulseCameraRoutine(noise, amplitude, frequency, duration));
    }

    public void ZoomFishSelectCamera(float targetFov, float duration)
    {
        if (_fishSelectVCam == null)
            return;

        StartZoomRoutine(_fishSelectVCam, targetFov, duration);
    }

    public void ResetFishSelectCameraZoom(float duration)
    {
        if (_fishSelectVCam == null)
            return;

        StartZoomRoutine(_fishSelectVCam, _fishSelectDefaultFov, duration);
    }

    /// <summary>
    /// Re-aims the fish-select camera toward the bite area without moving the camera itself.
    /// Only LookAt is overridden. Follow remains untouched.
    /// </summary>
    public void StartFishBiteFocus(Transform bobber, Transform fish, float targetFov, float duration)
    {
        if (_fishSelectVCam == null || bobber == null || fish == null)
            return;

        if (_biteFocusTargetObject == null)
        {
            _biteFocusTargetObject = new GameObject("FishBiteCameraFocusTarget");
            _biteFocusTarget = _biteFocusTargetObject.transform;
        }

        if (_biteFocusRoutine != null)
            StopCoroutine(_biteFocusRoutine);

        _originalFishSelectLookAt = _fishSelectVCam.LookAt;
        _fishSelectVCam.LookAt = _biteFocusTarget;

        _biteFocusRoutine = StartCoroutine(UpdateBiteFocusTargetRoutine(bobber, fish));

        StartZoomRoutine(_fishSelectVCam, targetFov, duration);
    }

    public void StopFishBiteFocus(float resetZoomDuration)
    {
        if (_biteFocusRoutine != null)
        {
            StopCoroutine(_biteFocusRoutine);
            _biteFocusRoutine = null;
        }

        if (_fishSelectVCam != null)
        {
            _fishSelectVCam.LookAt = _originalFishSelectLookAt;
        }

        ResetFishSelectCameraZoom(resetZoomDuration);
    }

    private IEnumerator UpdateBiteFocusTargetRoutine(Transform bobber, Transform fish)
    {
        while (bobber != null && fish != null)
        {
            Vector3 focusPoint = Vector3.Lerp(bobber.position, fish.position, _biteFocusTowardFish);
            focusPoint.y += _biteFocusHeightOffset;

            _biteFocusTarget.position = focusPoint;
            yield return null;
        }

        _biteFocusRoutine = null;
    }

    private void StartZoomRoutine(CinemachineVirtualCamera targetCamera, float targetFov, float duration)
    {
        if (_zoomRoutine != null)
            StopCoroutine(_zoomRoutine);

        _zoomRoutine = StartCoroutine(ZoomRoutine(targetCamera, targetFov, duration));
    }

    private IEnumerator ZoomRoutine(CinemachineVirtualCamera targetCamera, float targetFov, float duration)
    {
        float velocity = 0f;
        float currentFov = targetCamera.m_Lens.FieldOfView;

        while (Mathf.Abs(currentFov - targetFov) > 0.01f)
        {
            currentFov = Mathf.SmoothDamp(
                currentFov,
                targetFov,
                ref velocity,
                duration
            );

            targetCamera.m_Lens.FieldOfView = currentFov;
            yield return null;
        }

        targetCamera.m_Lens.FieldOfView = targetFov;
        _zoomRoutine = null;
    }

    private CinemachineVirtualCamera GetActiveCamera()
    {
        CinemachineVirtualCamera activeCamera = null;
        int highestPriority = int.MinValue;

        if (_gameplayVCam != null && _gameplayVCam.Priority > highestPriority)
        {
            activeCamera = _gameplayVCam;
            highestPriority = _gameplayVCam.Priority;
        }

        if (_menuVCam != null && _menuVCam.Priority > highestPriority)
        {
            activeCamera = _menuVCam;
            highestPriority = _menuVCam.Priority;
        }

        if (_fishSelectVCam != null && _fishSelectVCam.Priority > highestPriority)
        {
            activeCamera = _fishSelectVCam;
            highestPriority = _fishSelectVCam.Priority;
        }

        if (_baitPrepVCam != null && _baitPrepVCam.Priority > highestPriority)
        {
            activeCamera = _baitPrepVCam;
            highestPriority = _baitPrepVCam.Priority;
        }

        return activeCamera;
    }

    private IEnumerator PulseCameraRoutine(
        CinemachineBasicMultiChannelPerlin noise,
        float amplitude,
        float frequency,
        float duration)
    {
        float originalAmplitude = noise.m_AmplitudeGain;
        float originalFrequency = noise.m_FrequencyGain;

        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            noise.m_AmplitudeGain = Mathf.Lerp(amplitude, originalAmplitude, t);
            noise.m_FrequencyGain = Mathf.Lerp(frequency, originalFrequency, t);

            yield return null;
        }

        noise.m_AmplitudeGain = originalAmplitude;
        noise.m_FrequencyGain = originalFrequency;

        _pulseRoutine = null;
    }
}