using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrewingMachine : MonoBehaviour
{
    public static BrewingMachine Instance { get; private set; }

    [SerializeField] private ResourceStack outputStack;
    [SerializeField] private TradeStationCapacity tankCapacity;
    [SerializeField] private ResourceTypes resourceForBrew;
    [SerializeField] private GuidComponent guidComponent;
    [SerializeField] private TextMeshProUGUI wheatBalanceText;
    [SerializeField] private Transform wheatBalanceCanvas;
    [SerializeField] private Transform brewingProgressCanvas;
    [SerializeField] private Image brewingProgress;
    [SerializeField] private ResourceTypes outputType = ResourceTypes.Beer;
    [SerializeField] private bool shouldOutputRotate = false;
    [SerializeField] private Animator animator;

    private float wheatBalance;
    private float wheatReserved;

    private float brewTimer;
    [SerializeField] private float brewTimerMax = 2f;

    [SerializeField] private float wheatAmountForOneBeer = 5;
    private bool isBrewing;
    private string uniqueID;

    private Coroutine payoutCoroutine;
    private Tween scalebalanceCanvasTween;
    private Tween scaleBrewProgressCanvastween;

    private void Awake()
    {
        Instance = this;
        uniqueID = guidComponent.GetGuid().ToString();
    }

    private void Start()
    {
        LoadAndFillProductBalanceMap();
        if (wheatBalance > wheatAmountForOneBeer)
        {
            isBrewing = true;

            UpdateAnimator();

            scaleBrewProgressCanvastween?.Kill();
            scaleBrewProgressCanvastween = brewingProgressCanvas.DOScale(1, 0.2f).SetEase(Ease.Linear);
        }
    }

    private void UpdateAnimator()
    {
        if (animator)
            animator.SetBool("IsWorking", isBrewing);
    }

    private void LoadAndFillProductBalanceMap()
    {
        wheatBalance = PlayerPrefs.GetFloat($"{uniqueID}{(int)resourceForBrew}Balance", 0);
        wheatBalanceText.text = $"{wheatBalance}/{tankCapacity.capacities[0].Capacity}";
        wheatReserved = wheatBalance;
    }

    public void AddProducts(ResourceTypes type, float productAmount)
    {

        wheatBalance += productAmount;
        wheatBalanceText.text = $"{wheatBalance}/{tankCapacity.capacities[0].Capacity}";
        SaveProductsBalance();

        if (wheatBalance >= wheatAmountForOneBeer)
        {
            isBrewing = true;

            UpdateAnimator() ;

            scaleBrewProgressCanvastween?.Kill();
            scaleBrewProgressCanvastween = brewingProgressCanvas.DOScale(1, 0.2f).SetEase(Ease.Linear);
        }

    }

    public void ReservePlaceForProduct(ResourceTypes type, float productAmount)
    {
        wheatReserved += productAmount;
    }

    private void SaveProductsBalance()
    {
        PlayerPrefs.SetFloat($"{uniqueID}{(int)resourceForBrew}Balance", wheatBalance);
    }

    private void Update()
    {
        if (isBrewing && outputStack.RemainingSpaceInTheStack() > 0)
        {
            brewTimer += Time.deltaTime;

            brewingProgress.fillAmount = Mathf.Clamp(brewTimer / brewTimerMax, 0, 1);

            if (brewTimer >= brewTimerMax)
            {
                brewTimer = 0;
                brewingProgress.fillAmount = Mathf.Clamp(brewTimer / brewTimerMax, 0, 1);

                wheatBalance -= wheatAmountForOneBeer;
                wheatReserved -= wheatAmountForOneBeer;
                SaveProductsBalance();

                wheatBalanceText.text = $"{wheatBalance}/{tankCapacity.capacities[0].Capacity}";

                if (wheatBalance < wheatAmountForOneBeer)
                {
                    isBrewing = false;

                    UpdateAnimator();

                    scaleBrewProgressCanvastween?.Kill();
                    scaleBrewProgressCanvastween = brewingProgressCanvas.DOScale(0, 0.2f).SetEase(Ease.Linear);
                }

                outputStack.ReservePlace(1);
                PrepareOutputUnit();
            }
        }
    }

    private void PrepareOutputUnit()
    {
        CoinUnit outputUnit = PoolingSystem.Instance.GetResourcePool(outputType).GetObject() as CoinUnit;
        outputUnit.transform.position = transform.position;
        Vector3 coinDestination = (outputStack as IStackHolder).GetNextStackElementPosition();
        outputUnit.SetDestination(coinDestination);
        outputUnit.SetHasToRotateHorizontally(shouldOutputRotate);
        outputUnit.SetJumpDuration(1f);
        outputUnit.gameObject.SetActive(true);
        outputUnit.OnMotionEnd += OnCoinMotionEnd;
    }

    private void OnCoinMotionEnd(ResourceUnit sender)
    {
        outputStack.AddResources(1);
        outputStack.UpdateVisuals();
        sender.OnMotionEnd -= OnCoinMotionEnd;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            scalebalanceCanvasTween?.Kill();
            scalebalanceCanvasTween = wheatBalanceCanvas.DOScale(1, 0.3f).SetEase(Ease.Linear);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            scalebalanceCanvasTween?.Kill();
            scalebalanceCanvasTween = wheatBalanceCanvas.DOScale(0, 0.3f).SetEase(Ease.Linear);
        }
    }

    public float GetResourceAmountForBrewing() => wheatAmountForOneBeer;

    public ResourceTypes GetResourceTypeForBrewing() => resourceForBrew;

    public bool IsBrewingTankFull() => wheatReserved == tankCapacity.capacities[0].Capacity;
}