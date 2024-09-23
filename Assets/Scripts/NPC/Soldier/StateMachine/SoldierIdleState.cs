using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierIdleState : SoldierState
{
    private Animator animator;
    private Transform playerTransform;
    private Transform ownerTransform;
    private float maxDistanceToPlayer;

    private const string IDLE_ANIM_KEY = "Idle";

    public override bool NeedFollowPlayer => true;
    public override float distanceToFollowPlayer => 3;

    public SoldierIdleState(SoldierController soldierController) : base(soldierController)
    {
        animator = ownerController.Animator;
        maxDistanceToPlayer = ownerController.MaxDistanceToThePlayer;
        ownerTransform = ownerController.transform;

        canChaseEnemies = true;
        canGoToMine = true;
    }

    public override void OnStateStart()
    {
        playerTransform = PlayerController.Instance.transform;

        animator.SetBool(IDLE_ANIM_KEY, true);
    }

    public override void OnStateEnd()
    {
        animator.SetBool(IDLE_ANIM_KEY, false);
    }
}
