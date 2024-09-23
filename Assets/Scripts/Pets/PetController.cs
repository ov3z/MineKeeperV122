using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PetController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Rigidbody rb;

    private PetState currentState;
    private Dictionary<PetStates, PetState> statesMap = new();

    public Animator Animator => animator;
    public NavMeshAgent Agent => agent;
    public Rigidbody Rigidbody => rb;

    private void Start()
    {
        InitializeStatesMap();
        SwitchState(PetStates.Idle);
        agent.updatePosition = false;
    }

    private void InitializeStatesMap()
    {
        statesMap.Add(PetStates.Idle, new PetIdleState(this));
        statesMap.Add(PetStates.Follow, new PetFollowState(this));
    }

    private void Update()
    {
        currentState?.Execute();
    }

    private void FixedUpdate()
    {
        currentState?.ExecuteFixedUpdate();
        rb.velocity = agent.velocity;
    }

    public void SwitchState(PetStates newState)
    {
        currentState?.OnStateEnd();
        currentState = statesMap[newState];
        currentState.OnStateStart();
    }
}
