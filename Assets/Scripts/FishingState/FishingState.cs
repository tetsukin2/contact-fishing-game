using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FishingState
{
    protected FishingManager fishingManager;

    public FishingState(FishingManager fishingManager)
    {
        this.fishingManager = fishingManager;
    }

    public virtual void Setup()
    {
        // Setup any state-specific variables or settings here
    }

    // Called when entering the state
    public virtual void Enter() { }

    // Called on every frame while in this state
    public virtual void Update() { }

    // Called when transitioning out of the state
    public virtual void Exit() { }
}
