using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetUIUnit : MonoBehaviour,ICoinTradeItem
{
    [SerializeField] private int price;
    [SerializeField] protected int capacityBonus;
    [SerializeField] protected Button buyButton;
    [SerializeField] protected Button selectButton;
    [SerializeField] protected TextMeshProUGUI priceText;
    [SerializeField] protected TextMeshProUGUI capacityBonusText;
    [SerializeField] protected Sprite selectedButtonSprite;
    [SerializeField] protected Sprite unlockedButtonSprite;
    [SerializeField] protected PetType petType;
    [SerializeField] protected GuidComponent guidComponent;
    [SerializeField] protected UpgradePanel petPanel;

    protected PetSelectionState currentState;
    private string uniqueID => guidComponent.GetGuid().ToString();

    public float Price => price;
    public Button BuyButton => buyButton;   

    protected virtual IEnumerator Start()
    {
        petPanel.OnOpen += Initialize;
        yield return null;
        Initialize();
        QuestTargetSystem.Instance.RegisterPetButton(petType, transform);
    }

    protected virtual void Initialize()
    {
        LoadUIState();
        InitializeTexts();
    }

    protected void LoadUIState()
    {
        var isUnlocked = PlayerPrefs.GetInt($"IsUnlocked{uniqueID}", 0) == 1 ? true : false;
        var isSelected = PlayerPrefs.GetInt($"IsSelected{uniqueID}", 0) == 1 ? true : false;

        if (isSelected)
        {
            currentState = PetSelectionState.Selected;
            DisableBuyButton();
            Select();
        }
        else if (isUnlocked)
        {
            currentState = PetSelectionState.Unlocled;
            DisableBuyButton();
        }
        else
            currentState = PetSelectionState.Locked;
    }

    protected virtual void InitializeTexts()
    {
        capacityBonusText.text = $"{capacityBonus}";
        priceText.text = $"{price}";
    }

    protected virtual void DisableBuyButton()
    {
        buyButton.gameObject.SetActive(false);
        selectButton.gameObject.SetActive(true);
        selectButton.transform.GetComponent<Image>().sprite = unlockedButtonSprite;
    }

    public void Buy()
    {
        if (price <= ResourceStorage.Instance.GetResourceBalance(ResourceTypes.Coins))
        {
            ResourceStorage.Instance.ChangeResourceAmount(ResourceTypes.Coins, -price);
            Aquire();
        }
    }

    public void Aquire()
    {
        buyButton.gameObject.SetActive(false);
        PlayerPrefs.SetInt($"IsUnlocked{uniqueID}", 1);
        selectButton.gameObject.SetActive(true);
        QuestEvents.FireOnPetBuy(petType);
        Select();
    }

    public void Select()
    {
        PlayerPrefs.SetInt($"IsSelected{uniqueID}", 1);
        PlayerPrefs.SetFloat("Capacity", PlayerPrefs.GetFloat("Capacity", 100) + capacityBonus);
        PlayerPrefs.SetFloat("CapacityBonus", capacityBonus);
        PetHouse.Instance.SelectPet(petType, this);
        selectButton.transform.GetComponent<Image>().sprite = selectedButtonSprite;
        InventoryController.Instance.UpdateCapacityMax();
    }

    public void Deselect()
    {
        PlayerPrefs.SetInt($"IsSelected{uniqueID}", 0);
        PlayerPrefs.SetFloat("Capacity", PlayerPrefs.GetFloat("Capacity", 100) - capacityBonus);
        selectButton.transform.GetComponent<Image>().sprite = unlockedButtonSprite;
    }
}

public enum PetSelectionState
{
    Locked,
    Unlocled,
    Selected
}
