using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamic Video Panel for Input prompts.
/// Can be main or secondary input prompt, and depends on UIManager prompt events.
/// Supports:
/// - video + text prompt display
/// - optional radial progress ring
/// - repeating pulse feedback (fast outward / slow inward)
/// - quick full-frame shine on successful completion
/// </summary>
public class InputPromptPanel : DynamicVideoPanel
{
    public enum InputPromptType
    {
        Main,
        Secondary
    }

    public static InputPromptPanel MainInstance { get; private set; }
    public static InputPromptPanel SecondaryInstance { get; private set; }

    [Header("Prompt Content")]
    [SerializeField] private TextMeshProUGUI _message;
    [SerializeField] private InputPromptType _inputPromptType = InputPromptType.Main;

    [Header("Progress Ring")]
    [SerializeField] private GameObject _progressRoot;
    [SerializeField] private Image _progressFill;

    [Header("Pulse Rings")]
    [SerializeField] private Image[] _pulseRings;

    [Header("Fast Outward Pulse")]
    [SerializeField] private Color _fastPulseColor = new Color(1f, 0.55f, 0.7f, 0.9f);
    [SerializeField] private float _fastPulseStartScale = 1.0f;
    [SerializeField] private float _fastPulseEndScale = 1.55f;
    [SerializeField] private float _fastPulseDuration = 0.65f;
    [SerializeField] private float _fastPulseRingDelay = 0.24f;

    [Header("Slow Inward Pulse")]
    [SerializeField] private Color _slowPulseColor = new Color(0.35f, 1f, 1f, 0.85f);
    [SerializeField] private float _slowPulseStartScale = 1.85f;
    [SerializeField] private float _slowPulseEndScale = 1.0f;
    [SerializeField] private float _slowPulseDuration = 1.00f;
    [SerializeField] private float _slowPulseRingDelay = 0.32f;

    [Header("Pulse Loop Timing")]
    [SerializeField] private float _fastPulseLoopPause = 0.35f;
    [SerializeField] private float _slowPulseLoopPause = 0.50f;

    [Header("Completion Shine")]
    [SerializeField] private Image _shineOverlay;
    [SerializeField] private float _shineFlashAlpha = 0.9f;
    [SerializeField] private float _shineFlashDuration = 0.10f;

    [Header("Progress Colors")]
    [SerializeField] private Color _normalProgressColor = Color.white;
    [SerializeField] private Color _completeFlashColor = new Color(1f, 1f, 1f, 1f);

    private Coroutine _pulseRoutine;
    private Coroutine _shineRoutine;
    private InputPrompt.PromptPulseType _currentPulseType = InputPrompt.PromptPulseType.None;

    private int _pulseSequenceId = 0;

    private void Awake()
    {
        if (_inputPromptType == InputPromptType.Main)
            MainInstance = this;
        else
            SecondaryInstance = this;

        UIManager.Instance.MainInputPromptShown.AddListener(OnMainInputPromptShown);
        UIManager.Instance.SecondInputPromptShown.AddListener(OnSecondInputPromptShown);

        ShowProgress(false);
        ResetProgress();
        SetShineAlpha(0f);
        ResetPulseRings();
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.MainInputPromptShown.RemoveListener(OnMainInputPromptShown);
            UIManager.Instance.SecondInputPromptShown.RemoveListener(OnSecondInputPromptShown);
        }

        if (MainInstance == this)
            MainInstance = null;

        if (SecondaryInstance == this)
            SecondaryInstance = null;

        StopPulseLoop();
    }

    private void OnMainInputPromptShown(InputPrompt inputPrompt)
    {
        if (_inputPromptType == InputPromptType.Main)
            SetInputPrompt(inputPrompt);
    }

    private void OnSecondInputPromptShown(InputPrompt inputPrompt)
    {
        if (_inputPromptType == InputPromptType.Secondary)
            SetInputPrompt(inputPrompt);
    }

    /// <summary>
    /// Sets the video and message for the input prompt. Hides the panel if inputPrompt is null.
    /// </summary>
    public void SetInputPrompt(InputPrompt inputPrompt)
    {
        if (inputPrompt == null)
        {
            Show(false);
            ShowProgress(false);
            ResetProgress();

            _currentPulseType = InputPrompt.PromptPulseType.None;
            StopPulseLoop();
            ResetPulseRings();
            return;
        }

        StopPulseLoop();
        ResetPulseRings();

        Show(true);
        SetVideo(inputPrompt.Video);
        _message.text = inputPrompt.Message;

        _currentPulseType = inputPrompt.PulseType;

        ShowProgress(inputPrompt.UseProgress);

        if (!inputPrompt.UseProgress)
            ResetProgress();

        PlayPromptPulse();
    }

    public void ShowProgress(bool show)
    {
        if (_progressRoot != null)
            _progressRoot.SetActive(show);

        if (!show)
            ResetPulseRings();
    }

    /// <summary>
    /// Sets radial progress from 0 to 1.
    /// Stops repeating pulses and flashes the frame when progress completes.
    /// </summary>
    public void SetProgress(float value)
    {
        if (_progressFill == null)
            return;

        float clamped = Mathf.Clamp01(value);
        _progressFill.fillAmount = clamped;

        if (clamped >= 1f)
        {
            _progressFill.color = _completeFlashColor;
            StopPulseLoop();
            ResetPulseRings();
            PlayShineFlash();
        }
        else
        {
            _progressFill.color = _normalProgressColor;
        }
    }

    public void ResetProgress()
    {
        if (_progressFill == null)
            return;

        _progressFill.fillAmount = 0f;
        _progressFill.color = _normalProgressColor;
    }

    public void PlayPromptPulse()
    {
        if (_currentPulseType == InputPrompt.PromptPulseType.None)
        {
            StopPulseLoop();
            ResetPulseRings();
            return;
        }

        StopPulseLoop();
        _pulseRoutine = StartCoroutine(PulseLoopRoutine(++_pulseSequenceId));
    }

    private IEnumerator PulseLoopRoutine(int sequenceId)
    {
        while (sequenceId == _pulseSequenceId)
        {
            yield return StartCoroutine(PlayPulseRoutine(_currentPulseType, sequenceId));

            if (sequenceId != _pulseSequenceId)
                yield break;

            float pauseBetweenLoops = GetPauseBetweenLoops(_currentPulseType);
            yield return new WaitForSecondsRealtime(pauseBetweenLoops);
        }
    }

    private float GetPauseBetweenLoops(InputPrompt.PromptPulseType pulseType)
    {
        switch (pulseType)
        {
            case InputPrompt.PromptPulseType.FastOutward:
                return _fastPulseLoopPause;

            case InputPrompt.PromptPulseType.SlowInward:
                return _slowPulseLoopPause;

            default:
                return 0.25f;
        }
    }

    private IEnumerator PlayPulseRoutine(InputPrompt.PromptPulseType pulseType, int sequenceId)
    {
        ResetPulseRings();

        if (_pulseRings == null || _pulseRings.Length == 0)
            yield break;

        Color pulseColor;
        float startScale;
        float endScale;
        float duration;
        float ringDelay;

        if (pulseType == InputPrompt.PromptPulseType.FastOutward)
        {
            pulseColor = _fastPulseColor;
            startScale = _fastPulseStartScale;
            endScale = _fastPulseEndScale;
            duration = _fastPulseDuration;
            ringDelay = _fastPulseRingDelay;
        }
        else
        {
            pulseColor = _slowPulseColor;
            startScale = _slowPulseStartScale;
            endScale = _slowPulseEndScale;
            duration = _slowPulseDuration;
            ringDelay = _slowPulseRingDelay;
        }

        for (int i = 0; i < _pulseRings.Length; i++)
        {
            StartCoroutine(AnimatePulseRing(
                _pulseRings[i],
                i * ringDelay,
                pulseColor,
                startScale,
                endScale,
                duration,
                sequenceId));
        }

        float totalDuration = duration + ((_pulseRings.Length - 1) * ringDelay);
        yield return new WaitForSecondsRealtime(totalDuration);

        if (sequenceId == _pulseSequenceId)
            ResetPulseRings();
    }

    private IEnumerator AnimatePulseRing(
        Image ring,
        float delay,
        Color pulseColor,
        float startScale,
        float endScale,
        float duration,
        int sequenceId)
    {
        if (ring == null)
            yield break;

        yield return new WaitForSecondsRealtime(delay);

        if (sequenceId != _pulseSequenceId)
            yield break;

        RectTransform ringRect = ring.rectTransform;
        ring.gameObject.SetActive(true);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (sequenceId != _pulseSequenceId)
            {
                ring.gameObject.SetActive(false);
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scale = Mathf.Lerp(startScale, endScale, t);
            ringRect.localScale = Vector3.one * scale;

            Color c = pulseColor;
            c.a = Mathf.Lerp(pulseColor.a, 0f, t);
            ring.color = c;

            yield return null;
        }

        ringRect.localScale = Vector3.one;
        Color endColor = pulseColor;
        endColor.a = 0f;
        ring.color = endColor;
        ring.gameObject.SetActive(false);
    }

    private void ResetPulseRings()
    {
        if (_pulseRings == null)
            return;

        for (int i = 0; i < _pulseRings.Length; i++)
        {
            if (_pulseRings[i] == null)
                continue;

            _pulseRings[i].rectTransform.localScale = Vector3.one;

            Color c = _pulseRings[i].color;
            c.a = 0f;
            _pulseRings[i].color = c;

            _pulseRings[i].gameObject.SetActive(false);
        }
    }

    private void StopPulseLoop()
    {
        _pulseSequenceId++;

        if (_pulseRoutine != null)
        {
            StopCoroutine(_pulseRoutine);
            _pulseRoutine = null;
        }
    }

    private void PlayShineFlash()
    {
        if (_shineRoutine != null)
            StopCoroutine(_shineRoutine);

        _shineRoutine = StartCoroutine(PlayShineFlashRoutine());
    }

    private IEnumerator PlayShineFlashRoutine()
    {
        if (_shineOverlay == null)
            yield break;

        SetShineAlpha(_shineFlashAlpha);
        yield return new WaitForSecondsRealtime(_shineFlashDuration);
        SetShineAlpha(0f);

        _shineRoutine = null;
    }

    private void SetShineAlpha(float alpha)
    {
        if (_shineOverlay == null)
            return;

        Color c = _shineOverlay.color;
        c.a = alpha;
        _shineOverlay.color = c;
    }
}