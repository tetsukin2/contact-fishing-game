using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishInspectionPanel : GUIContainer
{
    [SerializeField] private Image _fishImage;
    [SerializeField] private TextMeshProUGUI _fishNameText;
    [SerializeField] private GUIContainer _discoveredPanel;

    public void Start()
    {
        // Hide the panel at the start
        Show(false);
    }

    public void ShowFish(FishData fish)
    {
        _fishImage.sprite = fish.Sprite;
        _fishNameText.text = fish.FishName;
    }

    public void ShowDiscoveredBanner(bool discovered)
    {
        _discoveredPanel.Show(discovered);
    }
}
