using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class StateBase
{
    protected PlayerController ownerController;
    protected StateBase substate;

    public StateBase(PlayerController playerController)
    {
        ownerController = playerController;
    }

    public abstract void OnStateStart();
    public virtual void Execute() { }
    public virtual void FixedExecute() { }
    public abstract void OnStateEnd();  
    
}

public enum States
{
    Idle,
    Move,
    Gather,
    Fight,
    Death,
    ShowSword
}
