using UnityEngine;

/// <summary>
/// Base class of most UI elements, a sort of container.
/// This allows scripts to work fine even if "hiding" the UI, 
///     by separating the content into its own gameobject, then shown or hidden rather than
///     this gameobject itself.
/// </summary>
public class GUIContainer : MonoBehaviour
{
    [SerializeField] protected GameObject _content;

    /// <summary>
    /// Whether this panel's content exists and is currently visible.
    /// </summary>
    public bool ContentActive => (_content != null && _content.activeInHierarchy);

    /// <summary>
    /// Show or hide the content of this panel.
    /// </summary>
    /// <param name="show"></param>
    public virtual void Show(bool show)
    {
        if (_content != null)
        {
            _content.SetActive(show);
        }
    }
}
