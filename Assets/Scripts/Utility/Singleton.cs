using UnityEngine;

/// <summary>
/// Singleton class that destroys any new instance of the class leaving the original intact.
/// </summary>
/// <typeparam name="T">Name of the MonoBehaviour script to be made into a Singleton</typeparam>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
    }
}
