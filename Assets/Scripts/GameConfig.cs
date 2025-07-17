using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Scriptable Object/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Rotation Thresholds")]
    public float RotateUpAngle;
    public float RotateDownAngle;
    public float RollRightAngle;
    public float RollLeftAngle;
    public float SideRotateUpAngle;
    public float SideRotateDownAngle;

    [Space]
    [Header("Bait Preparation")]
    public int BobberSensitivity;
    public int BaitPreparationSteps;

    [Space]
    [Header("Reeling")]
    public int ReelTotalProgress;
    public int ReelProgressAmount;
    public float ReelDecayRate;
    public List<ReelingState.ReelActionName> ReelActionSequence;

    [Space]
    [Header("Braille")]
    public float BraillePatternInterval;
}
