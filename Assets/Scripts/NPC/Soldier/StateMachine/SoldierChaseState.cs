using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoldierChaseState : SoldierState
{
    private Animator animator;
    private NavMeshAgent agent;
    private float attackDistance;
    private IDamageable target;
    private Transform weapon;
    private Transform tool;

    private const string MOVE_ANIM_KEY = "Move";
    private const string SPEED_ANIM_KEY = "Speed";

    public override bool NeedFollowPlayer => true;

    public SoldierChaseState(SoldierController soldierController) : base(soldierController)
    {
        animator = ownerController.Animator;
        agent = ownerController.Agent;
        attackDistance = ownerController.AttackRange;
        weapon = ownerController.Weapon;
        tool = ownerController.Tool;

        canChaseEnemies = false;
        canGoToMine = false;
    }

    public override void OnStateStart()
    {
        animator.SetBool(MOVE_ANIM_KEY, true);
        agent.isStopped = false;
        SetNewTarget();

        if (weapon.localScale != Vector3.one)
        {
            ownerController.toolAnimTween.Kill();

            var sequence = DOTween.Sequence();
            sequence.Pause();

            if (tool.localScale != Vector3.zero)
                sequence.Append(tool.DOScale(0, 0.2f).SetEase(Ease.Linear));

            sequence.Append(weapon.DOScale(1, 0.2f).SetEase(Ease.Linear));
            sequence.Play();

            ownerController.toolAnimTween = sequence;
        }
    }

    private void SetNewTarget()
    {
        target = ownerController.GetClosestDamageable();

        if (target == null)
        {
            ownerController.SwitchState(SoldierStates.FollowPlayer);
        }
        else
        {
            float distancePlayerTarget = Vector3.Distance(target.transform.position, ownerController.PlayerTransform.position);
            if (distancePlayerTarget > ownerController.MaxDistanceToThePlayer)
            {
                ownerController.SwitchState(SoldierStates.FollowPlayer);
            }
            else
            {
                agent.SetDestination(target.transform.position);
            }
        }
    }

    public override void Execute()
    {
        float speedVelocityNormalized = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(SPEED_ANIM_KEY, speedVelocityNormalized);

        Vector3 speedDirection = new Vector3(agent.velocity.x, 0, agent.velocity.z).normalized;
        ownerController.transform.forward = Vector3.Lerp(ownerController.transform.forward, speedDirection, 15 * Time.deltaTime);

        agent.SetDestination(target.transform.position);

        float distanceToTarget = Vector3.Distance(ownerController.transform.position, target.transform.position);
        float distancePlayerTarget = Vector3.Distance(ownerController.PlayerTransform.position, target.transform.position);

        if (distanceToTarget <= attackDistance)
        {
            ownerController.SwitchState(SoldierStates.Fight);
        }
        else if (target == null || target.GetHealth() <= 0)
        {
            SetNewTarget();
        }
        else if (distancePlayerTarget > ownerController.MaxDistanceToThePlayer)
        {
            ownerController.SwitchState(SoldierStates.FollowPlayer);
            return;
        }
    }

    public override void OnStateEnd()
    {
        agent.SetDestination(ownerController.transform.position);
        agent.isStopped = true;

        animator.SetFloat(SPEED_ANIM_KEY, 0);
        animator.SetBool(MOVE_ANIM_KEY, false);
    }
}