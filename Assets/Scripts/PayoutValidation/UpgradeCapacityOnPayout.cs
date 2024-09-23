using UnityEngine;

public class UpgradeCapacityOnPayout : PayoutValidation
{
    [SerializeField] private GameObject upgradeSubject;

    public override void OnPayout()
    {
        upgradeSubject.GetComponent<IUpgradable>().Upgrade();
    }
}
