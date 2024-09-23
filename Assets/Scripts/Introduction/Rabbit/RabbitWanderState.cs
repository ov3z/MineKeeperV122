using UnityEngine;
using UnityEngine.AI;

public class RabbitWanderState : RabbitState
{
    private const string MOVE_ANIM_KEY = "Run";
    private const string SPEED_ANIM_KEY = "Speed";

    private Animator animator;
    private NavMeshAgent agent;
    private Transform ownerTransform;

    private bool HasStopped => (agent.remainingDistance < 0.1f) || (agent.velocity == Vector3.zero);

    public RabbitWanderState(RabbitController rabbitController) : base(rabbitController)
    {
        animator = ownerController.Animator;
        agent = ownerController.Agent;
        ownerTransform = ownerController.transform;
    }

    public override void OnStateStart()
    {
        animator.SetTrigger(MOVE_ANIM_KEY);
    }
    public override void Execute()
    {
        if (HasStopped)
        {
            agent.ResetPath();
            while (true)
            {
                Vector3 randomPosition = ownerTransform.position + new Vector3(Random.Range(-10, 10),
                                                                                     0,
                                                                                     Random.Range(-10, 10));
                if (NavMesh.SamplePosition(randomPosition, out var hit, 20, -1))
                {
                    agent.SetDestination(hit.position);
                    break;
                }
            }
            return;
        }

        ownerTransform.forward = Vector3.Lerp(ownerTransform.forward, agent.velocity, 15 * Time.deltaTime);
        animator.SetFloat("Speed", (agent.velocity.magnitude / agent.speed));
    }

    public override void OnStateEnd()
    {
        animator.SetTrigger(MOVE_ANIM_KEY);
    }
}
