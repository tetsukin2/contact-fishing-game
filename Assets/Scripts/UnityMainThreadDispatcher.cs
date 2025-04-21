using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    private static UnityMainThreadDispatcher _instance;

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            Debug.LogError("🚨 UnityMainThreadDispatcher is not found in the scene. Please add it manually.");
        }
        return _instance;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this; // ✅ Assign instance on Awake()
            DontDestroyOnLoad(gameObject); // ✅ Ensure it persists across scenes
        }
        else
        {
            Destroy(gameObject); // ✅ Prevent duplicates
        }
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        while (_executionQueue.Count > 0)
        {
            Action action;
            lock (_executionQueue)
            {
                action = _executionQueue.Dequeue();
            }
            action.Invoke();
        }
    }
}
