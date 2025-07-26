using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Telemetry managerment, currently only does action timing.
/// </summary>
public class ActionTelemetryHandler : SingletonPersistent<ActionTelemetryHandler>
{
    // Stores the start time for each action currently being timed
    private Dictionary<string, float> _activeTimers = new();

    // Stores the list of time taken for each action
    public Dictionary<string, List<float>> ActionTimeTaken { get; private set; } = new();

    /// <summary>
    /// Starts timing an action. Multiple actions can be timed simultaneously.
    /// </summary>
    public void StartActionTimer(string actionName)
    {
        if (_activeTimers.ContainsKey(actionName))
        {
            Debug.LogWarning($"StartActionTimer called for '{actionName}' but a timer was already running. Overriding previous start time.");
        }
        _activeTimers[actionName] = Time.time;
    }

    /// <summary>
    /// Ends timing for an action and records the elapsed time.
    /// </summary>
    public void EndAndRecordActionTimer(string actionName)
    {
        if (_activeTimers.TryGetValue(actionName, out float startTime))
        {
            float elapsed = Time.time - startTime;
            if (!ActionTimeTaken.ContainsKey(actionName))
                ActionTimeTaken[actionName] = new List<float>();
            ActionTimeTaken[actionName].Add(elapsed);
            _activeTimers.Remove(actionName);
        }
        else
        {
            Debug.LogWarning($"EndActionTime called for '{actionName}' but no timer was started.");
        }
    }

    /// <summary>
    /// Returns a dictionary of action names to their integer average time taken.
    /// </summary>
    public Dictionary<string, int> GetAverageTimeTaken()
    {
        var averages = new Dictionary<string, int>();
        foreach (var kvp in ActionTimeTaken)
        {
            if (kvp.Value.Count > 0)
            {
                float sum = 0f;
                foreach (var t in kvp.Value)
                    sum += t;
                int avg = Mathf.RoundToInt(sum / kvp.Value.Count);
                averages[kvp.Key] = avg;
            }
            else
            {
                averages[kvp.Key] = 0;
            }
        }
        return averages;
    }

    public void ClearAllActionData()
    {
        _activeTimers.Clear();
        ActionTimeTaken.Clear();
    }
}
