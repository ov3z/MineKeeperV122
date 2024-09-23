using UnityEngine;
using UnityEngine.AI;

public class RabbitRunAwayState : RabbitState
{
    private const string MOVE_ANIM_KEY = "Run";
    private const string SPEED_ANIM_KEY = "Speed";

    private Animator animator;
    private NavMeshAgent agent;
    private Transform ownerTransform;
    private Transform target;

    private bool HasStopped => (agent.remainingDistance < 0.1f) || (agent.velocity == Vector3.zero);

    public RabbitRunAwayState(RabbitController rabbitController) : base(rabbitController)
    {
        animator = ownerController.Animator;
        agent = ownerController.Agent;
        target = ownerController.RunPoint;
        ownerTransform = ownerController.transform;  
    }


    public override void OnStateStart()
    {
        animator.SetTrigger(MOVE_ANIM_KEY);
        agent.SetDestination(target.position);
    }
    public override void Execute()
    {
        ownerTransform.forward = Vector3.Lerp(ownerTransform.forward, agent.velocity, 15 * Time.deltaTime);
        animator.SetFloat(SPEED_ANIM_KEY, (agent.velocity.magnitude / agent.speed));
    }

    public override void OnStateEnd()
    {
        
    }
}
