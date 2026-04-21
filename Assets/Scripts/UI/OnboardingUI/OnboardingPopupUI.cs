using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class OnboardingPopupUI : MonoBehaviour
{
    [System.Serializable]
    public class OnboardingPage
    {
        public string title;

        [TextArea(3, 8)]
        public string body;

        public VideoClip videoClip;
    }

    private const string ONBOARDING_COMPLETED_KEY = "OnboardingCompleted";

    [Header("Pages")]
    [SerializeField] private List<OnboardingPage> _pages = new();

    [Header("Root")]
    [SerializeField] private GameObject _rootObject;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("UI")]
    [SerializeField] private TMP_Text _headerText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private RawImage _videoDisplay;

    [Header("Dots")]
    [SerializeField] private Transform _dotsContainer;
    [SerializeField] private Image _dotPrefab;
    [SerializeField] private Color _activeDotColor = Color.white;
    [SerializeField] private Color _inactiveDotColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Vector2 _activeDotSize = new Vector2(24f, 24f);
    [SerializeField] private Vector2 _inactiveDotSize = new Vector2(18f, 18f);

    [Header("Buttons")]
    [SerializeField] private Button _prevButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _startButton;

    [Header("Button Visuals")]
    [SerializeField] private CanvasGroup _prevCanvasGroup;
    [SerializeField] private CanvasGroup _nextCanvasGroup;
    [SerializeField] private float _disabledButtonAlpha = 0.35f;
    [SerializeField] private float _enabledButtonAlpha = 1f;

    [Header("Video")]
    [SerializeField] private VideoPlayer _videoPlayer;

    [Header("Cursor")]
    [SerializeField] private SimpleJoystickUICursor _cursor;

    [Header("Start Game")]
    [SerializeField] private string _firstLevelName = "Stage1";
    [SerializeField] private bool _skipOnboardingIfAlreadyCompleted = false;

    private readonly List<Image> _spawnedDots = new();
    private int _currentPage = 0;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        _prevButton.onClick.AddListener(PreviousPage);
        _nextButton.onClick.AddListener(NextPage);
        _startButton.onClick.AddListener(FinishOnboarding);

        BuildDots();
        HideImmediate();
    }

    public bool ShouldSkipOnboarding()
    {
        return _skipOnboardingIfAlreadyCompleted &&
               PlayerPrefs.GetInt(ONBOARDING_COMPLETED_KEY, 0) == 1;
    }

    public void OpenPopup()
    {
        if (_pages == null || _pages.Count == 0)
        {
            Debug.LogWarning("OnboardingPopupUI: No pages assigned.");
            return;
        }

        IsOpen = true;
        _currentPage = 0;

        _rootObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;

        RefreshPage();

        if (_cursor != null)
            _cursor.EnableCursor(true);
    }

    public void HideImmediate()
    {
        IsOpen = false;

        StopVideo();

        if (_cursor != null)
            _cursor.EnableCursor(false);

        _rootObject.SetActive(false);

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
    }

    public void ClosePopup()
    {
        IsOpen = false;

        StopVideo();

        if (_cursor != null)
            _cursor.EnableCursor(false);

        _rootObject.SetActive(false);

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
    }

    public void FinishOnboarding()
    {
        AudioManager.Instance?.PlaySelect();

        PlayerPrefs.SetInt(ONBOARDING_COMPLETED_KEY, 1);
        PlayerPrefs.Save();

        if (MainMenuUIController.Instance != null)
        {
            MainMenuUIController.Instance.ChangeView(MainMenuUIController.MainMenuView.None);
        }

        ClosePopup();

        Debug.Log("Onboarding finished. Loading first level...");
        SceneSwitchHandler.Instance.LoadScene(_firstLevelName);
    }

    public void NextPage()
    {
        AudioManager.Instance?.PlaySelect();
        if (_currentPage >= _pages.Count - 1)
            return;

        _currentPage++;
        RefreshPage();
    }

    public void PreviousPage()
    {
        AudioManager.Instance?.PlaySelect();
        if (_currentPage <= 0)
            return;

        _currentPage--;
        RefreshPage();
    }

    private void RefreshPage()
    {
        if (_pages == null || _pages.Count == 0)
            return;

        OnboardingPage page = _pages[_currentPage];

        _headerText.text = page.title;
        _bodyText.text = page.body;

        PlayVideo(page.videoClip);
        RefreshDots();
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        bool hasPrev = _currentPage > 0;
        bool hasNext = _currentPage < _pages.Count - 1;
        bool isLastPage = _currentPage == _pages.Count - 1;

        _prevButton.interactable = hasPrev;
        _nextButton.interactable = hasNext;

        if (_prevCanvasGroup != null)
            _prevCanvasGroup.alpha = hasPrev ? _enabledButtonAlpha : _disabledButtonAlpha;

        if (_nextCanvasGroup != null)
            _nextCanvasGroup.alpha = hasNext ? _enabledButtonAlpha : _disabledButtonAlpha;

        _startButton.gameObject.SetActive(isLastPage);
    }

    private void BuildDots()
    {
        if (_dotsContainer == null || _dotPrefab == null)
            return;

        for (int i = _dotsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_dotsContainer.GetChild(i).gameObject);
        }

        _spawnedDots.Clear();

        for (int i = 0; i < _pages.Count; i++)
        {
            Image dot = Instantiate(_dotPrefab, _dotsContainer);
            dot.gameObject.SetActive(true);

            LayoutElement layout = dot.GetComponent<LayoutElement>();
            if (layout == null)
                layout = dot.gameObject.AddComponent<LayoutElement>();

            _spawnedDots.Add(dot);
        }

        Debug.Log($"Building dots for {_pages.Count} pages");

        RefreshDots();
    }

    private void RefreshDots()
    {
        if (_spawnedDots.Count == 0)
            return;

        for (int i = 0; i < _spawnedDots.Count; i++)
        {
            bool isActive = i == _currentPage;

            _spawnedDots[i].color = isActive ? _activeDotColor : _inactiveDotColor;

            LayoutElement layout = _spawnedDots[i].GetComponent<LayoutElement>();
            Vector2 size = isActive ? _activeDotSize : _inactiveDotSize;

            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
        }
    }

    private void PlayVideo(VideoClip clip)
    {
        if (_videoPlayer == null)
            return;

        if (clip == null)
        {
            StopVideo();
            if (_videoDisplay != null)
                _videoDisplay.enabled = false;
            return;
        }

        if (_videoDisplay != null)
            _videoDisplay.enabled = true;

        _videoPlayer.Stop();
        _videoPlayer.clip = clip;
        _videoPlayer.isLooping = true;
        _videoPlayer.Play();
    }

    private void StopVideo()
    {
        if (_videoPlayer == null)
            return;

        _videoPlayer.Stop();
        _videoPlayer.clip = null;
    }
}