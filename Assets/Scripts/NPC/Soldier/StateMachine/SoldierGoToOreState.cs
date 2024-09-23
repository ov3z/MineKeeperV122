using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class SoldierGoToOreState : SoldierState
{
    private Animator animator;
    private NavMeshAgent agent;
    private Ore targetOre;
    private Transform weapon;
    private Transform tool;
    private Transform playerTransform;

    private const string MOVE_ANIM_KEY = "Move";
    private const string SPEED_ANIM_KEY = "Speed";

    private bool hasPickaxeInHand;
    private Vector3 targetPosition;
    private Transform targetPoint;

    public override bool NeedFollowPlayer => true;

    public SoldierGoToOreState(SoldierController soldierController) : base(soldierController)
    {
        animator = ownerController.Animator;
        agent = ownerController.Agent;
        weapon = ownerController.Weapon;
        tool = ownerController.Tool;

        canGoToMine = false;
        canChaseEnemies = true;
    }

    public override void OnStateStart()
    {
        playerTransform = PlayerController.Instance.transform;

        animator.SetBool(MOVE_ANIM_KEY, true);
        agent.isStopped = false;
        SetNewTarget();

        if (tool.localScale != Vector3.one)
        {
            ownerController.toolAnimTween.Kill();

            var sequence = DOTween.Sequence();
            sequence.Pause();

            if (weapon.localScale != Vector3.zero)
                sequence.Append(weapon.DOScale(0, 0.2f).SetEase(Ease.Linear));

            sequence.Append(tool.DOScale(1, 0.2f).SetEase(Ease.Linear));
            sequence.Play();

            ownerController.toolAnimTween = sequence;
        }
    }

    private void SetNewTarget()
    {

        if (targetPoint)
            targetOre.ReleaseGatherPoint();

        targetOre = ownerController.GetClosestOre();

        if (targetOre == null)
        {
            ownerController.SwitchState(SoldierStates.FollowPlayer);
        }
        else
        {
            float distanceLPayerTarget = Vector3.Distance(targetOre.transform.position, ownerController.PlayerTransform.position);
            if (distanceLPayerTarget > ownerController.MaxDistanceToThePlayer)
            {
                ownerController.SwitchState(SoldierStates.FollowPlayer);
            }
            else
            {

                targetPoint = targetOre.GetClosestGatherPoint(ownerController.transform);
                agent.SetDestination(targetPoint.position);

                //Debug.Log($"{targetOre} {targetPoint}", targetPoint);
            }
        }
    }

    public override void Execute()
    {
        float speedVelocityNormalized = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(SPEED_ANIM_KEY, speedVelocityNormalized);

        Vector3 speedDirection = new Vector3(agent.velocity.x, 0, agent.velocity.z).normalized;
        ownerController.transform.forward = Vector3.Lerp(ownerController.transform.forward, speedDirection, 15 * Time.deltaTime);

        float distancePLayerTarget = Vector3.Distance(ownerController.PlayerTransform.position, targetOre.transform.position);

        if (distancePLayerTarget > ownerController.MaxDistanceToThePlayer)
        {
            ownerController.SwitchState(SoldierStates.FollowPlayer);
            return;
        }

        var distanceOreSoldier = (targetPoint.position - ownerController.transform.position);
        distanceOreSoldier.y = 0;
        var distanceMagnitude = distanceOreSoldier.magnitude;

        if (targetOre.IsDevastated())
        {
            SetNewTarget();
        }
        else if (distanceMagnitude < 0.35f || agent.remainingDistance < 0.35f)
        {
            ownerController.SwitchState(SoldierStates.Mine);
        }
    }

    public override void OnStateEnd()
    {
        animator.SetFloat(SPEED_ANIM_KEY, 0);
        animator.SetBool(MOVE_ANIM_KEY, false);

        agent.SetDestination(ownerController.transform.position);
        agent.isStopped = true;

        if (targetPoint)
            targetOre.ReleaseGatherPoint();
    }
}