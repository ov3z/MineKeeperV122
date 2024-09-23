using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SoldierState
{
    protected SoldierController ownerController;
    protected SoldierState substate;

    public bool canGoToMine { get; protected set; }
    public bool canChaseEnemies { get; protected set; }
    public virtual float distanceToFollowPlayer => 7;
    public virtual bool NeedFollowPlayer => false;

    public SoldierState(SoldierController ownerController)
    {
        this.ownerController = ownerController;
    }

    public abstract void OnStateStart();
    public virtual void Execute() { }
    public virtual void ExecuteFixedUpdate() { }
    public virtual void ExecuteLateUpdate() { }
    public abstract void OnStateEnd();
}
