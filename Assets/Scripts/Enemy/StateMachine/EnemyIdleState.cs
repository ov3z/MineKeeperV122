using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private Animator animator;

    private const string IDLE_ANIM_KEY = "Idle";

    public EnemyIdleState(EnemyController enemyController) : base(enemyController)
    {
        animator = ownerController.Animator;

        ownerController.NotifyOnTriggerEnter += OwnerController_NotifyOnTriggerEnter;
        ownerController.NotifyOnTriggerExit += OwnerController_NotifyOnTriggerExit;
    }

    private void OwnerController_NotifyOnTriggerEnter(Collider obj)
    {
        if (obj.TryGetComponent<IDamageable>(out var target))
        {
            ownerController.RegisterEnemy(target);
            ownerController.SwitchState(EnemyStates.Chase);
        }
    }

    private void OwnerController_NotifyOnTriggerExit(Collider obj)
    {
        if(obj.TryGetComponent<IDamageable>(out var target))
        {
            ownerController.DiscardEnemy(target);
        }
    }

    public override void OnStateStart()
    {
        animator.SetBool(IDLE_ANIM_KEY, true);
    }

    public override void OnStateEnd()
    {
        animator.SetBool(IDLE_ANIM_KEY, false);

        ownerController.NotifyOnTriggerEnter -= OwnerController_NotifyOnTriggerEnter;
        ownerController.NotifyOnTriggerExit -= OwnerController_NotifyOnTriggerExit;
    }
}