public abstract class ReelAction
{
    protected ReelingState reelingState;

    public ReelAction(ReelingState reelingState)
    {
        this.reelingState = reelingState;
    }

    protected FishingManager fishingManager => reelingState.FishingManager;

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
