using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncyclopediaGameState : GameState
{
    public EncyclopediaGameState(GameManager gameManager) : base(gameManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Encyclopedia State");
    }

    public override void Update()
    {

    }

    public override void Exit()
    {
        InputDeviceManager.SendBrailleASCII(0, 0); // Reset braille values
        Debug.Log("Exiting Encyclopedia State");
    }
}
