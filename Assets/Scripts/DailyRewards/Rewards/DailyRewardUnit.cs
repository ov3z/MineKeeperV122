using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Daily Reward Unit", menuName = "Rewards/DailyReward")]
public class DailyRewardUnit : ScriptableObject
{
    [SerializeField] public List<SerializablePair<DailyRewardTypes, int>> rewards;
}

public enum DailyRewardTypes
{
    Coins,
    Strawberry,
    Woood,
    Emerald,
    Wheat,
    Beer,
    Dragon
}

public static class DailyRewardToResourceConverter
{
    public static ResourceTypes Convert(DailyRewardTypes reward) => reward switch
    {
        DailyRewardTypes.Coins => ResourceTypes.Coins,
        DailyRewardTypes.Strawberry => ResourceTypes.Blueberry,
        DailyRewardTypes.Woood => ResourceTypes.Wood,
        DailyRewardTypes.Wheat => ResourceTypes.Wheat,
        DailyRewardTypes.Beer => ResourceTypes.Beer,
        DailyRewardTypes.Emerald => ResourceTypes.Emerald,
        _ => ResourceTypes.XP
    };
}