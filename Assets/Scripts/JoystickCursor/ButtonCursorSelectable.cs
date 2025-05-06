using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonCursorSelectable : JoystickCursorSelectable
{
    public UnityEvent onSelect = new();

    public override void OnSelect()
    {
        if (GameManager.Instance.CurrentState != GameManager.Instance.EncyclopediaState) return;
        onSelect.Invoke();
    }
}
