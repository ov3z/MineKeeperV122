using UnityEngine;

public abstract class EnemyState
{
    protected EnemyController ownerController;
    protected StateBase substate;

    public EnemyState(EnemyController playerController)
    {
        ownerController = playerController;
    }

    public abstract void OnStateStart();
    public virtual void Execute() { }
    public virtual void FixedExecute() { }
    public abstract void OnStateEnd();

}
