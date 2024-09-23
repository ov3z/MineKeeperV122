using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTackledState : EnemyState
{
    private const string IDLE_ANIM_KEY = "Idle";

    private NavMeshAgent agent;
    private Animator animator;
    private Transform characterTransform;
    private Tween moveTween;
    private float displacement = 1.25f;

    public EnemyTackledState(EnemyController playerController) : base(playerController)
    {
        agent = ownerController.Agent;
        animator = ownerController.Animator;
        characterTransform = ownerController.transform;
    }

    public override void OnStateStart()
    {
        animator.SetBool(IDLE_ANIM_KEY, true);
        agent.enabled = false;
        MoveBack();
    }

    private void MoveBack()
    {
        var newPosition = characterTransform.position - characterTransform.forward * displacement;
        moveTween = characterTransform.DOMove(newPosition, 0.35f).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            ownerController.SwitchState(EnemyStates.Chase);
        });
    }

    public override void OnStateEnd()
    {
        animator.SetBool(IDLE_ANIM_KEY, false);
        agent.enabled = true;
        moveTween?.Kill();
    }
}

