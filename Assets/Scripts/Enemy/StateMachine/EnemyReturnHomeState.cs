using UnityEngine;
using UnityEngine.AI;

public class EnemyReturnHomeState : EnemyState
{
    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 homePosition;
    private Quaternion initialRotation;

    private float minimalDistanceToHome = 0.5f;
    private float distanceCheckTimer;
    private float distanceCheckTimerMax;

    private bool gotToHome;

    private const string IDLE_ANIM_KEY = "Idle";
    private const string MOVE_ANIM_KEY = "Move";
    private const string SPEED_ANIM_KEY = "Speed";

    public EnemyReturnHomeState(EnemyController enemyController) : base(enemyController)
    {
        agent = ownerController.Agent;
        animator = ownerController.Animator;
        homePosition = ownerController.InitialPositon;
        initialRotation = ownerController.InitialRotation;
    }

    public override void OnStateStart()
    {
        animator.SetBool(MOVE_ANIM_KEY, true);
        agent.isStopped = false;
        agent.SetDestination(homePosition);

        ownerController.NotifyOnTriggerEnter += OwnerController_NotifyOnTriggerEnter;
        ownerController.NotifyOnTriggerExit += OwnerController_NotifyOnTriggerExit;
    }

    private void OwnerController_NotifyOnTriggerEnter(Collider obj)
    {
        if (obj.TryGetComponent<IDamageable>(out var enemy))
        {
            ownerController.RegisterEnemy(enemy);
            ownerController.SwitchState(EnemyStates.Chase);
        }
    }

    private void OwnerController_NotifyOnTriggerExit(Collider obj)
    {
        if (obj.TryGetComponent<IDamageable>(out var enemy))
        {
            ownerController.DiscardEnemy(enemy);
        }
    }

    public override void Execute()
    {
        float normalizedVelocity = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(SPEED_ANIM_KEY, normalizedVelocity);

        Vector3 speedDirection = agent.velocity.normalized;
        ownerController.transform.forward = Vector3.Lerp(ownerController.transform.forward, speedDirection, 15 * Time.deltaTime);

        distanceCheckTimer += Time.deltaTime;

        if (distanceCheckTimer >= distanceCheckTimerMax)
        {
            distanceCheckTimer = 0;

            float distanceToHome = Vector3.Distance(ownerController.transform.position, homePosition);
            if (distanceToHome <= minimalDistanceToHome)
            {
                gotToHome = true;
                animator.SetBool(MOVE_ANIM_KEY, false);
                animator.SetBool(IDLE_ANIM_KEY, true);
            }
        }
        if (gotToHome)
        {
            ownerController.transform.rotation = Quaternion.Lerp(ownerController.transform.rotation, initialRotation, 10 * Time.deltaTime);
            if (ownerController.transform.rotation == initialRotation)
            {
                ownerController.SwitchState(EnemyStates.Idle);
            }
        }
    }

    public override void OnStateEnd()
    {
        agent.SetDestination(ownerController.transform.position);
        agent.isStopped = true;
        animator.SetBool(MOVE_ANIM_KEY, false);
        animator.SetBool(IDLE_ANIM_KEY, false);
        animator.SetFloat(SPEED_ANIM_KEY, 0);
    }
}
