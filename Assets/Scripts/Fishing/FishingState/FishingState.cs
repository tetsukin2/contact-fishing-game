public interface IFishingState
{
    /// <summary>
    /// Sets up the state, called once when the fishing manager is initialized.
    /// </summary>
    public abstract void Setup();

    /// <summary>
    /// Called when transitioning into the state.
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// Updates the state, called every frame while in the state.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Called when transitioning out of the state.
    /// </summary>
    public abstract void Exit();
}
