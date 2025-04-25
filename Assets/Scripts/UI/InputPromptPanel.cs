using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputPromptPanel : GUIPanel
{
    [SerializeField] private Image _promptImage;

    public void Start()
    {
        // Hide the panel at the start
        Show(false);
    }

    public void ShowPrompt(Sprite promptSprite)
    {
        if (promptSprite == null)
        {
            _content.SetActive(false);
            return;
        }
        _content.SetActive(true);
        _promptImage.sprite = promptSprite;
    }
}
