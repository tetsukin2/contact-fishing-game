using UnityEngine;

/// <summary>
/// Controls the visibility of a GUIContainer based on the Main Menu View
/// </summary>
public class MainMenuVisibility : MonoBehaviour
{
    [SerializeField] private MainMenuUIController.MainMenuView[] visibleViews;

    private GUIContainer container;

    private void Awake()
    {
        // Safety Checks
        if (MainMenuUIController.Instance == null)
        {
            Debug.LogError($"{gameObject.name}: Disabling MainMenuVisibility as MainMenuUIController is not initialized.");
            enabled = false;
            return;
        }
        if (!TryGetComponent<GUIContainer>(out container))
        {
            Debug.LogError($"{gameObject.name}: Disabling MainMenuVisibility as it requires a GUIContainer component.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        MainMenuUIController.Instance.ViewChanged.AddListener(OnViewChanged);
        // Set initial visibility
        OnViewChanged(MainMenuUIController.Instance.CurrentView);
    }

    //private void OnEnable()
    //{
    //    MainMenuUIController.Instance.ViewChanged.AddListener(OnViewChanged);
    //    // Set initial visibility
    //    OnViewChanged(MainMenuUIController.Instance.CurrentView);
    //}

    //private void OnDisable()
    //{
    //    MainMenuUIController.Instance.ViewChanged.RemoveListener(OnViewChanged);
    //}

    private void OnViewChanged(MainMenuUIController.MainMenuView view)
    {
        bool shouldShow = System.Array.Exists(visibleViews, v => v == view);
        container.Show(shouldShow);
    }
}
