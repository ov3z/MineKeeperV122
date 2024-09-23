using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeStation : MonoBehaviour, IUpgradable
{
    public static TradeStation Instance { get; private set; }

    public EventElement<float>[] OnWaitingTimerUpdateArray = new EventElement<float>[3];
    public event Action OnBuild;

    [SerializeField] private ResourceStack coinStack;
    [SerializeField] private List<CounterProductsUnit> productsOnTheCounter;
    [SerializeField] private TradeStationCapacity stationCapacity;
    [SerializeField] private List<QueueLine> queueLines;
    [SerializeField] private List<ResourceTypeIconPair> resourceIcons;
    [SerializeField] private GuidComponent buidTradeStationID;
    [SerializeField] private Transform uiUnitsSample;
    [SerializeField] private Transform productsOnCounterCanvas;
    [SerializeField] private List<Transform> sellerNPCs;
    [SerializeField] private BuildOnPayout buildTradeStation;

    private Dictionary<ResourceTypes, List<ProductOnCounter>> productOnCounterMap = new Dictionary<ResourceTypes, List<ProductOnCounter>>();

    private Dictionary<ResourceTypes, float> productBalanceMap = new Dictionary<ResourceTypes, float>();
    private Dictionary<ResourceTypes, float> productReservedMap = new Dictionary<ResourceTypes, float>();
    private Dictionary<ResourceTypes, float> productReservedOutputMap = new Dictionary<ResourceTypes, float>();
    private Dictionary<ResourceTypes, float> productBalanceMaxMap = new Dictionary<ResourceTypes, float>();
    private Dictionary<ResourceTypes, bool> unlockedProductsMap = new Dictionary<ResourceTypes, bool>();
    private Dictionary<ResourceTypes, Sprite> resourceIconsMap = new Dictionary<ResourceTypes, Sprite>();
    private Dictionary<ResourceTypes, ResourceUIUnit> uiUnitsMap = new Dictionary<ResourceTypes, ResourceUIUnit>();

    private List<VillagerNPC> queue = new List<VillagerNPC>();
    private List<List<VillagerNPC>> queueMatrix = new List<List<VillagerNPC>>();

    private int queueLength = 5;

    private float[] servingTimer = new float[3];
    private bool[] isThereOrderInLine = new bool[3];
    private float servingTimerMax = 2f;
    private ServingContext[] servingContext = new ServingContext[3];
    private bool[] isServing = new bool[3];
    private string buidStationIDString;
    private float stationCapacityIncrement = 15;
    private Renderer stackRenderer;

    private Coroutine payoutCoroutine;
    private Tween productsOnCounterUIScale;


    private bool IsBuilded => PlayerPrefs.GetInt($"IsBuilded{buidStationIDString}", 0) == 0 ? false : true;

    private void Awake()
    {
        Instance = this;
        InitializeMaps();
    }

    private void Start()
    {
        stackRenderer = coinStack.gameObject.GetComponent<Renderer>();

        buidStationIDString = buidTradeStationID.GetGuid().ToString();
        LoadAndFillProductBalanceMap();

        unlockedProductsMap[ResourceTypes.Blueberry] = true;

        foreach (var unlockedResource in unlockedProductsMap)
        {
            if (unlockedResource.Value)
            {
                queueMatrix.Add(new List<VillagerNPC>());
                sellerNPCs[productOnCounterMap.Keys.ToList().IndexOf(unlockedResource.Key)].gameObject.SetActive(true);
            }
        }

        OnWaitingTimerUpdateArray[0] = new EventElement<float>();
        OnWaitingTimerUpdateArray[1] = new EventElement<float>();
        OnWaitingTimerUpdateArray[2] = new EventElement<float>();

        buildTradeStation.OnPayoutComplete += FireOnBuild;
    }

    private void FireOnBuild()
    {
        OnBuild?.Invoke();
    }

    private void InitializeMaps()
    {
        foreach (var icon in resourceIcons)
        {
            resourceIconsMap.Add(icon.Type, icon.Icon);

            Transform uiUnitinstance = Instantiate(uiUnitsSample, uiUnitsSample.parent);

            TextMeshProUGUI productText = uiUnitinstance.GetChild(0).GetComponent<TextMeshProUGUI>();
            Image prouctIcon = uiUnitinstance.GetChild(0).GetChild(0).GetComponent<Image>();

            uiUnitsMap.Add(icon.Type, new ResourceUIUnit
            {
                PriceText = productText,
                PriceIcon = prouctIcon,
            });
        }

        uiUnitsSample.gameObject.SetActive(false);

        foreach (var unit in productsOnTheCounter)
        {
            productOnCounterMap.Add(unit.Type, unit.ProductsOnCounter);
        }
        foreach (var unit in stationCapacity.capacities)
        {
            productBalanceMaxMap.Add(unit.Type, PlayerPrefs.GetFloat($"MaxCapacity{unit.Type}", unit.Capacity));
        }
    }

    private void LoadAndFillProductBalanceMap()
    {
        foreach (var resourceType in productBalanceMaxMap.Keys.ToList())
        {
            productBalanceMap.Add(resourceType, PlayerPrefs.GetFloat($"TradeStation{(int)resourceType}Balance", 0));
            productReservedMap.Add(resourceType, productBalanceMap[resourceType]);
            productReservedOutputMap.Add(resourceType, productBalanceMap[resourceType]);
            unlockedProductsMap.Add(resourceType, PlayerPrefs.GetInt($"HasUnlocked{(int)resourceType}", 0) == 1);
            UpdateCounterVisual(resourceType);

            if (productBalanceMap[resourceType] == 0 && resourceType != ResourceTypes.Blueberry)
                uiUnitsMap[resourceType].PriceText.transform.parent.gameObject.SetActive(false);

            uiUnitsMap[resourceType].PriceText.text = $"{productBalanceMap[resourceType]}/{productBalanceMaxMap[resourceType]}";
            uiUnitsMap[resourceType].PriceIcon.sprite = ResourceSpriteStorage.Instance.GetIcon(resourceType);
        }
    }

    public void AddProducts(ResourceTypes type, float productAmount)
    {
        productBalanceMap[type] += productAmount;
        productReservedOutputMap[type] += productAmount;

        uiUnitsMap[type].PriceText.text = $"{productBalanceMap[type]}/{productBalanceMaxMap[type]}";
        SaveProductsBalance();
        UpdateCounterVisual(type);

        if (!unlockedProductsMap[type])
        {
            unlockedProductsMap[type] = true;
            queueMatrix.Add(new List<VillagerNPC>());
            PlayerPrefs.SetInt($"HasUnlocked{(int)type}", 1);

            sellerNPCs[productOnCounterMap.Keys.ToList().IndexOf(type)].gameObject.SetActive(true);
        }

        if (productReservedMap[type] != 0 && !uiUnitsMap[type].PriceText.transform.parent.gameObject.activeSelf)
            uiUnitsMap[type].PriceText.transform.parent.gameObject.SetActive(true);

        StartServingIfPossible();
    }

    private void StartServingIfPossible()
    {
        for (int i = 0; i < queueMatrix.Count; i++)
        {
            if (servingContext[i] != null)
            {
                float productsReservedBefore = -servingContext[i].productAmountForServing;

                for (int j = i; j >= 0; j--)
                    productsReservedBefore += servingContext[j].productAmountForServing;

                float productsForLineInCounter = productBalanceMap[servingContext[i].productTypeForServing] - productsReservedBefore;

                if (isThereOrderInLine[i] && productsForLineInCounter >= servingContext[i].productAmountForServing && !isServing[i])
                {
                    isServing[i] = true;
                    isThereOrderInLine[i] = false;

                    productReservedOutputMap[servingContext[i].productTypeForServing] -= servingContext[i].productAmountForServing;
                }
            }
        }
    }

    public void ReservePlaceForProduct(ResourceTypes type, float productAmount)
    {
        productReservedMap[type] += productAmount;
    }

    private void SaveProductsBalance()
    {
        foreach (var resourceType in productBalanceMaxMap.Keys.ToList())
        {
            PlayerPrefs.SetFloat($"TradeStation{(int)resourceType}Balance", productBalanceMap[resourceType]);
        }
    }

    private void UpdateCounterVisual(ResourceTypes type)
    {
        int productsToBeShown = Mathf.Clamp(Mathf.CeilToInt(productOnCounterMap[type].Count * productBalanceMap[type] / productBalanceMaxMap[type]), 0, productOnCounterMap[type].Count);

        for (int i = 0; i < productsToBeShown; i++)
        {
            if (!productOnCounterMap[type][i].IsEnabled)
                productOnCounterMap[type][i].Enable();
        }

        if (productsToBeShown <= productOnCounterMap[type].Count)
        {
            for (int i = productsToBeShown; i < productOnCounterMap[type].Count; i++)
            {
                if (productOnCounterMap[type][i].IsEnabled)
                    productOnCounterMap[type][i].Disable();
            }
        }
    }

    private void Update()
    {
        if (!IsBuilded) return;

        AddNewNPCIfPossible();

        for (int i = 0; i < queueMatrix.Count; i++)
        {
            if (queueMatrix[i].Count > 0 && isServing[i])
            {
                bool isThereSpaceInTheStack = coinStack.RemainingSpaceInTheStack() >= servingContext[i].productAmountForServing;

                servingTimer[i] += Time.deltaTime;

                float waitingProgress = servingTimer[i] / servingTimerMax;
                OnWaitingTimerUpdateArray[i].Invoke(waitingProgress);

                if (servingTimer[i] >= servingTimerMax && isThereSpaceInTheStack)
                {
                    servingTimer[i] = 0;

                    isServing[i] = false;
                    isThereOrderInLine[i] = false;

                    ResourceTypes servingType = servingContext[i].productTypeForServing;

                    productBalanceMap[servingType] -= servingContext[i].productAmountForServing;
                    productReservedMap[servingType] -= servingContext[i].productAmountForServing;

                    uiUnitsMap[servingType].PriceText.text = $"{productBalanceMap[servingType]}/{productBalanceMaxMap[servingType]}";
                    SaveProductsBalance();

                    queue.Remove(queueMatrix[i][0]);
                    queueMatrix[i][0].ReturnHome();
                    queueMatrix[i].RemoveAt(0);

                    AddNewNPCToTheQueue();

                    if (payoutCoroutine != null)
                        StopCoroutine(payoutCoroutine);

                    payoutCoroutine = StartCoroutine(PayoutCoroutine(i));

                    UpdateCounterVisual(servingContext[i].productTypeForServing);

                    MoveQueue(i);
                }
            }
        }
    }

    private void AddNewNPCIfPossible()
    {
        for (int i = 0; i < queueMatrix.Count; i++)
        {
            if (queueMatrix[i].Count < queueLength && VillagerNPCMaster.Instance.isThereAnyIdleNPC())
            {
                AddNewNPCToTheQueue();
            }
        }
    }

    private IEnumerator PayoutCoroutine(int index)
    {
        for (int i = 0; i < servingContext[index].productAmountForServing; i++)
        {
            coinStack.ReservePlace(1);
            PrepareCoinUnit();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void PrepareCoinUnit()
    {
        CoinUnit coinUnit = PoolingSystem.Instance.GetResourcePool(ResourceTypes.Coins).GetObject() as CoinUnit;
        coinUnit.transform.position = transform.position;
        Vector3 coinDestination = (coinStack as IStackHolder).GetNextStackElementPosition();
        coinUnit.SetDestination(coinDestination);
        coinUnit.SetJumpDuration(0.7f);
        coinUnit.SetJumpPower(2f);
        coinUnit.gameObject.SetActive(true);
        coinUnit.OnMotionEnd += OnCoinMotionEnd;
    }

    private void OnCoinMotionEnd(ResourceUnit sender)
    {
        coinStack.AddResources(1);
        coinStack.UpdateVisuals();
        sender.OnMotionEnd -= OnCoinMotionEnd;

        if (stackRenderer.isVisible)
            SoundManager.Instance.Play(SoundTypes.Coin);
    }

    private void AddNewNPCToTheQueue()
    {
        VillagerNPC newNPC = VillagerNPCMaster.Instance.GetIdleNPC();

        if (newNPC && !queue.Contains(newNPC))
        {
            queue.Add(newNPC);

            int lineIndex;
            int positionIndex;

            (lineIndex, positionIndex) = GetShortesQuesueIndex();

            queueMatrix[lineIndex].Add(newNPC);

            queueMatrix[lineIndex][^1].GoToQueue(queueLines[lineIndex].linePositions[positionIndex]);
            queueMatrix[lineIndex][^1].SetNumberInQueue(positionIndex);
            queueMatrix[lineIndex][^1].SetLineIndex(lineIndex);
        }
    }

    private (int, int) GetShortesQuesueIndex()
    {
        int shortestQueueIndex = int.MaxValue;
        int shortestQueueLength = int.MaxValue;

        for (int i = 0; i < queueMatrix.Count; i++)
        {
            if (queueMatrix[i].Count < shortestQueueLength)
            {
                shortestQueueIndex = i;
                shortestQueueLength = queueMatrix[i].Count;
            }
        }

        return (shortestQueueIndex, shortestQueueLength);
    }

    private void MoveQueue(int queueIndex)
    {
        if (queueMatrix[queueIndex].Count > 0)
        {
            for (int j = 0; j < queueMatrix[queueIndex].Count; j++)
            {
                int lineIndex = queueIndex;
                int positionIndex = j;

                queueMatrix[queueIndex][j].GoToQueue(queueLines[lineIndex].linePositions[positionIndex]);
                queueMatrix[queueIndex][j].SetNumberInQueue(positionIndex);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            productsOnCounterUIScale.Kill();
            productsOnCounterUIScale = productsOnCounterCanvas.DOScale(1, 0.3f).SetEase(Ease.Linear);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            productsOnCounterUIScale.Kill();
            productsOnCounterUIScale = productsOnCounterCanvas.DOScale(0, 0.3f).SetEase(Ease.Linear);
        }
    }

    public void AskForProduct(ResourceTypes productType, float productAmount, int lineIndex)
    {
        servingContext[lineIndex] = new ServingContext { productTypeForServing = productType, productAmountForServing = productAmount };

        isThereOrderInLine[lineIndex] = true;

        if (productReservedOutputMap[productType] >= productAmount && !isServing[lineIndex])
        {
            productReservedOutputMap[productType] -= productAmount;
            isServing[lineIndex] = true;
        }
    }

    public bool IsStationFullOf(ResourceTypes type) => productReservedMap[type] >= productBalanceMaxMap[type];

    public List<ResourceTypes> GetResourceTypesForSale(bool onlyAvailable = true)
    {
        List<ResourceTypes> productsForSale = productOnCounterMap.Keys.ToList();
        List<ResourceTypes> productsToBeRemoved = new List<ResourceTypes>();

        if (onlyAvailable)
            for (int i = 0; i < productsForSale.Count; i++)
            {
                if (productBalanceMap[productsForSale[i]] == 0)
                {
                    productsToBeRemoved.Add(productsForSale[i]);
                }
            }

        foreach (var product in productsToBeRemoved)
            productsForSale.Remove(product);

        return productsForSale;
    }

    public float GetProductReserve(ResourceTypes type) => productReservedOutputMap[type];
    public float GetRemainingSpaceInCoinStack() => coinStack.RemainingSpaceInTheStack();

    public void Upgrade()
    {
        foreach (var resourceType in productBalanceMap.Keys)
        {
            if (unlockedProductsMap[resourceType])
            {
                productBalanceMaxMap[resourceType] += stationCapacityIncrement;
                PlayerPrefs.SetFloat($"MaxCapacity{resourceType}", productBalanceMaxMap[resourceType]);
                uiUnitsMap[resourceType].PriceText.text = $"{productBalanceMap[resourceType]}/{productBalanceMaxMap[resourceType]}";
            }
        }

        coinStack.GetComponent<IUpgradable>().Upgrade();
    }

    public float GetCurrentStat()
    {
        return productBalanceMaxMap[ResourceTypes.Blueberry];
    }

    public float GetUpgradeIncrement()
    {
        return stationCapacityIncrement;
    }

    public string GetID()
    {
        return "TradeStation";
    }

    public bool IsUnlocked()
    {
        return IsBuilded;
    }

    private void OnDestroy()
    {
        buildTradeStation.OnPayoutComplete -= FireOnBuild;
    }

    [System.Serializable]
    public class CounterProductsUnit
    {
        public ResourceTypes Type;
        public List<ProductOnCounter> ProductsOnCounter;
    }

    [System.Serializable]
    public class QueueLine
    {
        public List<Transform> linePositions;
    }

    public class ServingContext
    {
        public ResourceTypes productTypeForServing;
        public float productAmountForServing;
    }
}