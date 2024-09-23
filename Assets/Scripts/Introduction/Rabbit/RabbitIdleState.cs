using UnityEngine;
using UnityEngine.AI;

public class RabbitIdleState : RabbitState
{
    private const string IDLE_ANIM_KEY = "Idle";

    private Animator animator;
    private NavMeshAgent agent;

    public RabbitIdleState(RabbitController rabbitController) : base(rabbitController)
    {
        animator = ownerController.Animator;
        agent = ownerController.Agent;
    }

    public override void OnStateStart()
    {
        animator.SetTrigger(IDLE_ANIM_KEY);
        agent.SetDestination(ownerController.transform.position);
    }
    public override void Execute()
    {
        
    }

    public override void OnStateEnd()
    {

    }
}
