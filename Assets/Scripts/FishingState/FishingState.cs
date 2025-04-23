using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FishingState
{
    protected FishingManager _fishingManager;

    public FishingState(FishingManager fishingManager)
    {
        _fishingManager = fishingManager;
    }

    // Called when entering the state
    public virtual void Enter() { }

    // Called on every frame while in this state
    public virtual void Update() { }

    // Called when transitioning out of the state
    public virtual void Exit() { }
}
