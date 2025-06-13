public abstract class LevelState
{
    protected LevelManager gameManager;

    public LevelState(LevelManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}