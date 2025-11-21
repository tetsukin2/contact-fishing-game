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

    private Dictionary<string, int> _repetitionCounts = new();
     private Dictionary<string, List<float>> _anglesPerAction = new();

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
    public Dictionary<string, float> GetAverageTimeTaken()
    {
        var averages = new Dictionary<string, float>();
        foreach (var kvp in ActionTimeTaken)
        {
            if (kvp.Value.Count > 0)
            {
                float sum = 0f;
                foreach (var t in kvp.Value)
                    sum += t;
                //int avg = Mathf.RoundToInt(sum / kvp.Value.Count);
                float avg = sum / kvp.Value.Count;
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

    public void RecordRepetition(string actionName)
    {
        if (!_repetitionCounts.ContainsKey(actionName))
            _repetitionCounts[actionName] = 0;
        _repetitionCounts[actionName]++;
    }

    public void RecordAngle(string actionName, float angle)
    {
        if (!_anglesPerAction.ContainsKey(actionName))
            _anglesPerAction[actionName] = new List<float>();
        _anglesPerAction[actionName].Add(angle);
    }


    public Dictionary<string, int> GetRepetitionCounts() => _repetitionCounts;
     /// <summary>
    /// Returns average of all recorded angles per action. If no angle exists, it's excluded.
    /// </summary>
    public Dictionary<string, float> GetMaxAngles()
    {
        var maxAverages = new Dictionary<string, float>();
        foreach (var kvp in _anglesPerAction)
        {
            if (kvp.Value.Count > 0)
            {
                float maxSum = 0f;
                foreach (float angle in kvp.Value)
                    maxSum += angle;
                maxAverages[kvp.Key] = maxSum / kvp.Value.Count;
            }
        }
        return maxAverages;
    }
}
