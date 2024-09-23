using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Price Asset", menuName = "Configs/PriceAsset")]
public class PriceAsset : ScriptableObject
{
    [SerializeField] public List<PriceUnit> prices;
}

[System.Serializable]
public class PriceUnit
{
    public ResourceTypes Type;
    public float Price;
}
