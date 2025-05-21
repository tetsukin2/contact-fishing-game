using UnityEngine;

/// <summary>
/// Singleton class that persists across scenes.
/// </summary>
/// <typeparam name="T">Name of the MonoBehaviour script to be made into a Persistent Singleton</typeparam>
public class SingletonPersistent<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
