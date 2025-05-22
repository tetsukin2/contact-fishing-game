using System;
using System.Collections.Generic;

public class UnityMainThreadDispatcher : SingletonPersistent<UnityMainThreadDispatcher>
{
    private static readonly Queue<Action> _executionQueue = new();

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
