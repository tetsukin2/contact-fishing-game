using UnityEngine;

/// <summary>
/// UI Panels, enabling/disabling only content allows this script to always run, 
/// avoiding errors from directly disabling the gameobject and causing the script
/// to be unreachable.
/// </summary>
public class GUIPanel : MonoBehaviour
{
    [SerializeField] protected GameObject _content;

    public virtual void Show(bool show)
    {
        //Debug.Log($"Showing {gameObject.name}: {show}");
        if (_content != null)
        {
            _content.SetActive(show);
        }
    }
}
