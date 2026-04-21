using System.Collections.Generic;
using UnityEngine;

public abstract class MenuSelect : GUIContainer
{
    [SerializeField] protected MenuSelectOption[] _menuSelectOptions;

    protected List<float> _selectionPoints = new();
    protected int _currentSelectionIndex = 0;

    private int _lastSelectionIndex = -1;

    protected virtual void Start()
    {
        float menuRange = FishingRodMenu.Instance.MenuRotationMax - FishingRodMenu.Instance.MenuRotationMin;
        float selectionRange = menuRange / _menuSelectOptions.Length;

        for (int i = 0; i < _menuSelectOptions.Length; i++)
        {
            float selectionPoint = FishingRodMenu.Instance.MenuRotationMin + selectionRange * (i + 0.5f);
            _selectionPoints.Add(selectionPoint);
        }

        _lastSelectionIndex = -1;
    }

    protected virtual void Update()
    {
        if (!enabled || !gameObject.activeInHierarchy)
            return;

        if (!IsSelectionActive())
            return;

        float currentRotation = -InputDeviceManager.Instance.IMUInput.Rotation.x;

        int closestPointIndex = 0;
        float closestDistance = Mathf.Abs(currentRotation - _selectionPoints[0]);

        for (int i = 1; i < _selectionPoints.Count; i++)
        {
            float distance = Mathf.Abs(currentRotation - _selectionPoints[i]);
            if (distance < closestDistance)
            {
                closestPointIndex = i;
                closestDistance = distance;
            }
        }

        for (int i = 0; i < _menuSelectOptions.Length; i++)
        {
            _menuSelectOptions[i].SetSelected(i == closestPointIndex);
        }

        if (_lastSelectionIndex != -1 &&
            closestPointIndex != _lastSelectionIndex &&
            ShouldPlayMoveSfx())
        {
            AudioManager.Instance?.PlayMenuMove();
        }

        _currentSelectionIndex = closestPointIndex;
        _lastSelectionIndex = closestPointIndex;
    }

    protected virtual bool IsSelectionActive()
    {
        return true;
    }

    protected virtual bool ShouldPlayMoveSfx()
    {
        return true;
    }

    protected virtual void OnOptionSelected()
    {
    }
}