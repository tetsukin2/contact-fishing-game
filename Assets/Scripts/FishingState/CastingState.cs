using UnityEngine;

public class CastingState : FishingState
{
    public CastingState(FishingManager fishingManager) : base(fishingManager) { }

    public override void Enter()
    {
        Debug.Log("Entering Casting State");
    }

    public override void Update()
    {
        // Example: Transition to BaitPreparationState after casting
        if (Input.GetKeyDown(KeyCode.B))
        {
            _fishingManager.TransitionToState(new BaitPreparationState(_fishingManager));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Casting State");
    }
}