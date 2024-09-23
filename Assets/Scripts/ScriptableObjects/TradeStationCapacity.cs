using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Trade Station Capacity", menuName = "Stats/TradeStationCapacity")]
public class TradeStationCapacity : ScriptableObject
{
    public List<ResourceCapacityUnit> capacities = new List<ResourceCapacityUnit>();

    public void Initialize()
    {
        
    }
}

[System.Serializable]
public class ResourceCapacityUnit
{
    public ResourceTypes Type;
    public float Capacity;
}
