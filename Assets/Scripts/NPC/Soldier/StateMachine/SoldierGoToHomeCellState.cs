using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class SoldierGoToHomeCellState : SoldierState
{
    private Animator animator;
    private NavMeshAgent agent;

    private const string MOVE_ANIM_KEY = "Move";
    private const string SPEED_ANIM_KEY = "Speed";

    public SoldierGoToHomeCellState(SoldierController soldierController) : base(soldierController)
    {
        animator = ownerController.Animator;
        agent = ownerController.Agent;

        canChaseEnemies = false;
        canGoToMine = false;
    }

    public override void OnStateStart()
    {
        animator.SetBool(MOVE_ANIM_KEY, true);
        agent.isStopped = false;
        agent.SetDestination(ownerController.HomeCell.position);
    }

    public override void Execute()
    {
        float speedVelocityNormalized = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(SPEED_ANIM_KEY, speedVelocityNormalized);

        Vector3 speedDirection = new Vector3(agent.velocity.x, 0, agent.velocity.z).normalized;
        ownerController.transform.forward = Vector3.Lerp(ownerController.transform.forward, speedDirection, 15 * Time.deltaTime);

        if (agent.remainingDistance < 0.1f)
        {
            ownerController.SwitchState(SoldierStates.Idle);
        }
    }

    public override void OnStateEnd()
    {
        animator.SetBool(MOVE_ANIM_KEY, false);
        animator.SetFloat(SPEED_ANIM_KEY, 0);

        agent.SetDestination(ownerController.transform.position);
        agent.isStopped = true;

        ownerController.transform.DORotate(new Vector3(0, 90, 0), 0.3f).SetEase(Ease.Linear);
    }
}