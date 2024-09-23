using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FarmerNPC : MonoBehaviour, ICollector
{
    [SerializeField] private ResourceTypes farmerType;

    [SerializeField] private Animator animator;
    [SerializeField] private List<Transform> farmerStackVisual;
    [SerializeField] private AnimationEventHandler animationEventHandler;
    [SerializeField] private Transform noPlaceUI;
    [SerializeField] private Transform waitingForResourceUi;
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] new private Renderer renderer;

    private FarmerNPCStateBase currentState;

    private Dictionary<NPCStates, FarmerNPCStateBase> npcStatesMap = new();
    private List<ICollectable> collectables = new();

    private ResourceStack targetStack;
    private NavMeshAgent agent;
    private ResourceStack baseStack;
    private Transform homeCellTransform;

    private float reservedSpace;
    private float gatheredResources;
    private float maxCapacity = 3;

    public ICollectable targetPlant { get; set; }

    public NavMeshAgent Agent => agent;
    public Animator Animator => animator;
    public ResourceTypes FarmerType => farmerType;
    public Transform HomeCellTransform => homeCellTransform;
    public ResourceStack TargetStack => targetStack;
    public Transform NOPlaceUI => noPlaceUI;
    public Transform WaitingForResourceUI => waitingForResourceUi;
    public bool IsNearHome => targetStack != null;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animationEventHandler.OnGatherAnimationEvent += OnAnimEvent;
    }

    private void OnAnimEvent()
    {
        if (reservedSpace < maxCapacity)
        {
            if (targetPlant.Collect(this))
            {
                reservedSpace++;

                if (hitParticle)
                    hitParticle.Play();

                if (reservedSpace >= maxCapacity)
                {
                    SwitchState(NPCStates.GoToHome);
                }
            }
            if (targetPlant.IsDevastated() && reservedSpace < maxCapacity)
            {
                SwitchState(NPCStates.LookForTheTarget);
            }
        }
    }

    public void SwitchState(NPCStates newState)
    {
        currentState?.OnStateEnd();
        currentState = npcStatesMap[newState];
        currentState?.OnStateStart();
    }

    private void Start()
    {
        InitialiazeNPCStatesMap();
        currentState = npcStatesMap[NPCStates.LookForTheTarget];
    }

    private void InitialiazeNPCStatesMap()
    {
        npcStatesMap.Add(NPCStates.LookForTheTarget, new NPCLookForTheTargetState(this));
        npcStatesMap.Add(NPCStates.GatherCollectable, new NPCGatherCollebtablesState(this));
        npcStatesMap.Add(NPCStates.GoToHome, new NPCGoToHomeState(this));
        npcStatesMap.Add(NPCStates.GoToTarget, new NPCGoToTargetState(this));
        npcStatesMap.Add(NPCStates.DropResources, new NPCDropResourcesState(this));
        npcStatesMap.Add(NPCStates.Idle, new NPCIdleState(this));
    }

    private void Update()
    {
        currentState?.Execute();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent<ICollectable>(out var collectable))
        {
            if (!collectables.Contains(collectable))
            {
                collectables.Add(collectable);
                collectable.OnDevastation += RemovePlant;
            }
        }
        else if (other.TryGetComponent<ResourceStack>(out var stack))
        {
            targetStack = stack;
            if (gatheredResources > 0 && currentState.GetType() == typeof(NPCGoToHomeState))
            {
                SwitchState(NPCStates.DropResources);
            }
            else if (currentState.GetType() == typeof(NPCGoToHomeState))
            {
                SwitchState(NPCStates.LookForTheTarget);
            }
        }
    }

    private void RemovePlant(ICollectable sender)
    {
        collectables.Remove(sender);
        sender.OnDevastation -= RemovePlant;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<ResourceStack>(out var stack))
        {
            targetStack = null;
        }
        if (other.TryGetComponent<ICollectable>(out var collectable))
        {
            collectables.Remove(collectable);
        }
    }

    public void SetBase(ResourceStack stack) => baseStack = stack;

    public void SetHomeCell(Transform homecell) => homeCellTransform = homecell;

    public void OnResourceCollect(ResourceTypes type, float collectedAmount) { }

    public void OnResourceCollect(ResourceUnit sender)
    {
        gatheredResources += 1;
        UpdateVisuals();
        if (gatheredResources >= maxCapacity)
        {
            SwitchState(NPCStates.GoToHome);
        }

        var distanceToPlyer = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
        var soundPropagationDistance = 13;

        if (distanceToPlyer <= soundPropagationDistance)
        {
            SoundManager.Instance.Play(SoundTypes.Collect);
        }
    }

    public void UpdateVisuals()
    {
        for (int i = 0; i < (int)gatheredResources; i++)
        {
            if (!farmerStackVisual[i].gameObject.activeSelf)
                farmerStackVisual[i].gameObject.SetActive(true);
        }
        for (int i = (int)gatheredResources; i < farmerStackVisual.Count; i++)
        {
            if (farmerStackVisual[i].gameObject.activeSelf)
                farmerStackVisual[i].gameObject.SetActive(false);
        }
    }

    public Transform GetDestinationTransform() => farmerStackVisual[(int)gatheredResources + 1];
    public void ChangeGatheredResourceAmount(float amount)
    {
        gatheredResources += amount;
        if (gatheredResources < 0 || gatheredResources > maxCapacity)
        {
            Debug.Log(currentState.GetType() + " " + gameObject);
        }
    }
    public float GetGatheredResourcesAmount() => gatheredResources;
    public void ChangeReservedSpaceAmount(float amount)
    {
        reservedSpace += amount;
        if (reservedSpace < 0 || reservedSpace > maxCapacity)
        {
            Debug.Log(currentState.GetType() + " " + gameObject);
        }
    }
    public float GetReservedSpaceAmount() => reservedSpace;
    public float GetGatherPower() => 1;
}