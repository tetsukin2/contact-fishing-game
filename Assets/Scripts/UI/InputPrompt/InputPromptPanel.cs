using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private CanvasGroup _progressCanvasGroup;
    [SerializeField] private float _progressFadeDuration = 0.15f;
    [SerializeField] private float _progressSmoothSpeed = 6f;

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

    [Header("Step Feedback")]
    [SerializeField] private Image _shineOverlay;
    [SerializeField] private RectTransform _shineRect;
    [SerializeField] private float _stepFlashPeakAlpha = 0.35f;
    [SerializeField] private float _stepFlashFadeInDuration = 0.05f;
    [SerializeField] private float _stepFlashFadeOutDuration = 0.12f;
    [SerializeField] private float _stepFlashScaleMultiplier = 1.04f;

    [Header("Completion Feedback")]
    [SerializeField] private float _completionFlashPeakAlpha = 0.85f;
    [SerializeField] private float _completionFlashFadeInDuration = 0.06f;
    [SerializeField] private float _completionFlashFadeOutDuration = 0.20f;
    [SerializeField] private float _completionFlashScaleMultiplier = 1.10f;

    [Header("Progress Colors")]
    [SerializeField] private Color _normalProgressColor = Color.white;
    [SerializeField] private Color _completeFlashColor = new Color(1f, 1f, 1f, 1f);

    private Coroutine _pulseRoutine;
    private Coroutine _shineRoutine;
    private Coroutine _progressFadeRoutine;
    private InputPrompt.PromptPulseType _currentPulseType = InputPrompt.PromptPulseType.None;

    private int _pulseSequenceId = 0;

    private float _targetProgress = 0f;
    private float _currentProgress = 0f;

    private bool _completionFeedbackPlayed = false;
    private Vector3 _shineBaseScale = Vector3.one;

    private void Awake()
    {
        if (_inputPromptType == InputPromptType.Main)
            MainInstance = this;
        else
            SecondaryInstance = this;

        UIManager.Instance.MainInputPromptShown.AddListener(OnMainInputPromptShown);
        UIManager.Instance.SecondInputPromptShown.AddListener(OnSecondInputPromptShown);

        if (_progressCanvasGroup != null)
            _progressCanvasGroup.alpha = 0f;

        if (_shineRect != null)
            _shineBaseScale = _shineRect.localScale;

        ShowProgress(false, true);
        ResetAllVisualState();
    }

    private void OnEnable()
    {
        ResetAllVisualState();
    }

    private void Update()
    {
        if (_progressFill != null)
        {
            _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, Time.unscaledDeltaTime * _progressSmoothSpeed);

            if (Mathf.Abs(_currentProgress - _targetProgress) < 0.001f)
                _currentProgress = _targetProgress;

            _progressFill.fillAmount = _currentProgress;
        }
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

    public void SetInputPrompt(InputPrompt inputPrompt)
    {
        StopPulseLoop();
        ResetPulseRings();
        ResetCompletionFeedbackState();

        if (inputPrompt == null)
        {
            Show(false);
            ShowProgress(false);
            ResetProgress();
            _currentPulseType = InputPrompt.PromptPulseType.None;
            return;
        }

        Show(true);
        SetVideo(inputPrompt.Video);
        _message.text = inputPrompt.Message;

        _currentPulseType = inputPrompt.PulseType;

        ShowProgress(inputPrompt.UseProgress);

        if (!inputPrompt.UseProgress)
            ResetProgress();

        PlayPromptPulse();
    }

    public void ShowProgress(bool show, bool immediate = false)
    {
        if (_progressRoot == null)
            return;

        if (_progressFadeRoutine != null)
            StopCoroutine(_progressFadeRoutine);

        if (immediate || _progressCanvasGroup == null)
        {
            _progressRoot.SetActive(show);

            if (_progressCanvasGroup != null)
                _progressCanvasGroup.alpha = show ? 1f : 0f;

            return;
        }

        _progressFadeRoutine = StartCoroutine(FadeProgressRoutine(show));
    }

    public void SetProgress(float value)
    {
        float clamped = Mathf.Clamp01(value);
        _targetProgress = clamped;

        if (_progressFill != null)
        {
            if (clamped >= 1f)
            {
                _progressFill.color = _completeFlashColor;
                PlayCompletionFeedback();
            }
            else
            {
                _progressFill.color = _normalProgressColor;

                if (clamped <= 0.001f)
                    ResetCompletionFeedbackState();
            }
        }
    }

    /// <summary>
    /// Small smooth confirmation for a correct sub-step.
    /// Example: tilt down succeeded, tilt up succeeded, cast back succeeded.
    /// </summary>
    public void PlayStepFeedback()
    {
        PlayShineFeedback(
            _stepFlashPeakAlpha,
            _stepFlashFadeInDuration,
            _stepFlashFadeOutDuration,
            _stepFlashScaleMultiplier
        );
    }

    /// <summary>
    /// Bigger completion feedback for finishing the full action.
    /// Example: full reel action complete, full cast complete, inspection complete.
    /// </summary>
    public void PlayCompletionFeedback()
    {
        if (_completionFeedbackPlayed)
            return;

        _completionFeedbackPlayed = true;

        StopPulseLoop();
        ResetPulseRings();

        PlayShineFeedback(
            _completionFlashPeakAlpha,
            _completionFlashFadeInDuration,
            _completionFlashFadeOutDuration,
            _completionFlashScaleMultiplier
        );
    }

    public void ResetProgress()
    {
        _targetProgress = 0f;
        _currentProgress = 0f;
        ResetCompletionFeedbackState();

        if (_progressFill != null)
        {
            _progressFill.fillAmount = 0f;
            _progressFill.color = _normalProgressColor;
        }
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

    private void PlayShineFeedback(float peakAlpha, float fadeInDuration, float fadeOutDuration, float scaleMultiplier)
    {
        if (_shineOverlay == null)
            return;

        StopShineRoutine();
        _shineRoutine = StartCoroutine(PlayShineFeedbackRoutine(peakAlpha, fadeInDuration, fadeOutDuration, scaleMultiplier));
    }

    private IEnumerator PlayShineFeedbackRoutine(float peakAlpha, float fadeInDuration, float fadeOutDuration, float scaleMultiplier)
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);

            SetShineAlpha(Mathf.Lerp(0f, peakAlpha, t));
            SetShineScale(Vector3.Lerp(_shineBaseScale, _shineBaseScale * scaleMultiplier, t));

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);

            SetShineAlpha(Mathf.Lerp(peakAlpha, 0f, t));
            SetShineScale(Vector3.Lerp(_shineBaseScale * scaleMultiplier, _shineBaseScale, t));

            yield return null;
        }

        ResetShineVisuals();
        _shineRoutine = null;
    }

    private IEnumerator FadeProgressRoutine(bool show)
    {
        if (show)
            _progressRoot.SetActive(true);

        float startAlpha = _progressCanvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;

        float elapsed = 0f;
        while (elapsed < _progressFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _progressFadeDuration);
            _progressCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        _progressCanvasGroup.alpha = endAlpha;

        if (!show)
            _progressRoot.SetActive(false);

        _progressFadeRoutine = null;
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

    private void StopShineRoutine()
    {
        if (_shineRoutine != null)
        {
            StopCoroutine(_shineRoutine);
            _shineRoutine = null;
        }
    }

    private void ResetAllVisualState()
    {
        StopPulseLoop();
        StopShineRoutine();
        ResetPulseRings();
        ResetShineVisuals();
        ResetProgress();
    }

    private void ResetShineVisuals()
    {
        SetShineAlpha(0f);
        SetShineScale(_shineBaseScale);
    }

    private void SetShineAlpha(float alpha)
    {
        if (_shineOverlay == null)
            return;

        Color c = _shineOverlay.color;
        c.a = alpha;
        _shineOverlay.color = c;
    }

    private void SetShineScale(Vector3 scale)
    {
        if (_shineRect != null)
            _shineRect.localScale = scale;
    }

    private void ResetCompletionFeedbackState()
    {
        _completionFeedbackPlayed = false;
    }
}