using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicImagePanel : GUIPanel
{
    [SerializeField] private Image _image;

    public void Awake()
    {
        // Hide the panel at the start if nothing's assigned to it
        Show(_image.sprite != null);
    }

    public void SetImage(Sprite sprite)
    {
        if (sprite == null)
        {
            _content.SetActive(false);
            return;
        }
        _content.SetActive(true);
        _image.sprite = sprite;
    }
}
