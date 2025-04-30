using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicImagePanel : GUIPanel
{
    [SerializeField] private Image _image;

    public void Awake()
    {
        // Hide the panel at the start
        Show(false);
    }

    public void SetImage(Sprite promptSprite)
    {
        if (promptSprite == null)
        {
            _content.SetActive(false);
            return;
        }
        _content.SetActive(true);
        _image.sprite = promptSprite;
    }
}
