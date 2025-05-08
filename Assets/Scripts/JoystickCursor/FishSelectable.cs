using UnityEngine;

public class FishSelectable : JoystickCursorSelectable
{
    public string FishID;
    [SerializeField] private Sprite _fishSprite;
    [SerializeField] private Sprite _fishNotDiscoveredSprite;
    [SerializeField] private JoystickCursorTooltip.TooltipText _NotDiscoveredTooltipText;

    private bool _discovered = false;

    public override JoystickCursorTooltip.TooltipText TooltipText
    {
        get
        {
            if (_discovered)
            {
                return base.TooltipText;
            }
            else
            {
                return _NotDiscoveredTooltipText;
            }
        }
    }

    public void SetDiscovered(bool value)
    {
        //Debug.Log($"Discovered: {FishID} = {value}");
        TriggersActuation = value;
        _discovered = value;
        _normalSprite = value ? _fishSprite : _fishNotDiscoveredSprite;
        _hoverSprite = value ? _fishSprite : _fishNotDiscoveredSprite;
        DisplayImage.sprite = value ? _fishSprite : _fishNotDiscoveredSprite;
    }
}
