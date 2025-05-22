using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene transitions and loading.
/// </summary>
public class SceneSwitchHandler : SingletonPersistent<SceneSwitchHandler>
{
    public event System.Action SceneTransitionComplete;

    [SerializeField] private Animator _transitionAnimator;
    [SerializeField] private float TransitionDuration = 0.5f;

    // Hardcoded stuff
    private const string MAIN_MENU_SCENE_NAME = "MainMenu";
    private const string SWITCH_OUT_NAME = "Wipe Out";
    private const string SWITCH_IN_NAME = "Wipe In";

    /// <summary>
    /// Flag for safety to prevent multiple scene switches at once.
    /// </summary>
    public bool IsSwitching { get; private set; } = false;

    public void PlaySwitchInTransition()
    {
        _transitionAnimator.Play(SWITCH_IN_NAME);
    }
    public void PlaySwitchOutTransition()
    {
        _transitionAnimator.Play(SWITCH_OUT_NAME);
    }

    public void LoadScene(int sceneIndex)
    {
        if (!IsSwitching)
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
    }

    public void LoadScene(string sceneIndex)
    {
        if (!IsSwitching)
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
    }

    /// <summary>
    /// Load the next scene in the build index.
    /// </summary>
    public void LoadNextScene()
    {
        if (!IsSwitching)
            StartCoroutine(LoadSceneCoroutine(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void ReturnToMainMenu()
    {
        if (!IsSwitching)
            StartCoroutine(LoadSceneCoroutine(MAIN_MENU_SCENE_NAME));
    }

    // Load level based on index
    private IEnumerator LoadSceneCoroutine(int sceneIndex)
    {
        PlaySwitchOutTransition();
        IsSwitching = true;

        // Track for scene being loaded
        bool sceneLoaded = false;
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == sceneIndex)
            {
                sceneLoaded = true;
            }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Wait for _transitionAnimator out animation
        yield return new WaitForSecondsRealtime(TransitionDuration);

        PlaySwitchInTransition();
        SceneManager.LoadScene(sceneIndex);

        // Delay until scene loads
        while (!sceneLoaded)
        {
            yield return null;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;

        SceneTransitionComplete?.Invoke();
        IsSwitching = false;
    }

    // Load Scene based on scene name
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        PlaySwitchOutTransition();

        // Track for scene being loaded
        bool sceneLoaded = false;
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == sceneName)
            {
                sceneLoaded = true;
            }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Wait for _transitionAnimator out animation
        yield return new WaitForSecondsRealtime(TransitionDuration);

        PlaySwitchInTransition();
        SceneManager.LoadScene(sceneName);

        // Delay until scene loads
        while (!sceneLoaded)
        {
            yield return null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;

        SceneTransitionComplete?.Invoke();
        IsSwitching = false;
    }
}
