using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _ambientSource;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _loopSfxSource;

    [Header("Gameplay / World Clips")]
    [SerializeField] private AudioClip _seaWavesClip;
    [SerializeField] private AudioClip _gameplayBgmClip;
    [SerializeField] private AudioClip _fishSplashClip;
    [SerializeField] private AudioClip _rodCastClip;
    [SerializeField] private AudioClip _rodReelClip;
    [SerializeField] private AudioClip _shipHornClip;
    [SerializeField] private AudioClip _successClip;
    [SerializeField] private AudioClip _fishEscapeClip;
    [SerializeField] private AudioClip _stageCompleteClip;

    [Header("UI Clips")]
    [SerializeField] private AudioClip _menuMoveClip;
    [SerializeField] private AudioClip _selectClip;

    [Header("Volumes")]
    [Range(0f, 1f)] [SerializeField] private float _ambientVolume = 0.2f;
    [Range(0f, 1f)] [SerializeField] private float _musicVolume = 0.2f;
    [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float _loopSfxVolume = 0.8f;

    [Header("Per-Sound Multipliers")]
    [Range(0f, 1f)] [SerializeField] private float _shipHornVolumeMultiplier = 0.35f;
    [Range(0f, 1f)] [SerializeField] private float _menuMoveVolumeMultiplier = 1f;
    [Range(0f, 1f)] [SerializeField] private float _selectVolumeMultiplier = 1f;
    [Range(0f, 1f)] [SerializeField] private float _successVolumeMultiplier = 1f;
    [Range(0f, 1f)] [SerializeField] private float _fishSplashVolumeMultiplier = 1f;
    [Range(0f, 1f)] [SerializeField] private float _rodCastVolumeMultiplier = 1f;
    [Range(0f, 1f)] [SerializeField] private float _fishEscapeVolumeMultiplier = 1f;
    [Range(0f, 1f)] [SerializeField] private float _stageCompleteVolumeMultiplier = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();
    }

    private void Start()
    {
        PlaySeaWaves();
    }

    private void SetupSources()
    {
        if (_ambientSource != null)
        {
            _ambientSource.playOnAwake = false;
            _ambientSource.loop = true;
            _ambientSource.spatialBlend = 0f;
            _ambientSource.volume = _ambientVolume;
        }

        if (_musicSource != null)
        {
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;
            _musicSource.volume = _musicVolume;
        }

        if (_sfxSource != null)
        {
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
            _sfxSource.spatialBlend = 0f;
            _sfxSource.volume = _sfxVolume;
        }

        if (_loopSfxSource != null)
        {
            _loopSfxSource.playOnAwake = false;
            _loopSfxSource.loop = true;
            _loopSfxSource.spatialBlend = 0f;
            _loopSfxSource.volume = _loopSfxVolume;
        }
    }

    public void PlaySeaWaves()
    {
        if (_ambientSource == null || _seaWavesClip == null)
            return;

        if (_ambientSource.clip == _seaWavesClip && _ambientSource.isPlaying)
            return;

        _ambientSource.volume = _ambientVolume;
        _ambientSource.clip = _seaWavesClip;
        _ambientSource.loop = true;
        _ambientSource.Play();
    }

    public void StopSeaWaves()
    {
        if (_ambientSource == null)
            return;

        _ambientSource.Stop();
    }

    public void PlayGameplayBgm()
    {
        if (_musicSource == null || _gameplayBgmClip == null)
            return;

        if (_musicSource.clip == _gameplayBgmClip && _musicSource.isPlaying)
            return;

        _musicSource.volume = _musicVolume;
        _musicSource.clip = _gameplayBgmClip;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    public void StopGameplayBgm()
    {
        if (_musicSource == null)
            return;

        _musicSource.Stop();
    }

    public void PlayFishSplash()
    {
        PlayOneShot(_fishSplashClip, _fishSplashVolumeMultiplier);
    }

    public void PlayRodCast()
    {
        PlayOneShot(_rodCastClip, _rodCastVolumeMultiplier);
    }

    public void PlayShipHorn()
    {
        PlayOneShot(_shipHornClip, _shipHornVolumeMultiplier);
    }

    public void PlaySuccess()
    {
        PlayOneShot(_successClip, _successVolumeMultiplier);
    }

    public void PlayFishEscape()
    {
        PlayOneShot(_fishEscapeClip, _fishEscapeVolumeMultiplier);
    }

    public void PlayStageComplete()
    {
        PlayOneShot(_stageCompleteClip, _stageCompleteVolumeMultiplier);
    }

    public void PlayMenuMove()
    {
        if (!CanPlayMenuMoveSfx())
            return;

        PlayOneShot(_menuMoveClip, _menuMoveVolumeMultiplier);
    }

    public void PlaySelect()
    {
        if (!CanPlayUiSelectSfx())
            return;

        PlayOneShot(_selectClip, _selectVolumeMultiplier);
    }

    public void StartRodReelLoop()
    {
        if (_loopSfxSource == null || _rodReelClip == null)
            return;

        if (_loopSfxSource.clip == _rodReelClip && _loopSfxSource.isPlaying)
            return;

        _loopSfxSource.volume = _loopSfxVolume;
        _loopSfxSource.clip = _rodReelClip;
        _loopSfxSource.loop = true;
        _loopSfxSource.Play();
    }

    public void StopRodReelLoop()
    {
        if (_loopSfxSource == null)
            return;

        _loopSfxSource.Stop();
        _loopSfxSource.clip = null;
    }

    private bool CanPlayMenuMoveSfx()
    {
        // Allow menu SFX in Main Menu scene
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (MainMenuUIController.Instance == null)
                return false;

            return MainMenuUIController.Instance.CurrentView == MainMenuUIController.MainMenuView.MainMenu;
        }

        // Allow menu SFX during pause or end states in gameplay
        if (LevelManager.Instance != null)
        {
            if (LevelManager.Instance.IsGamePaused)
                return true;

            if (LevelManager.Instance.CurrentState == LevelManager.Instance.EndScoreState)
                return true;
        }

        return false;
    }

    private bool CanPlayUiSelectSfx()
    {
        // Allow in MainMenu scene
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return true;

        // Allow in pause or end screens
        if (LevelManager.Instance != null)
        {
            if (LevelManager.Instance.IsGamePaused)
                return true;

            if (LevelManager.Instance.CurrentState == LevelManager.Instance.EndScoreState)
                return true;
        }

        return false;
    }

    private void PlayOneShot(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (_sfxSource == null || clip == null)
            return;

        _sfxSource.PlayOneShot(clip, volumeMultiplier);
    }
}