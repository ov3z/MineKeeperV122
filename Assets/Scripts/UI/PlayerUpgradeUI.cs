using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUpgradeUI : MonoBehaviour,ICoinTradeItem
{
    [SerializeField] private string upgradeKey;
    [SerializeField] private TextMeshProUGUI upgradeLevelText;
    [SerializeField] private TextMeshProUGUI currentStatText;
    [SerializeField] private TextMeshProUGUI statAfterUpgradeText;
    [SerializeField] private TextMeshProUGUI upgradePriceText;
    [SerializeField] private int upgradeIncrement;
    [SerializeField] private int priceIncrement = 50;
    [SerializeField] private UpgradeButtonTypes buttonType;
    [SerializeField] private Transform buttonLockPanel;
    [SerializeField] private Button buyButton;

    private int upgradePrice = 150;
    private int upgradeLevel = 1;
    private int stat;

    public float Price => upgradePrice;
    public Button BuyButton => buyButton;

    private void Start()
    {
        QuestTargetSystem.Instance.RegisterUpgardeButton(buttonType, transform);
        ResourceStorage.Instance.OnResourceAmountChange += OnResourceAmountChange;

        stat = (int)PlayerPrefs.GetFloat(upgradeKey);
        upgradeLevel = PlayerPrefs.GetInt("Level" + upgradeKey, 1);
        upgradePrice += upgradeLevel * priceIncrement;
        UpdateUI();
        CheckIsThereEnoughMoney();
    }

    private void OnResourceAmountChange(ResourceTypes type, float amount)
    {
        if (type == ResourceTypes.Coins)
        {
            CheckIsThereEnoughMoney();
        }
    }

    private void CheckIsThereEnoughMoney()
    {
        float amount = ResourceStorage.Instance.GetResourceBalance(ResourceTypes.Coins);
        if (amount >= upgradePrice)
        {
            buttonLockPanel.gameObject.SetActive(false);
        }
        else
        {
            buttonLockPanel.gameObject.SetActive(true);
        }
    }

    private void UpdateUI()
    {
        upgradeLevelText.text = $"lvl {upgradeLevel}";
        currentStatText.text = $"{stat}";
        statAfterUpgradeText.text = $"{stat + upgradeIncrement}";
        upgradePriceText.text = $"{upgradePrice + (upgradeLevel - 1) * priceIncrement}";
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
        QuestEvents.FireOnPlayerUpgrade(buttonType);

        upgradeLevel++;
        stat += upgradeIncrement;

        UpdateUI();
        Save();

        if (upgradeKey == "Axe")
            PlayerController.Instance.SetGatherPower(stat);
        if (upgradeKey == "Capacity")
            InventoryController.Instance.UpdateCapacityMax();
    }

    private void Save()
    {
        PlayerPrefs.SetFloat(upgradeKey, stat);
        PlayerPrefs.SetInt("Level" + upgradeKey, upgradeLevel);
    }
}
