using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeliveryGuy : MonoBehaviour, ICollector, ITradeMaker
{
    [SerializeField] private Animator animator;
    [SerializeField] private List<Transform> farmerStackVisual;
    [SerializeField] private Transform cartTransform;
    [SerializeField] private ResourceTypes deliveredType;
    [SerializeField] private Transform stackTransform;
    [SerializeField] private Transform marketTransform;
    [SerializeField] private Transform cartWheel;
    [SerializeField] private float cartAngulerSpeed = 10;

    public Action<Collider> OnDelivererTriggerEnter;
    public Action<Collider> OnDelivererTriggerStay;
    public Action<Collider> OnDelivererTriggerExit;

    private DeliveryGuyState currentState;
    private Dictionary<DeliveryGuyStates, DeliveryGuyState> statesMap = new();
    private IInteractable interactionTarget;


    private float gatheredResources;
    [SerializeField] private float maxCapacity = 12;
    private float gatherPower = 1;
    private float speed;
    private NavMeshAgent agent;

    public Transform StackTransform => stackTransform;
    public Transform MarketTransform => marketTransform;
    public int MaxCapacity => (int)maxCapacity;
    public IInteractable InteractionTarget => interactionTarget;
    public ResourceTypes CollectorType => deliveredType;
    public NavMeshAgent Agent => agent;
    public Animator Animator => animator;
    public float Speed => speed;
    public bool IsThereFreeSpace => gatheredResources < maxCapacity;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        speed = agent.speed;
    }

    private void Start()
    {
        InitializeStatesMap();

        currentState = statesMap[DeliveryGuyStates.Idle];
        currentState.OnStateStart();
    }

    private void InitializeStatesMap()
    {
        statesMap.Add(DeliveryGuyStates.Collect, new DeliveryGuyCollectState(this));
        statesMap.Add(DeliveryGuyStates.GoToStack, new DeliveryGuyGoToStackState(this));
        statesMap.Add(DeliveryGuyStates.GoToMarket, new DeliveryGuyGoToMarketState(this));
        statesMap.Add(DeliveryGuyStates.Idle, new DeliveryGuyIdleState(this));
        statesMap.Add(DeliveryGuyStates.Unload, new DeliveryGuyUnloadState(this));
    }

    private void OnTriggerEnter(Collider other)
    {
        OnDelivererTriggerEnter?.Invoke(other);
    }
    private void OnTriggerStay(Collider other)
    {
        OnDelivererTriggerStay?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        OnDelivererTriggerExit?.Invoke(other);
    }

    private void Update()
    {
        currentState?.OnStateUpdate();

        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchState(DeliveryGuyStates.GoToStack);
        }*/

        UpdateCartWheelRotation();
    }

    private void UpdateCartWheelRotation()
    {
        cartWheel.Rotate(-animator.GetFloat("Speed") * cartAngulerSpeed * Vector3.right, Space.Self);
    }

    public void OnResourceCollect(ResourceTypes type, float collectedAmount)
    {
        gatheredResources += collectedAmount;

        UpdateVisuals();
        if (gatheredResources >= maxCapacity)
        {
            SwitchState(DeliveryGuyStates.GoToMarket);
        }
    }

    public void OnResourceCollect(ResourceUnit sender) { }

    public void UpdateVisuals()
    {
        var cartFillProgress = (float)gatheredResources / maxCapacity * farmerStackVisual.Count;
        cartFillProgress = Mathf.Clamp((int)cartFillProgress, 0, farmerStackVisual.Count);

        for (int i = 0; i < (int)cartFillProgress; i++)
        {
            if (!farmerStackVisual[i].gameObject.activeSelf)
                farmerStackVisual[i].gameObject.SetActive(true);
        }
        for (int i = (int)cartFillProgress; i < farmerStackVisual.Count; i++)
        {
            if (farmerStackVisual[i].gameObject.activeSelf)
                farmerStackVisual[i].gameObject.SetActive(false);
        }
    }

    public void SetInteractionTarget(IInteractable interactable) => interactionTarget = interactable;
    public void ResetInteractionTarget() => interactionTarget = null;
    public IInteractable GetInteractionTarget() => interactionTarget;

    public void SwitchState(DeliveryGuyStates newState)
    {
        Debug.Log(newState);

        currentState?.OnStateEnd();
        currentState = statesMap[newState];
        currentState?.OnStateStart();
    }

    public Transform GetDestinationTransform()
    {
        return cartTransform;
    }

    public float GetGatherPower()
    {
        return gatherPower;
    }

    public float GetResourceBalance(ResourceTypes resourceType)
    {
        if (resourceType == deliveredType)
        {
            return gatheredResources;
        }
        else
        {
            return 0;
        }
    }

    public void ResetDeliverer()
    {
        gatheredResources = 0;
        UpdateVisuals();
    }

    public void ChangeResourceAmount(ResourceTypes resourceTypes, int amount)
    {
        if (resourceTypes == deliveredType)
        {
            gatheredResources += amount;
        }
    }
}
