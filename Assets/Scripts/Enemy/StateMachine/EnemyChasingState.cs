using UnityEngine;
using UnityEngine.AI;

public class EnemyChasingState : EnemyState
{
    private Animator animator;
    private NavMeshAgent agent;
    private IDamageable target;

    private float destinationUpdateTimer;
    private float destinationUpdateTimerMax = 0.3f;
    private float chasingTimer;
    private float chasingTimerMax = 3f;

    private float chasingDistanceMax;
    private float attackDistance;

    private const string MOVE_ANIM_KEY = "Move";
    private const string SPEED_ANIM_KEY = "Speed";

    public EnemyChasingState(EnemyController enemyController) : base(enemyController)
    {
        animator = ownerController.Animator;
        agent = ownerController.Agent;
        chasingDistanceMax = 3 * ownerController.ChasingRange;
        attackDistance = ownerController.AttackRange;
    }

    public override void OnStateStart()
    {
        ownerController.NotifyOnTriggerEnter += OwnerController_NotifyOnTriggerEnter;
        ownerController.NotifyOnTriggerExit += OwnerController_NotifyOnTriggerExit;

        animator.SetBool(MOVE_ANIM_KEY, true);
        agent.isStopped = false;
        SetNewTarget();
    }

    private void OwnerController_NotifyOnTriggerEnter(Collider obj)
    {
        if (obj.TryGetComponent<IDamageable>(out var enemy))
        {
            ownerController.RegisterEnemy(enemy);
        }
    }

    private void OwnerController_NotifyOnTriggerExit(Collider obj)
    {
        if (obj.TryGetComponent<IDamageable>(out var enemy))
        {
            ownerController.DiscardEnemy(enemy);
        }
    }

    private void SetNewTarget()
    {
        target = ownerController.GetClosestDamageable();

        if (target == null)
        {
            ownerController.SwitchState(EnemyStates.Idle);
            return;
        }

        agent.SetDestination(target.transform.position);
    }

    public override void Execute()
    {
        if (target == null)
        {
            ownerController.SwitchState(EnemyStates.Idle);
            return;
        }

        float speedVelocityNormalized = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(SPEED_ANIM_KEY, speedVelocityNormalized);

        Vector3 speedDirection = agent.velocity.normalized;
        ownerController.transform.forward = Vector3.Lerp(ownerController.transform.forward, speedDirection, 15 * Time.deltaTime);

        destinationUpdateTimer += Time.deltaTime;
        chasingTimer += Time.deltaTime;

        if (destinationUpdateTimer >= destinationUpdateTimerMax)
        {
            destinationUpdateTimer = 0;
            agent.SetDestination(target.transform.position);

            float distanceToTarget = Vector3.Distance(ownerController.transform.position, target.transform.position);

            if (chasingTimer >= chasingTimerMax || distanceToTarget > chasingDistanceMax)
            {
                if (ownerController.ActiveTargetsCount > 0)
                {
                    SetNewTarget();
                }
                else
                {
                    ownerController.SwitchState(EnemyStates.ReturnHome);
                }
            }
            if (distanceToTarget <= attackDistance)
            {
                ownerController.SwitchState(EnemyStates.Attack);
            }
        }
    }

    public override void OnStateEnd()
    {
        animator.SetBool(MOVE_ANIM_KEY, false);
        animator.SetFloat(SPEED_ANIM_KEY, 0);
        agent.SetDestination(ownerController.transform.position);
        agent.isStopped = true;

        chasingTimer = 0;

        ownerController.NotifyOnTriggerEnter -= OwnerController_NotifyOnTriggerEnter;
        ownerController.NotifyOnTriggerExit -= OwnerController_NotifyOnTriggerExit;
    }
}
