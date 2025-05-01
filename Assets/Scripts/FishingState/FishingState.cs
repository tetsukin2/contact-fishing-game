public abstract class FishingState
{
    protected FishingManager fishingManager;

    // Accessor if any other class needs to access the fishing manager
    public FishingManager FishingManager => fishingManager;

    public FishingState(FishingManager fishingManager)
    {
        this.fishingManager = fishingManager;
    }

    // Setup any state-specific variables or settings here
    public virtual void Setup() { }

    // Called when entering the state
    public virtual void Enter() { }

    // Called on every frame while in this state
    public virtual void Update() { }

    // Called when transitioning out of the state
    public virtual void Exit() { }
}
