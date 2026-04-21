using UnityEngine;

public class FishingStateLabelPanel : DynamicImagePanel
{
    [SerializeField] private Sprite _baitPrepLabel;
    [SerializeField] private Sprite _castingLabel;
    [SerializeField] private Sprite _waitingLabel;
    [SerializeField] private Sprite _hookItLabel;        // NEW
    [SerializeField] private Sprite _fishEscapedLabel;    // NEW
    [SerializeField] private Sprite _reelingLabel;
    [SerializeField] private Sprite _inspectionLabel;

    public void SetLabel(FishingManager.FishingStateName state)
    {
        switch (state)
        {
            case FishingManager.FishingStateName.BaitPreparation:
                SetImage(_baitPrepLabel);
                break;
            case FishingManager.FishingStateName.Casting:
                SetImage(_castingLabel);
                break;
            case FishingManager.FishingStateName.WaitingForBite:
                SetImage(_waitingLabel);
                break;
            case FishingManager.FishingStateName.HookIt: // NEW
                SetImage(_hookItLabel != null ? _hookItLabel : _waitingLabel);
                break;
            case FishingManager.FishingStateName.FishEscaped: // NEW
                SetImage(_fishEscapedLabel != null ? _fishEscapedLabel : _waitingLabel);
                break;
            case FishingManager.FishingStateName.Reeling:
                SetImage(_reelingLabel);
                break;
            case FishingManager.FishingStateName.FishInspection:
                SetImage(_inspectionLabel);
                break;
            default:
                SetImage(null);
                break;
        }
    }
}