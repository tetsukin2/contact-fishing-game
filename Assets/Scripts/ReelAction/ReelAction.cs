using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ReelAction
{
    protected ReelingState reelingState;

    public ReelAction(ReelingState reelingState)
    {
        this.reelingState = reelingState;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
