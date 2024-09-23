using UnityEngine;

public abstract class PetState
{
    protected PetController ownerController;

    public PetState(PetController creator)
    {
        ownerController = creator;
    }

    public abstract void OnStateStart();
    public abstract void Execute();
    public virtual void ExecuteFixedUpdate() { }
    public abstract void OnStateEnd();
}

public enum PetStates
{
    Idle,
    Follow
}
