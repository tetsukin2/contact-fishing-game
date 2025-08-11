using System.Collections.Generic;

public interface IReelAction
{
    public abstract void Enter();
    /// <summary>
    /// Updates when this reel action is active.
    /// </summary>
    public abstract void Update();
    public abstract void Exit();
}
