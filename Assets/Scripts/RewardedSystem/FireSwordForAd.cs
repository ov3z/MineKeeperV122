
public class FireSwordForAd : BonusForAd
{
    protected override void GiveRewardResult()
    {
        GameManager.Instance.GetFireSword();
    }
}
