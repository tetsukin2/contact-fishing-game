using UnityEngine;

public class FishSelectable : JoystickCursorSelectable
{
    public string FishID;
    [SerializeField] private Sprite _fishSprite;
    [SerializeField] private Sprite _fishNotDiscoveredSprite;

    public void SetDiscovered(bool value)
    {
        //Debug.Log($"Discovered: {FishID} = {value}");
        TriggersActuation = value;
        _normalSprite = value ? _fishSprite : _fishNotDiscoveredSprite;
        _hoverSprite = value ? _fishSprite : _fishNotDiscoveredSprite;
        DisplayImage.sprite = value ? _fishSprite : _fishNotDiscoveredSprite;
    }
}
