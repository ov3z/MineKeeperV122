using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SoldierUpgradeManager : MonoBehaviour
{
    public event Action<SoldierType> OnSoldierUpgrade;
    public static SoldierUpgradeManager Instance { get; private set; }

    [SerializeField] private List<UpgradeUIUnit> upgradeList = new List<UpgradeUIUnit>();

    private Dictionary<SoldierType, List<Transform>> soldierUpgradeMap = new();
    private Dictionary<SoldierType, int> soldierLevelMap = new();
    private Dictionary<SoldierType, TextMeshProUGUI> squadPowerTextMap = new();
    private Dictionary<SoldierType, TextMeshProUGUI> upgradePriceText = new();
    private Dictionary<SoldierType, int> soldierUpgradePrice = new();

    private void Awake()
    {
        Instance = this;
        InitializeMaps();
    }

    private void Start()
    {
        EnableSuadVisualForUpgrade();
    }

    private void EnableSuadVisualForUpgrade()
    {
        foreach (var soldierType in soldierLevelMap.Keys.ToList())
        {
            int soldierSquadIndex = Mathf.Clamp(soldierLevelMap[soldierType], 0, 1);
            soldierUpgradeMap[soldierType][soldierSquadIndex].gameObject.SetActive(true);

            SetSquadPowerText(soldierType);

            upgradePriceText[soldierType].text = $"{soldierUpgradePrice[soldierType]}";
        }
    }

    private void SetSquadPowerText(SoldierType soldierType)
    {
        int soldierLevel = PlayerPrefs.GetInt($"UpgradeLevel{(int)soldierType}", 0);
        int soldierCount = soldierLevel == 0 ? 2 : 3;
        int squadPower = soldierCount * (soldierLevel + 1);
        squadPowerTextMap[soldierType].text = $"Squad Power {squadPower}";
    }

    private void InitializeMaps()
    {
        foreach (var unit in upgradeList)
        {
            soldierUpgradeMap.Add(unit.type, unit.upgradeSteps);
            soldierLevelMap.Add(unit.type, PlayerPrefs.GetInt($"UpgradeLevel{(int)unit.type}", 0));
            squadPowerTextMap.Add(unit.type, unit.squadPowerText);
            upgradePriceText.Add(unit.type, unit.upgradePrice);
            soldierUpgradePrice.Add(unit.type, 75 + 200 * soldierLevelMap[unit.type]);
        }
    }

    public void UpgradeSoldier(int soldierType)
    {
        SoldierType soldierForUpgrade = GetSoldierType(soldierType);

        if (soldierUpgradePrice[soldierForUpgrade] <= ResourceStorage.Instance.GetResourceBalance(ResourceTypes.Coins))
        {
            ResourceStorage.Instance.ChangeResourceAmount(ResourceTypes.Coins, -soldierUpgradePrice[soldierForUpgrade]);
            AquireSoldier(soldierType);
        }
    }

    public void AquireSoldier(int soldierType)
    {
        SoldierType soldierForUpgrade = GetSoldierType(soldierType);
        soldierLevelMap[soldierForUpgrade]++;

        if (soldierLevelMap[soldierForUpgrade] == 1)
        {
            soldierUpgradeMap[soldierForUpgrade][0].gameObject.SetActive(false);
            soldierUpgradeMap[soldierForUpgrade][1].gameObject.SetActive(true);
            PlayerPrefs.SetInt($"Ground{(int)soldierForUpgrade}Tier", 1);
            OnSoldierUpgrade?.Invoke(soldierForUpgrade);
        }

        SaveSolldierUpgrade(soldierForUpgrade);

        SetSquadPowerText(soldierForUpgrade);

        soldierUpgradePrice[soldierForUpgrade] = 75 + 200 * soldierLevelMap[soldierForUpgrade];
        upgradePriceText[soldierForUpgrade].text = $"{soldierUpgradePrice[soldierForUpgrade]}";

        QuestEvents.FireOnUpgrade(GetBuildingType(soldierForUpgrade));
    }

    private SoldierType GetSoldierType(int soldierType) => soldierType switch
    {
        1 => SoldierType.Swordsmen,
        2 => SoldierType.Spearmen,
        _ => SoldierType.Swordsmen
    };

    private BuildingTypes GetBuildingType(SoldierType soldierType) => soldierType switch
    {
        SoldierType.Spearmen => BuildingTypes.SpearmenBarracks,
        SoldierType.Swordsmen => BuildingTypes.SwordsmenBarrack,
        _ => BuildingTypes.SwordsmenBarrack,
    };

    private void SaveSolldierUpgrade(SoldierType soldierType)
    {
        PlayerPrefs.SetInt($"UpgradeLevel{(int)soldierType}", soldierLevelMap[soldierType]);
    }
}

[System.Serializable]
public class UpgradeUIUnit
{
    public SoldierType type;
    public List<Transform> upgradeSteps;
    public TextMeshProUGUI squadPowerText;
    public TextMeshProUGUI upgradePrice;
}