using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishInspectionPanel : GUIPanel
{
    [SerializeField] private Image _fishImage;
    [SerializeField] private TextMeshProUGUI _fishNameText;

    public void Start()
    {
        // Hide the panel at the start
        Show(false);
    }

    public void ShowFish(Fish fish)
    {
        _fishImage.sprite = fish.Sprite;
        _fishNameText.text = fish.FishName;
    }
}
