using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealForAd : BonusForAd
{
    protected override void GiveRewardResult()
    {
        PlayerController.Instance.RestoreHealth();
    }
}
