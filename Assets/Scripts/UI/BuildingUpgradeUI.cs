using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUpgradeUI : MonoBehaviour, ICoinTradeItem
{
    [SerializeField] private UpgradePanel parentPanel;
    [SerializeField] private GameObject upgradeTargetObject;
    [SerializeField] private Transform lockedPanel;
    [SerializeField] private Transform buttonLockPanel;
    [SerializeField] private TextMeshProUGUI upgradeLevelText;
    [SerializeField] private TextMeshProUGUI currentStatText;
    [SerializeField] private TextMeshProUGUI statAfterUpgradeText;
    [SerializeField] private TextMeshProUGUI upgradePriceText;
    [SerializeField] private int priceIncrement = 50;
    [SerializeField] private BuildingTypes upgradeType;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private UpgradeButtonTypes buttonType;

    private int upgradePrice => 75 + (upgradeLevel - 1) * priceIncrement;
    private int upgradeLevel = 1;
    private int stat;
    private string id;
    private int upgradeIncrement;
    private IUpgradable upgradeTarget;

    public float Price => upgradePrice;
    public Button BuyButton => upgradeButton;
    public bool IsUnlocked => lockedPanel.gameObject.activeSelf == false;

    private void Start()
    {
        parentPanel.OnOpen += OnOpen;
        ResourceStorage.Instance.OnResourceAmountChange += OnResourceChange;
        upgradeTarget = upgradeTargetObject.GetComponent<IUpgradable>();
        QuestTargetSystem.Instance.RegisterUpgardeButton(buttonType, transform);
    }

    private void OnOpen()
    {
        if (!upgradeTarget.IsUnlocked())
        {
            lockedPanel.gameObject.SetActive(true);
        }
        else
        {
            lockedPanel.gameObject.SetActive(false);
        }

        stat = (int)upgradeTarget.GetCurrentStat();
        upgradeIncrement = (int)upgradeTarget.GetUpgradeIncrement();

        id = upgradeTarget.GetID();
        upgradeLevel = PlayerPrefs.GetInt("Level" + id, 1);
        UpdateUI();

        CheckIsThereEnoughMoney();
    }

    private void CheckIsThereEnoughMoney()
    {
        if (!lockedPanel.gameObject.activeSelf)
        {
            bool isThereEnoughMoney = upgradePrice <= ResourceStorage.Instance.GetResourceBalance(ResourceTypes.Coins);
            if (isThereEnoughMoney)
            {
                buttonLockPanel.gameObject.SetActive(false);
            }
            else
            {
                buttonLockPanel.gameObject.SetActive(true);
            }
        }
    }

    private void OnResourceChange(ResourceTypes type, float amount)
    {
        if (type == ResourceTypes.Coins)
        {
            CheckIsThereEnoughMoney();
        }
    }

    private void UpdateUI()
    {
        upgradeLevelText.text = $"lvl {upgradeLevel}";
        currentStatText.text = $"{stat}";
        statAfterUpgradeText.text = $"{stat + upgradeIncrement}";
        upgradePriceText.text = $"{upgradePrice}";
    }

    public void Upgrade()
    {
        if (ResourceStorage.Instance.GetResourceBalance(ResourceTypes.Coins) >= upgradePrice)
        {
            ResourceStorage.Instance.ChangeResourceAmount(ResourceTypes.Coins, -upgradePrice);
            Aquire();
        }
    }

    public void Aquire()
    {
        QuestEvents.FireOnUpgrade(upgradeType);

        Debug.Log(upgradeType);

        upgradeLevel++;
        stat += upgradeIncrement;

        upgradeTarget.Upgrade();

        UpdateUI();
        Save();
    }

    private void OnDestroy()
    {
        parentPanel.OnOpen -= OnOpen;
        ResourceStorage.Instance.OnResourceAmountChange -= OnResourceChange;
    }

    private void Save()
    {
        PlayerPrefs.SetInt("Level" + id, upgradeLevel);
    }
}
