using System;
using UnityEngine;

public class EnemyAttackingState : EnemyState
{
    protected Animator animator;
    protected EnemyAnimationEventHandler animationEventHandler;
    protected IDamageable target;
    protected float attackDistance;
    protected float checkDistanceTimer;
    protected float checkDistanceTimerMax = 0.1f;
    protected float damage;

    private const string ATTACK_ANIM_KEY = "Attack";

    public EnemyAttackingState(EnemyController enemyController) : base(enemyController)
    {
        animator = ownerController.Animator;
        animationEventHandler = ownerController.AnimationEventHandler;
        attackDistance = ownerController.AttackRange;
        damage = ownerController.Damage;
    }

    public override void OnStateStart()
    {
        animator.SetBool(ATTACK_ANIM_KEY, true);
        animationEventHandler.OnAttackAnimEvent += DealDamage;
        SetNewTarget();
    }

    private void SetNewTarget()
    {
        target = ownerController.GetClosestDamageable();
        if (target == null)
        {
            ownerController.SwitchState(EnemyStates.ReturnHome);
        }
    }

    protected virtual void DealDamage()
    {
        target.TakeDamage(damage);

        if (target.GetHealth() <= 0)
        {
            ownerController.DiscardEnemy(target);
            SetNewTarget();
        }
    }

    public override void Execute()
    {
        if(target == null)
        {
            SetNewTarget();
            return;
        }

        Vector3 directionToTarget = (target.transform.position - ownerController.transform.position).normalized;
        float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        Vector3 eulerAngles = new Vector3(ownerController.transform.eulerAngles.x, angle, ownerController.transform.eulerAngles.z);

        ownerController.transform.rotation = Quaternion.Lerp(ownerController.transform.rotation, Quaternion.Euler(eulerAngles), 8 * Time.deltaTime);

        checkDistanceTimer += Time.deltaTime;

        if (checkDistanceTimer >= checkDistanceTimerMax)
        {
            float distance = Vector3.Distance(ownerController.transform.position, target.transform.position);

            if (distance >= attackDistance)
            {
                ownerController.SwitchState(EnemyStates.Chase);
            }
        }

        if (target.GetHealth() <= 0)
        {
            ownerController.DiscardEnemy(target);
            SetNewTarget();
        }
    }

    public override void OnStateEnd()
    {
        animator.SetBool(ATTACK_ANIM_KEY, false);
        animationEventHandler.OnAttackAnimEvent -= DealDamage;
    }
}