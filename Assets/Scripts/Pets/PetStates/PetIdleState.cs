using UnityEngine;

public class PetIdleState : PetState
{
    private const string IDLE_ANIM_KEY = "Idle";

    private Animator animator;
    private Transform ownerTransform;

    private float maxDistanceToPlayer = 2.5f;

    public PetIdleState(PetController creator) : base(creator)
    {
        animator = ownerController.Animator;
        ownerTransform = ownerController.transform;
    }

    public override void OnStateStart()
    {
        animator.SetBool(IDLE_ANIM_KEY, true);
    }

    public override void Execute()
    {
        var distanceToPlayer = Vector3.Distance(ownerTransform.position, PlayerController.Instance.transform.position);
        if (distanceToPlayer > maxDistanceToPlayer)
        {
            ownerController.SwitchState(PetStates.Follow);
        }
    }

    public override void OnStateEnd()
    {
        animator.SetBool(IDLE_ANIM_KEY, false);
    }
}
