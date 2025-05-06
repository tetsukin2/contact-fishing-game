using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoystickCursorSelectable : GUIPanel
{
    public Image DisplayImage;                 // The black-and-white image to detect over
    public Image BWImage;                 // The black-and-white image to detect over
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _hoverSprite;
    public bool TriggersActuation = true;
    public string TooltipTitle;
    public string TooltipDescription;
    public bool isSelectable = true; // Whether this image can be selected or not

    public Texture2D texture { get; private set; }          // Cached texture
    public Color32[] pixelData { get; private set; }      // Cached pixels
    public int width { get; private set; }
    public int height { get; private set; }        // Texture dimensions
    public RectTransform rectTransform { get; private set; }  // Cached rectTransform

    private void Awake()
    {
        // Putting this in start causes errors when encyclopedia opens
        // I guess start happens much later
        texture = BWImage.sprite.texture;

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
        if (!isSelectable) return;
        DisplayImage.sprite = hover ? _hoverSprite : _normalSprite;
    }

    public virtual void OnSelect()
    {

    }
}
