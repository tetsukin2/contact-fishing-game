using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoystickCursorSelectable : GUIContainer
{
    public Image DisplayImage;                 // The black-and-white image to detect over
    public Image BWImage;                 // The black-and-white image to detect over
    [SerializeField] protected Sprite _normalSprite;
    [SerializeField] protected Sprite _hoverSprite;
    [SerializeField] private Sprite _disabledSprite;
    [HideInInspector] public bool TriggersActuation = true;
    [SerializeField] private JoystickCursorTooltip.TooltipText _tooltipText;
    [SerializeField] private bool _isSelectable = true; // Whether this image can be selected or not

    public Texture2D texture { get; private set; }          // Cached texture
    public Color32[] pixelData { get; private set; }      // Cached pixels
    public int width { get; private set; }
    public int height { get; private set; }        // Texture dimensions
    public RectTransform rectTransform { get; private set; }  // Cached rectTransform

    public virtual JoystickCursorTooltip.TooltipText TooltipText => _tooltipText;

    /// <summary>
    /// Whether this can be selected at all 
    /// (Note: Even an invisible image may still be detected, which is why the flag exists)
    /// </summary>
    public bool IsSelectable
    {
        get => _isSelectable;
        set
        {
            _isSelectable = value;
            if (value)
            {
                DisplayImage.sprite = _normalSprite;
            }
            else
            {
                DisplayImage.sprite = _disabledSprite;
            }
        }
    }

    private void Awake()
    {
        // Putting this in start causes errors when encyclopedia opens
        // I guess start happens much later
        texture = BWImage.sprite.texture;

        // Initialize
        IsSelectable = _isSelectable;

        if (!texture.isReadable)
        {
            Debug.LogError($"Texture on {gameObject.name} must be readable (enable 'Read/Write' in import settings).");
            return;
        }

        pixelData = texture.GetPixels32();
        width = texture.width;
        height = texture.height;
        rectTransform = BWImage.rectTransform;
    }

    public void SetHover(bool hover)
    {
        if (!IsSelectable) return;
        DisplayImage.sprite = hover ? _hoverSprite : _normalSprite;
    }

    public virtual void OnSelect()
    {

    }
}
