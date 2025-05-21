using System.Collections;
using UnityEngine;

/// <summary>
/// Static instance class similar to a singleton, but any new instance of the class will override existing instances.
/// </summary>
/// <typeparam name="T">Name of the MonoBehaviour script to be made into a StaticInstance</typeparam>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { private set; get; }

    protected virtual void Awake() 
    {
        Instance = this as T;
        OnAwake();
    }

    private void Start()
    {
        StartCoroutine(DoRegister());
    }

    IEnumerator DoRegister()
    {
        OnRegister();
        yield return new WaitForEndOfFrame();
        OnSetup();
    }

    /// <summary>
    /// OnAwake is called at the instance Awake method.
    /// </summary>
    protected virtual void OnAwake() { }
    /// <summary>
    /// OnRegister is called at the start of the instance Start method.
    /// </summary>
    protected virtual void OnRegister() { }
    /// <summary>
    /// OnSetup is called in the frame after OnRegister.
    /// </summary>
    protected virtual void OnSetup() { }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
