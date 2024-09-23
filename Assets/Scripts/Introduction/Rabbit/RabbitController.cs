using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RabbitController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform runPoint;
    [SerializeField] private RabbitStates initialState;

    private Dictionary<RabbitStates, RabbitState> statesMap = new();

    private RabbitState currentState;
    private NavMeshAgent agent;

    public Animator Animator => animator;
    public NavMeshAgent Agent => agent;
    public Transform RunPoint => runPoint;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        (IntroductionQuestTargetSystem.Instance as IntroductionQuestTargetSystem).AddFollowTarget(transform);
        InitializeStatesMap();
        SwitchState(initialState);
    }

    private void InitializeStatesMap()
    {
        statesMap.Add(RabbitStates.Idle, new RabbitIdleState(this));
        statesMap.Add(RabbitStates.Wander, new RabbitWanderState(this));
        statesMap.Add(RabbitStates.RunAway, new RabbitRunAwayState(this));  
    }

    private void Update()
    {
        currentState.Execute();
    }

    public void SwitchState(RabbitStates newState)
    {
        currentState?.OnStateEnd();
        currentState = statesMap[newState];
        currentState?.OnStateStart();
    }
}
