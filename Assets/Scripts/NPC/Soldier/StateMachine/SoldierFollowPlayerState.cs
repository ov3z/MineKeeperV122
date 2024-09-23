using UnityEngine;
using UnityEngine.AI;

public class SoldierFollowPlayerState : SoldierState
{
    private const string MOVE_ANIM_KEY = "Move";
    private const string SPEED_ANIM_KEY = "Speed";

    private Animator animator;
    private NavMeshAgent agent;
    private Transform ownerTransform;
    private PlayerFollowPoint followPoint;

    private Vector3 pointLastPosition;

    public SoldierFollowPlayerState(SoldierController ownerController) : base(ownerController)
    {
        animator = ownerController.Animator;
        agent = ownerController.Agent;
        ownerTransform = ownerController.transform;

        canChaseEnemies = true;
        canGoToMine = true;
    }

    public override void OnStateStart()
    {
        followPoint = PlayerController.Instance.GetClosestPoint(ownerTransform.position);
        animator.SetBool(MOVE_ANIM_KEY, true);
        agent.isStopped = false;

        pointLastPosition = followPoint.GetPosition();
        agent.SetDestination(pointLastPosition);
    }

    public override void Execute()
    {
        float speedVelocityNormalized = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(SPEED_ANIM_KEY, speedVelocityNormalized);

        Vector3 speedDirection = new Vector3(agent.velocity.x, 0, agent.velocity.z).normalized;
        ownerController.transform.forward = Vector3.Lerp(ownerController.transform.forward, speedDirection, 15 * Time.deltaTime);

        float distanceToThePlayer = Vector3.Magnitude(ownerController.transform.position - followPoint.GetPosition());
        if (distanceToThePlayer < 0.5f)
        {
            ownerController.SwitchState(SoldierStates.Idle);
        }

        float playerDisplacement = Vector3.Distance(followPoint.GetPosition(), pointLastPosition);
        if (playerDisplacement > 0.5f)
        {
            pointLastPosition = followPoint.GetPosition();
            agent.SetDestination(pointLastPosition);
        }
    }

    public override void OnStateEnd()
    {
        agent.SetDestination(ownerController.transform.position);

        animator.SetFloat(SPEED_ANIM_KEY, 0);
        animator.SetBool(MOVE_ANIM_KEY, false);

        followPoint.Release();
    }
}
