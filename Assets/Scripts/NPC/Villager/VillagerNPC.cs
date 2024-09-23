using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class VillagerNPC : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CustomerOrderUI customerOrderUI;

    private const string ANIM_SPEED_KEY = "Speed";
    private const string ANIM_MOVE_KEY = "Run";
    private const string ANIM_IDLE_KEY = "Idle";

    private Transform homeCell;
    private Transform queueCell;
    private NavMeshAgent agent;
    private bool isGoingToHome;
    private bool isGoingToQueue;
    private bool isWaitingInHome;
    private bool isJustSpawned = true;

    private float homeTimer;
    private float homeTimerMax = 2f;

    private int numberInQueue;
    private int lineIndex;

    private int orderCountMin = 3;
    private int orderCountMax = 4;

    private bool DidGetToTradeStation => Vector3.Distance(transform.position, queueCell.position) <= 0.2f;
    private bool DidGetToHome => Vector3.Distance(transform.position, homeCell.position) <= 0.15f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    private void Update()
    {

        if (homeCell && DidGetToHome && isGoingToHome)
        {
            transform.forward = Vector3.Lerp(transform.forward, homeCell.forward, 10 * Time.deltaTime);
            if (animator.GetBool(ANIM_MOVE_KEY))
            {
                SetIdleAnimation();
                isWaitingInHome = true;
            }
        }
        else if (queueCell && DidGetToTradeStation && isGoingToQueue)
        {
            transform.forward = Vector3.Lerp(transform.forward, queueCell.forward, 10 * Time.deltaTime);
            if (animator.GetBool(ANIM_MOVE_KEY))
            {
                SetIdleAnimation();

                if (numberInQueue == 0)
                {
                    ResourceTypes product = ResourceTypes.Blueberry;
                    int expectedOrderCount = Random.Range(orderCountMin, orderCountMax + 1);
                    int orderCount = 0;

                    SetUpOrderContext(ref product, ref expectedOrderCount, ref orderCount);

                    MakeAnOrder(product, orderCount);

                    SetUpCustomerOrderUI(product, orderCount);
                }
            }
        }
        else
        {
            float moveSpeedNormalized = agent.velocity.magnitude / agent.speed;
            transform.forward = Vector3.Lerp(transform.forward, agent.velocity.normalized, 15 * Time.deltaTime);
            animator.SetFloat(ANIM_SPEED_KEY, moveSpeedNormalized);
        }

        if (isWaitingInHome)
        {
            homeTimer += Time.deltaTime;
            if (homeTimer > homeTimerMax)
            {
                homeTimer = 0;
                isWaitingInHome = false;
                VillagerNPCMaster.Instance.RegisterNpc(this);
            }
        }
    }

    private void SetIdleAnimation()
    {
        animator.SetBool(ANIM_MOVE_KEY, false);
        animator.SetBool(ANIM_IDLE_KEY, true);
    }

    private void SetUpOrderContext(ref ResourceTypes product, ref int expectedOrderCount, ref int orderCount)
    {
        List<ResourceTypes> resourcesForSale = TradeStation.Instance.GetResourceTypesForSale();

        if (resourcesForSale.Count > 0 && TradeStation.Instance.GetRemainingSpaceInCoinStack() > 0 && TradeStation.Instance.GetProductReserve(product) > 0)
        {
            int randomResource = Random.Range(0, resourcesForSale.Count);
            product = resourcesForSale[randomResource];

            bool isThereEnoughProducts = expectedOrderCount <= TradeStation.Instance.GetProductReserve(product);
            bool isThereEnoughSpaceInCoinStack = expectedOrderCount <= TradeStation.Instance.GetRemainingSpaceInCoinStack();
            bool isRemainigProductsPlaceInStack = TradeStation.Instance.GetProductReserve(product) <= TradeStation.Instance.GetRemainingSpaceInCoinStack();

            if (isThereEnoughProducts && isThereEnoughSpaceInCoinStack)
                orderCount = expectedOrderCount;
            else if (!isThereEnoughProducts && isRemainigProductsPlaceInStack)
                orderCount = (int)TradeStation.Instance.GetProductReserve(product);
            else
                orderCount = (int)TradeStation.Instance.GetRemainingSpaceInCoinStack();
        }
        else
        {
            orderCount = expectedOrderCount;
            customerOrderUI.EnableNoProductAlert();
        }
    }

    private void MakeAnOrder(ResourceTypes product, int orderCount)
    {
        TradeStation.Instance.AskForProduct(product, orderCount, lineIndex);
        TradeStation.Instance.OnWaitingTimerUpdateArray[lineIndex] += UpdateOrderTimerUi;
    }

    private void UpdateOrderTimerUi(float progress)
    {
        customerOrderUI.SetOrderTimerFill(progress);

        if (customerOrderUI.IsNoProductAlertEnabled())
            customerOrderUI.DisableNoProductAlert();
    }

    private void SetUpCustomerOrderUI(ResourceTypes product, int orderCount)
    {
        customerOrderUI.EnableOrderUI();
        customerOrderUI.SetOrderSprite(product);
        customerOrderUI.SetOrderCount(orderCount);
    }

    public void SetHome(Transform home)
    {
        homeCell = home;
    }

    public void ReturnHome()
    {
        agent.SetDestination(homeCell.position);
        animator.SetBool(ANIM_IDLE_KEY, false);
        animator.SetBool(ANIM_MOVE_KEY, true);

        customerOrderUI.DisableOrderUI();

        if (!isJustSpawned)
        {
            isJustSpawned = false;
            TradeStation.Instance.OnWaitingTimerUpdateArray[lineIndex] -= UpdateOrderTimerUi;
        }

        isGoingToHome = true;
        isGoingToQueue = false;
        numberInQueue = -1;
    }

    public void GoToQueue(Transform queuePosTransform)
    {
        queueCell = queuePosTransform;

        agent.SetDestination(queuePosTransform.position);

        animator.SetBool(ANIM_IDLE_KEY, false);
        animator.SetBool(ANIM_MOVE_KEY, true);

        isGoingToQueue = true;
        isGoingToHome = false;
        isWaitingInHome = false;
    }

    public void SetNumberInQueue(int numberInQueue)
    {
        this.numberInQueue = numberInQueue;
    }

    public void SetLineIndex(int line)
    {
        this.lineIndex = line;
    }

    public enum States
    {
        InHome,
        InQueue,
        GoingToHome,
        GoingToQueue
    }
}