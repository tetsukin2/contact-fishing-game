using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuSelect : GUIPanel
{
    [SerializeField] protected MenuSelectOption[] _menuSelectOptions;
    [SerializeField] protected FishingRodMovement _fishingRodMovement;

    protected List<float> _selectionPoints = new();
    protected int _currentSelectionIndex = 0;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        float menuRange = _fishingRodMovement.MenuRotationMax - _fishingRodMovement.MenuRotationMin;
        float selectionRange = menuRange / _menuSelectOptions.Length;
        for (int i = 0; i < _menuSelectOptions.Length; i++)
        {
            float selectionPoint = _fishingRodMovement.MenuRotationMin + selectionRange * (i + 0.5f);
            _selectionPoints.Add(selectionPoint);
        }

        // Define these in subclasses, lest the event be listened to between scenes
        //  (as in go handle active subbing and unsubbing from the events)
        //  ONLY listen during the correct state
        //InputDeviceManager.Instance.JoystickPressed.AddListener(OnOptionSelected);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Debug.Log($"{_fishingRodMovement.MenuRotationMin}/{InputDeviceManager.IMURotation.y}/{_fishingRodMovement.MenuRotationMax}");
        // Get the current IMU rotation on the Y-axis
        float currentRotation = Mathf.Lerp(-InputDeviceManager.IMURotation.z, 0f, Mathf.Abs(InputDeviceManager.IMURotation.y));

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
        // Update the selection state for menu options
        for (int i = 0; i < _menuSelectOptions.Length; i++)
        {
            _menuSelectOptions[i].SetSelected(i == closestPointIndex);
        }
        // Update index for rest of class to see
        _currentSelectionIndex = closestPointIndex;
    }

    protected virtual void OnOptionSelected()
    {
        
    }
}
