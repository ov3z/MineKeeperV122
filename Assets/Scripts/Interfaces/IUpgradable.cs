using UnityEngine;

public interface IUpgradable
{
    public void Upgrade();
    public float GetCurrentStat();
    public float GetUpgradeIncrement();
    public string GetID();
    public bool IsUnlocked();
}
