using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles game-wide management tasks
/// </summary>
public class GameManager : SingletonPersistent<GameManager>
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        // Testing/Cheat
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameDataHandler.DeleteGameData();
            //GameDataHandler.CurrentGameData = GameDataHandler.LoadGameData("data", $"{_fishTotalToCatch}");
            Debug.Log("Debug: Deleting Data");
        }
    }

    
}
