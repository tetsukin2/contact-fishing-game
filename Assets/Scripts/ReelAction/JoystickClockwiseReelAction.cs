using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickClockwiseReelAction : ReelAction
{
    public JoystickClockwiseReelAction(ReelingState reelingState) : base(reelingState) { }

    public override void Enter()
    {
        reelingState.FishingManager.InputHelper.ResetJoystickRotationCount();
        Debug.Log("JoystickClockwiseReelAction: Enter");
    }

    public override void Update()
    {
        if (reelingState.FishingManager.InputHelper.GetJoystickRotationCount(true) > 0)
        {
            reelingState.FishingManager.InputHelper.ResetJoystickRotationCount();
            reelingState.ProgressReel();
        }
    }

    public override void Exit()
    {
        Debug.Log("JoystickClockwiseReelAction: Exit");
    }
}
