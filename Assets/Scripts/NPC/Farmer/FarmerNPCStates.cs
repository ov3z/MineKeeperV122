using UnityEngine;
using UnityEngine.AI;

public abstract class FarmerNPCStateBase
{
    protected FarmerNPC ownerController;
    protected StateBase substate;
    protected Transform targetPoint;

    public FarmerNPCStateBase(FarmerNPC NPCController)
    {
        ownerController = NPCController;
    }

    public abstract void OnStateStart();
    public virtual void Execute() { }
    public virtual void FixedExecute() { }
    public abstract void OnStateEnd();
}

public class NPCLookForTheTargetState : FarmerNPCStateBase
{
    private NavMeshAgent agent;
    private Animator animator;
    private Transform ownerTransform;
    private ResourceTypes farmerType;
    private Transform waitingForResourceCanvas;

    private const string IDLE_ANIM_KEY = "Idle";

    public NPCLookForTheTargetState(FarmerNPC ownerController) : base(ownerController)
    {
        agent = ownerController.Agent;
        animator = ownerController.Animator;
        farmerType = ownerController.FarmerType;
        ownerTransform = ownerController.transform;
        waitingForResourceCanvas = ownerController.WaitingForResourceUI;
    }

    public override void OnStateStart()
    {
        animator.SetBool(IDLE_ANIM_KEY, true);
    }

    public override void Execute()
    {
        Plant plant = GameManager.Instance.GetClosestPlant(ownerTransform.position, farmerType);
        if (plant != null)
        {
            ownerController.targetPlant = plant;
            ownerController.SwitchState(NPCStates.GoToTarget);
            waitingForResourceCanvas.gameObject.SetActive(false);
        }
        else
        {
            if (ownerController.IsNearHome)
                waitingForResourceCanvas.gameObject.SetActive(true);
            else
                ownerController.SwitchState(NPCStates.GoToHome);
        }
    }

    public override void OnStateEnd()
    {
        animator.SetBool(IDLE_ANIM_KEY, false);
    }
}

public class NPCGatherCollebtablesState : FarmerNPCStateBase
{
    private Animator animator;
    private Transform ownerTransform;
    private ICollectable targetPlant;
    private string GATHER_ANIM_KEY = "GatherBerry";

    public NPCGatherCollebtablesState(FarmerNPC ownerController) : base(ownerController)
    {
        animator = ownerController.Animator;
        ownerTransform = ownerController.transform;
    }

    public override void OnStateStart()
    {
        targetPlant = ownerController.targetPlant;
        GATHER_ANIM_KEY = targetPlant.GetGatherAnimKey();
        animator.SetBool(GATHER_ANIM_KEY, true);
    }

    public override void Execute()
    {
        Vector3 lookDir = (targetPlant.GetPosition() - ownerTransform.position).normalized;
        ownerTransform.forward = Vector3.Lerp(ownerTransform.forward, lookDir, 12 * Time.deltaTime);
    }

    public override void OnStateEnd()
    {
        animator.SetBool(GATHER_ANIM_KEY, false);
    }
}

public class NPCGoToHomeState : FarmerNPCStateBase
{
    private NavMeshAgent agent;
    private Animator animator;
    private Transform ownerTransform;
    private Transform homeCellTransform;

    protected const string SPEED_ANIM_KEY = "Speed";
    protected const string MOVE_ANIM_KEY = "Run";

    public NPCGoToHomeState(FarmerNPC ownerController) : base(ownerController)
    {
        agent = ownerController.Agent;
        animator = ownerController.Animator;
        homeCellTransform = ownerController.HomeCellTransform;
        ownerTransform = ownerController.transform;
    }

    public override void OnStateStart()
    {
        agent.enabled = true;
        agent.SetDestination(homeCellTransform.position);
        animator.SetBool(MOVE_ANIM_KEY, true);
    }

    public override void Execute()
    {
        animator.SetFloat(SPEED_ANIM_KEY, agent.velocity.magnitude / agent.speed);
        ownerTransform.forward = Vector3.Lerp(ownerTransform.forward, agent.velocity.normalized, 15 * Time.deltaTime);
    }

    public override void OnStateEnd()
    {
        agent.enabled = false;
        animator.SetBool(MOVE_ANIM_KEY, false);
    }
}

public class NPCGoToTargetState : FarmerNPCStateBase
{
    private NavMeshAgent agent;
    private Animator animator;
    private Transform ownerTransform;

    protected const string SPEED_ANIM_KEY = "Speed";
    protected const string MOVE_ANIM_KEY = "Run";

    public NPCGoToTargetState(FarmerNPC ownerController) : base(ownerController)
    {
        agent = ownerController.Agent;
        animator = ownerController.Animator;
        ownerTransform = ownerController.transform;
    }

    public override void OnStateStart()
    {
        agent.enabled = true;

        targetPoint = ownerController.targetPlant.GetClosestGatherPoint(ownerTransform);
        agent.SetDestination(targetPoint.position);
        animator.SetBool(MOVE_ANIM_KEY, true);
    }

    public override void Execute()
    {
        animator.SetFloat(SPEED_ANIM_KEY, agent.velocity.magnitude / agent.speed);
        ownerTransform.forward = Vector3.Lerp(ownerTransform.forward, agent.velocity.normalized, 15 * Time.deltaTime);

        if (agent.remainingDistance < 0.3f)
        {
            ownerController.SwitchState(NPCStates.GatherCollectable);
        }
    }

    public override void OnStateEnd()
    {
        agent.enabled = false;
        ownerController.targetPlant.ReleaseGatherPoint();
        animator.SetBool(MOVE_ANIM_KEY, false);
    }
}

public class NPCDropResourcesState : FarmerNPCStateBase
{
    private Animator animator;
    private ResourceStack targetStack;
    private Transform ownerTransform;
    private Transform noSpaceUi;
    private ResourceTypes farmerType;

    private float stackTimer;
    private float stackTimerMax = 0.1f;

    protected const string IDLE_ANIM_KEY = "Idle";

    public NPCDropResourcesState(FarmerNPC ownerController) : base(ownerController)
    {
        animator = ownerController.Animator;
        farmerType = ownerController.FarmerType;
        ownerTransform = ownerController.transform;
        noSpaceUi = ownerController.NOPlaceUI;
    }

    public override void OnStateStart()
    {
        animator.SetBool(IDLE_ANIM_KEY, true);
        targetStack = ownerController.TargetStack;
    }

    public override void Execute()
    {
        stackTimer += Time.deltaTime;
        if (stackTimer >= stackTimerMax)
        {
            stackTimer = 0;
            if (targetStack.RemainingSpaceInTheStack() > 0 && ownerController.GetGatheredResourcesAmount() > 0)
            {
                ownerController.ChangeGatheredResourceAmount(-1);
                ownerController.UpdateVisuals();
                targetStack.ReservePlace(1);
                SpawnAndInitializeCollectedResourceVisual();

                if (ownerController.GetGatheredResourcesAmount() == 0)
                {
                    ownerController.SwitchState(NPCStates.Idle);
                }

                if (noSpaceUi.gameObject.activeSelf)
                {
                    noSpaceUi.gameObject.SetActive(false);
                }
            }
            else if(ownerController.GetGatheredResourcesAmount() > 0)
            {
                noSpaceUi.gameObject.SetActive(true);
            }
        }
    }

    private void SpawnAndInitializeCollectedResourceVisual()
    {
        ResourceUnit collectedResourceUnit = PoolingSystem.Instance.GetResourcePool(farmerType).GetObject() as ResourceUnit;
        collectedResourceUnit.transform.position = ownerTransform.position;
        Vector3 resourceDestinationPosition = (targetStack as IStackHolder).GetNextStackElementPosition();
        collectedResourceUnit.SetDestination(resourceDestinationPosition);
        collectedResourceUnit.SetJumpPower(2);
        collectedResourceUnit.SetJumpDuration(0.7f);
        collectedResourceUnit.gameObject.SetActive(true);
        collectedResourceUnit.OnMotionEnd += OnResourceForStackMotionEnd;
    }

    private void OnResourceForStackMotionEnd(ResourceUnit sender)
    {
        targetStack.AddResources(1);
        targetStack.UpdateVisuals();

        sender.OnMotionEnd -= OnResourceForStackMotionEnd;

        ownerController.ChangeReservedSpaceAmount(-1);

        if (ownerController.GetReservedSpaceAmount() == 0)
        {
            ownerController.SwitchState(NPCStates.LookForTheTarget);
        }
    }

    public override void OnStateEnd()
    {
        animator.SetBool(IDLE_ANIM_KEY, false);
    }
}

public class NPCIdleState : FarmerNPCStateBase
{
    private Animator animator;
    protected const string IDLE_ANIM_KEY = "Idle";

    public NPCIdleState(FarmerNPC ownerController) : base(ownerController)
    {
        animator = ownerController.Animator;
    }

    public override void OnStateStart()
    {
        animator.SetBool(IDLE_ANIM_KEY, true);
    }

    public override void OnStateEnd()
    {
        animator.SetBool(IDLE_ANIM_KEY, false);
    }
}

public enum NPCStates
{
    LookForTheTarget,
    GatherCollectable,
    GoToHome,
    GoToTarget,
    DropResources,
    Idle
}