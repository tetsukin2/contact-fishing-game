using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuGameState : GameState
{
    public MainMenuGameState(GameManager gameManager) : base(gameManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Main Menu State");
    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        Debug.Log("Exiting Main Menu State");
    }
}
