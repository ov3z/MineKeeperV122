using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Reward", menuName = "QuestSystem/Reward")]
public class Reward : ScriptableObject
{
    public List<RewardUnit> rewards;
}
[System.Serializable]
public class RewardUnit
{
    public ResourceTypes Type;
    public float Amount;
}