using UnityEngine;
using UnityEngine.AI;

public class PasserByNPC : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    private bool HasStopped => (agent.remainingDistance < 0.1f) || (agent.velocity == Vector3.zero);

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        agent.Warp(transform.position);
    }

    private void Start()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        animator.SetBool("Run", true);
    }

    private void Update()
    {
        if (transform.lossyScale == Vector3.zero) return;

        if (HasStopped)
        {
            agent.ResetPath();
            while (true)
            {
                Vector3 randomPosition = transform.position + new Vector3(Random.Range(-10, 10),
                                                                                     0,
                                                                                     Random.Range(-10, 10));
                if (NavMesh.SamplePosition(randomPosition, out var hit, 10, -1))
                {
                    agent.SetDestination(hit.position);
                    break;
                }
            }
            return;
        }

        transform.forward = Vector3.Lerp(transform.forward, agent.velocity, 15 * Time.deltaTime);
        animator.SetFloat("Speed", (agent.velocity.magnitude / agent.speed));
    }
}
