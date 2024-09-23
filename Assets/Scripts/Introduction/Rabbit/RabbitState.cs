using UnityEngine;

public abstract class RabbitState
{
    protected RabbitController ownerController;

    public RabbitState(RabbitController rabbitController)
    {
        ownerController = rabbitController;
    }

    public abstract void OnStateStart();
    public abstract void Execute();
    public abstract void OnStateEnd();
}

public enum RabbitStates
{
    Idle,
    Wander,
    Eat,
    RunAway
}
