using System.Collections.Generic;
using UnityEngine;

public class QuestTargetSystem : MonoBehaviour
{
    public static QuestTargetSystem Instance { get; private set; }

    [SerializeField] private Transform goToMineButton;
    [SerializeField] private Transform sellPlatform;
    [SerializeField] private Transform traderStation;
    [SerializeField] private Transform tradeStationCoinStack;

    private Dictionary<ResourceTypes, List<Transform>> resourceTargetsMap = new();
    private Dictionary<BuildingTypes, List<Transform>> buildingTargetsMap = new();
    private Dictionary<BuildingTypes, List<Transform>> upgradeTargetsMap = new();
    private Dictionary<UpgradeButtonTypes, Transform> upgradeButtonsMap = new();
    private Dictionary<PetType, Transform> petTypeMap = new();

    private void Awake()
    {
        Instance = this;
    }

    public void AddResource(ResourceTypes resourceType, Transform target)
    {
        if (!resourceTargetsMap.ContainsKey(resourceType))
        {
            resourceTargetsMap.Add(resourceType, new List<Transform>());
        }
        resourceTargetsMap[resourceType].Add(target);
    }

    public void DiscardResource(ResourceTypes resourceType, Transform target)
    {
        resourceTargetsMap[resourceType].Remove(target);
    }

    public void AddBuilding(BuildingTypes buildingType, Transform target)
    {
        if (!buildingTargetsMap.ContainsKey(buildingType))
        {
            buildingTargetsMap.Add(buildingType, new List<Transform>());
        }
        buildingTargetsMap[buildingType].Add(target);
    }

    public void DiscardBuilding(BuildingTypes buildingType, Transform target)
    {
        buildingTargetsMap[buildingType].Remove(target);
    }

    public void AddUpgrade(BuildingTypes buildingType, Transform target)
    {
        if (!upgradeTargetsMap.ContainsKey(buildingType))
        {
            upgradeTargetsMap.Add(buildingType, new List<Transform>());
        }
        upgradeTargetsMap[buildingType].Add(target);
    }

    public void Discardupgrade(BuildingTypes buildingType, Transform target)
    {
        upgradeTargetsMap[buildingType].Remove(target);
    }

    public Transform GetBuildingTarget(BuildingTypes buildingType, Vector3 requestPosition)
    {
        Transform target = null;

        if (buildingTargetsMap.ContainsKey(buildingType))
        {
            float closestDistance = float.MaxValue;

            foreach (var building in buildingTargetsMap[buildingType])
            {
                float distance = Vector3.Distance(requestPosition, building.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    target = building;
                }
            }
        }

        return target;
    }

    public Transform GetUpgradeTarget(BuildingTypes buildingType, Vector3 requestPosition)
    {
        Transform target = null;

        if (upgradeTargetsMap.ContainsKey(buildingType))
        {
            float closestDistance = float.MaxValue;

            foreach (var building in upgradeTargetsMap[buildingType])
            {
                float distance = Vector3.Distance(requestPosition, building.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    target = building;
                }
            }
        }

        return target;
    }

    public Transform GetEarningTarget(ResourceTypes resourceType, Vector3 requestPosition, List<EarningType> earningType)
    {
        Transform target = null;

        if (resourceType == ResourceTypes.Coins)
        {
            if (earningType.Count == 1)
            {
                switch (earningType[0])
                {
                    case EarningType.TradeStation:
                        target = tradeStationCoinStack;
                        break;
                    case EarningType.Trader:
                        target = traderStation;
                        break;
                }
            }
            else
            {
                float distanceToTrader = Vector3.Distance(requestPosition, traderStation.position);
                float disanceToTradeStation = Vector3.Distance(requestPosition, tradeStationCoinStack.position);

                target = traderStation;

                if (disanceToTradeStation < distanceToTrader)
                    target = tradeStationCoinStack;
            }

        }
        else
        {
            if (resourceTargetsMap.ContainsKey(resourceType))
            {
                float closestDistance = float.MaxValue;

                foreach (var resource in resourceTargetsMap[resourceType])
                {
                    float distance = Vector3.Distance(requestPosition, resource.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        target = resource;
                    }
                }
            }
        }

        return target;
    }

    public Transform GetGoToMineTarget()
    {
        return goToMineButton;
    }

    public Transform GetSpendingTarget(SpendingType spendingType) => spendingType switch
    {
        SpendingType.TradeStation => sellPlatform,
        SpendingType.Trader => traderStation,
        _ => null
    };

    public void RegisterUpgardeButton(UpgradeButtonTypes buttonType, Transform transform)
    {
        upgradeButtonsMap.Add(buttonType, transform);
    }

    public Transform GetUpgradeButton(UpgradeButtonTypes buttonType)
    {
        return upgradeButtonsMap[buttonType];
    }
    public void RegisterPetButton(PetType buttonType, Transform transform)
    {
        petTypeMap.Add(buttonType, transform);
    }

    public Transform GetPetButton(PetType buttonType)
    {
        return petTypeMap[buttonType];
    }
}

public enum EarningType
{
    Resource,
    TradeStation,
    Trader
}

public enum SpendingType
{
    TradeStation,
    Trader
}
