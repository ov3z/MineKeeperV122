using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI inventoryTextInside;
    [SerializeField] private TextMeshProUGUI inventoryTextOutside;
    [SerializeField] private TextMeshProUGUI inventoryTextOutsideFill;
    [SerializeField] private Transform uiUnitsParent;
    [SerializeField] private Transform exclamationMark;

    private List<InventoryUIUnit> inventoryUIUnits = new();
    private Dictionary<ResourceTypes, InventoryUIUnit> inventoryUnitsMap = new();

    private int inventoryCapacityMax => inventoryController.InventoryCapacityMax;
    private int itemsInInventory => inventoryController.ItemsInInventory;
    private InventoryController inventoryController;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        inventoryController = InventoryController.Instance;

        InitializeUnitsList();
        LoadInventroyUI();
        ResourceStorage.Instance.OnResourceAmountChange += UpdateInventoryUI;
    }

    private void InitializeUnitsList()
    {
        foreach (var unit in uiUnitsParent.GetComponentsInChildren<InventoryUIUnit>())
        {
            inventoryUIUnits.Add(unit);
        }
    }

    private void LoadInventroyUI()
    {
        foreach (ResourceTypes type in Enum.GetValues(typeof(ResourceTypes)))
        {
            float balance;
            if (type != ResourceTypes.Coins && type != ResourceTypes.XP)
            {
                balance = PlayerPrefs.GetFloat($"Resource{(int)type}", 0);
                inventoryController.ChangeItemCountInInventory((int)balance);

                if (balance > 0)
                {
                    CreateNewUIUnitAndSetUp(type, balance);
                }
            }
        }

        UpdateInventoryText();
    }

    private void UpdateInventoryUI(ResourceTypes type, float amount)
    {
        if (type == ResourceTypes.Coins) return;

        int increment;
        if (inventoryUnitsMap.TryGetValue(type, out var uiUnit))
        {
            increment = uiUnit.GetIncrement((int)amount);
            uiUnit.SetAmount((int)amount);
            uiUnit.SetIconActive(amount > 0);
        }
        else
        {
            CreateNewUIUnitAndSetUp(type, amount);
            increment = (int)amount;
        }

        inventoryController.ChangeItemCountInInventory(increment);
        UpdateInventoryText();
    }

    public void UpdateInventoryText()
    {
        inventoryTextInside.text = $"{itemsInInventory}/{inventoryCapacityMax}";
        inventoryTextOutside.text = inventoryTextInside.text;
        inventoryTextOutsideFill.text = inventoryTextInside.text;
        if (itemsInInventory == inventoryCapacityMax)
        {
            exclamationMark.gameObject.SetActive(true);
        }
        else if (exclamationMark.gameObject.activeSelf)
        {
            exclamationMark.gameObject.SetActive(false);
        }
    }

    private void CreateNewUIUnitAndSetUp(ResourceTypes type, float balance)
    {
        var newUnit = inventoryUIUnits[0];
        inventoryUIUnits.RemoveAt(0);

        inventoryUnitsMap.Add(type, newUnit);

        newUnit.SetIcon(ResourceSpriteStorage.Instance.GetIcon(type));
        newUnit.SetAmount((int)balance);
        newUnit.SetIconActive(balance > 0);
    }
}