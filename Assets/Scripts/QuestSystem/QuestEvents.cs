using System;
using UnityEngine.UI;

public static class QuestEvents
{
    public static event Action<ResourceTypes, float> OnResourceEarn;
    public static event Action<ResourceTypes, float> OnResourceSpend;
    public static event Action<BuildingTypes> OnBuilding;
    public static event Action<BuildingTypes> OnUpgrade;
    public static event Action<UpgradeButtonTypes> OnPlayerUpgarde;
    public static event Action<float> OnEarnXPForLevel;
    public static event Action<int> OnWinBattle;
    public static event Action OnFollowCompletion;
    public static event Action<int> OnEnemyKill;
    public static event Action<PetType> OnPetBuy;

    public static void FireOnResourceEarn(ResourceTypes earningType, float amount) => OnResourceEarn?.Invoke(earningType, amount);
    public static void FireOnResourceSpend(ResourceTypes earningType, float amount) => OnResourceSpend?.Invoke(earningType, amount);
    public static void FireOnBuilding(BuildingTypes buildingType) => OnBuilding?.Invoke(buildingType);
    public static void FireOnUpgrade(BuildingTypes buildingType) => OnUpgrade?.Invoke(buildingType);
    public static void FireOnEarnXPForLevel(float amount) => OnEarnXPForLevel?.Invoke(amount);
    public static void FireOnWinBattle(int battleCount) => OnWinBattle?.Invoke(battleCount);
    public static void FireOnFollowCompletion() => OnFollowCompletion?.Invoke();
    public static void FireOnEnemyKill(int enemyCount) => OnEnemyKill?.Invoke(enemyCount);
    public static void FireOnPlayerUpgrade(UpgradeButtonTypes upgradeType) => OnPlayerUpgarde?.Invoke(upgradeType);
    public static void FireOnPetBuy(PetType petType) => OnPetBuy?.Invoke(petType);
}

public enum BuildingTypes
{
    House,
    FruitStand,
    BlueberryBush,
    SwordsmenBarrack,
    BlueberryStorage,
    TownHall,
    SpearmenBarracks,
    Tavern,
    WheatStorage,
    WheatPlace,
    Farmer,
    ForestExpansion,
    Fountain,
    WheatArea,
    BrewMachine,
    WoodArea,
    WoodStorage,
    PetHouse,
    Trader,
    Refiner,
    SpecialChest
}