using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PayingInteractor : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform objectTransform;
    [SerializeField] private PriceAsset priceAsset;
    [SerializeField] private List<PayoutValidation> payoutValidations;
    [SerializeField] private GuidComponent guidComponent;
    [SerializeField] private Transform priceUnit;
    [SerializeField] private BuildingTypes buildingType = BuildingTypes.House;
    [SerializeField] private Collider trigger;
    [SerializeField] private Transform unlockAtLevel;
    [SerializeField] private Transform priceCanvas;
    [SerializeField] private TextMeshProUGUI unlockLevelText;
    [SerializeField] private int unlockLevel;
    [SerializeField] private bool isUpgrade;

    private Dictionary<ResourceTypes, float> priceMap = new Dictionary<ResourceTypes, float>();
    private Dictionary<ResourceTypes, float> priceReserveMap = new Dictionary<ResourceTypes, float>();
    private Dictionary<ResourceTypes, ResourceUIUnit> resourceUnitsMap = new Dictionary<ResourceTypes, ResourceUIUnit>();

    private bool isPayedOut = false;
    private bool canInteractInMotion = false;
    private string uniqueID;
    private float interactionTimerMax = 0.1f;
    private float interactionCofficient;

    public bool IsUnlocked => PlayerLevelManager.Instance.Level >= unlockLevel || unlockLevel == 0;

    private void Awake()
    {
        uniqueID = guidComponent.GetGuid().ToString();

        InitializeMaps();

        isPayedOut = PlayerPrefs.GetInt($"IsPayedOut{uniqueID}", 0) == 1 ? true : false;

        if (isPayedOut)
            gameObject.SetActive(false);
    }

    private void OnLevelUp(int level)
    {
        if (PlayerLevelManager.Instance.Level >= unlockLevel)
        {
            if (trigger != null)
                trigger.enabled = true;
            if (unlockAtLevel != null)
                unlockAtLevel.gameObject.SetActive(false);
            if (priceCanvas != null)
                priceCanvas.gameObject.SetActive(true);
        }
    }

    private void InitializeMaps()
    {
        foreach (var kvp in priceAsset.prices)
        {
            priceMap.Add(kvp.Type, PlayerPrefs.GetFloat($"{uniqueID}{(int)kvp.Type}", kvp.Price));
            priceReserveMap.Add(kvp.Type, PlayerPrefs.GetFloat($"{uniqueID}{(int)kvp.Type}", kvp.Price));
        }
    }

    private void InitializePriceUI()
    {
        foreach (var kvp in priceMap)
        {
            Transform uiUnitInstance = Instantiate(priceUnit, priceUnit.parent);

            resourceUnitsMap.Add(kvp.Key, new ResourceUIUnit
            {
                PriceText = uiUnitInstance.GetChild(0).GetComponent<TextMeshProUGUI>(),
                PriceIcon = uiUnitInstance.GetChild(0).GetChild(0).GetComponent<Image>()
            });

            resourceUnitsMap[kvp.Key].PriceText.text = $"{kvp.Value}";
            resourceUnitsMap[kvp.Key].PriceIcon.sprite = ResourceSpriteStorage.Instance.GetIcon(kvp.Key);

            if (kvp.Value == 0)
            {
                resourceUnitsMap[kvp.Key].PriceText.transform.parent.gameObject.SetActive(false);
            }
        }

        priceUnit.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(Register());
    }

    private void Start()
    {
        InitializePriceUI();

        PlayerLevelManager.Instance.OnLevelUp += OnLevelUp;

        if (transform.parent.TryGetComponent<PayoutValidation>(out var payoutValidation))
        {
            payoutValidation.OnPayoutValidationComplete += StartRegisterCoroutine;
        }

        transform.localScale *= 1.2f;
    }

    private void LockInteractorIfLevelIsLow()
    {
        if (unlockLevel != 0)
        {
            if (PlayerLevelManager.Instance.Level < unlockLevel)
            {
                trigger.enabled = false;

                if (unlockAtLevel)
                    unlockAtLevel.gameObject.SetActive(true);
                priceCanvas.gameObject.SetActive(false);
                if (unlockLevelText)
                    unlockLevelText.text = $"{unlockLevel}";
                return;
            }
        }
        if (unlockAtLevel != null)
            unlockAtLevel.gameObject.SetActive(false);
    }

    private void StartRegisterCoroutine()
    {
        StartCoroutine(Register());
    }

    private IEnumerator Register()
    {
        yield return null;

        LockInteractorIfLevelIsLow();

        yield return null;

        if (!isPayedOut && transform.parent.localScale != Vector3.zero && IsUnlocked)
        {
            if (isUpgrade)
            {
                QuestTargetSystem.Instance.AddUpgrade(buildingType, transform);
            }
            else
            {
                QuestTargetSystem.Instance.AddBuilding(buildingType, transform);
            }
        }
    }

    public void Interact(Transform interactor)
    {
        foreach (var key in priceMap.Keys.ToList())
        {
            bool canPay = ResourceStorage.Instance.GetResourceBalance(key) > 0 && priceReserveMap[key] > 0;
            if (canPay)
            {
                priceReserveMap[key]--;

                ResourceStorage.Instance.ChangeResourceAmount(key, -1);

                PoolableObject resourceInstance = PoolingSystem.Instance.GetResourcePool(key).GetObject();
                InitializeResourceUnit(resourceInstance as ResourceUnit);
            }
        }
        interactionCofficient++;
    }

    private void InitializeResourceUnit(ResourceUnit resourceUnit)
    {
        resourceUnit.SetDestination(PlayerController.Instance.GetInteractionTarget().GetPosition());
        resourceUnit.SetJumpDuration(0.7f);
        resourceUnit.OnMotionEnd += PayOneUnit;
        resourceUnit.transform.position = PlayerController.Instance.transform.position;
        resourceUnit.gameObject.SetActive(true);
    }

    private void PayOneUnit(ResourceUnit unit)
    {
        ResourceTypes key = unit.GetResourceType();
        priceMap[key]--;
        resourceUnitsMap[key].PriceText.text = $"{priceMap[key]}";

        if (priceMap[key] <= 0)
        {
            resourceUnitsMap[key].PriceText.transform.parent.gameObject.SetActive(false);
        }

        SavePriceMapValues();

        bool payedOut = true;
        foreach (var key1 in priceMap.Keys.ToList())
        {
            payedOut = payedOut && priceMap[key1] == 0;
        }

        if (payedOut && gameObject.activeSelf)
        {
            PlayerPrefs.SetInt($"IsPayedOut{uniqueID}", 1);

            foreach (var payoutValidation in payoutValidations)
            {
                if (payoutValidation.gameObject.activeSelf || payoutValidation.transform.GetComponent<RewardedItem>() == null)
                    payoutValidation.OnPayout();
            }

            if (isUpgrade)
            {
                QuestEvents.FireOnUpgrade(buildingType);
                QuestTargetSystem.Instance.Discardupgrade(buildingType, transform);
            }
            else
            {
                QuestEvents.FireOnBuilding(buildingType);
                QuestTargetSystem.Instance.DiscardBuilding(buildingType, transform);
            }

            gameObject.SetActive(false);
        }
    }

    private void SavePriceMapValues()
    {
        foreach (var kvp in priceAsset.prices)
        {
            PlayerPrefs.SetFloat($"{uniqueID}{(int)kvp.Type}", priceMap[kvp.Type]);
        }
    }

    public Vector3 GetPosition() => objectTransform.position;

    public void SetActive(bool state) => gameObject.SetActive(state);

    public float GetInteractionTimerMax()
    {
        float interactionTime = interactionTimerMax;
        interactionTime /= 1 + interactionCofficient / 5;
        return interactionTime;
    }

    public void DisableTrigger()
    {
        trigger.enabled = false;
    }

    public bool CanInteractInMotion() => canInteractInMotion;
}

[System.Serializable]
public class ResourceTypeIconPair
{
    public ResourceTypes Type;
    public Sprite Icon;
}

public class ResourceUIUnit
{
    public TextMeshProUGUI PriceText;
    public Image PriceIcon;
}