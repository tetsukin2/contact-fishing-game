using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSelectOption : MonoBehaviour
{
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Image _image;
    public string Action;

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            _image.sprite = _selectedSprite;
        }
        else
        {
            _image.sprite = _normalSprite;
        }
    }
}
