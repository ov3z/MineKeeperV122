using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;

public class PetFollowState : PetState
{
    private const string MOVE_ANIM_KEY = "Run";
    private const string SPEED_ANIM_KEY = "Speed";

    private NavMeshAgent agent;
    private Animator animator;
    private Transform ownerTransform;
    private Transform playerTransform;
    private Rigidbody rigidbody;

    private Vector3 lastPlayerPosition;
    private float destinationUpdateTimer;
    private float destinationUpdateTimerMax = 0.05f;
    private float playerDisplacementForUpdate = 0.1f;
    private float minimalDistanceToPlayer = 2f;

    public PetFollowState(PetController creator) : base(creator)
    {
        agent = ownerController.Agent;
        animator = ownerController.Animator;
        ownerTransform = ownerController.transform;
        rigidbody = ownerController.Rigidbody;
    }

    public override void OnStateStart()
    {
        animator.SetBool(MOVE_ANIM_KEY, true);
        playerTransform = PlayerController.Instance.transform;
    }

    public override void Execute()
    {
        destinationUpdateTimer += Time.deltaTime;
        if (destinationUpdateTimer >= destinationUpdateTimerMax)
        {
            destinationUpdateTimer = 0;

            var playerDisplacement = Vector3.Distance(lastPlayerPosition, playerTransform.position);
            if (playerDisplacement > playerDisplacementForUpdate)
            {
                var dirToPlayer = (playerTransform.position - ownerController.transform.position).normalized;
                var updatedDestination = playerTransform.position + dirToPlayer * minimalDistanceToPlayer;
                agent.SetDestination(updatedDestination);
            }

            var distanceToPlayer = Vector3.Distance(ownerTransform.position, playerTransform.position);
            if (distanceToPlayer <= minimalDistanceToPlayer)
            {
                ownerController.SwitchState(PetStates.Idle);
            }

            lastPlayerPosition = playerTransform.position;
        }



        var animSpeed = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(SPEED_ANIM_KEY, animSpeed);
    }

    public override void ExecuteFixedUpdate()
    {
        ownerTransform.forward = Vector3.Lerp(ownerTransform.forward, rigidbody.velocity.normalized, 15 * Time.deltaTime);
    }

    public override void OnStateEnd()
    {
        animator.SetBool(MOVE_ANIM_KEY, false);
        animator.SetFloat(SPEED_ANIM_KEY, 0);
        agent.SetDestination(ownerTransform.position);
    }
}
