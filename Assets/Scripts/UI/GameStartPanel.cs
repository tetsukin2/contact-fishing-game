using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartPanel : GUIPanel
{
    [SerializeField] private GameObject _ready;
    [SerializeField] private GameObject _set;
    [SerializeField] private GameObject _fish;

    public void OnReady()
    {
        _ready.SetActive(true);
        _set.SetActive(false);
        _fish.SetActive(false);
    }

    public void OnSet()
    {
        _ready.SetActive(true);
        _set.SetActive(true);
        _fish.SetActive(false);
    }

    public void OnFish()
    {
        _ready.SetActive(true);
        _set.SetActive(true);
        _fish.SetActive(true);
    }
}
