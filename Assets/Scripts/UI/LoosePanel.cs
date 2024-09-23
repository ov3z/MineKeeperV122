using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoosePanel : MonoBehaviour
{
    [SerializeField] private Transform getX2Button;
    [SerializeField] private Transform claimButton;
    [SerializeField] private Transform victory;

    [SerializeField] private Transform inGround;
    [SerializeField] private List<Transform> stars;
    [SerializeField] private Transform rewards;
    [SerializeField] private Transform levelUi;
    [SerializeField] private Canvas canvas;

    [SerializeField] private Transform noThanksButton;

    [SerializeField] private List<RewardUnit> rewardUnits;

    private void Start()
    {
        Debug.Log("start");

        levelUi.DOScale(0, 0.3f).SetEase(Ease.Linear);

        var initialScale = transform.GetChild(0).localScale;
        transform.GetChild(0).localScale = initialScale * 0.4f;
        transform.GetChild(0).DOScale(initialScale, 0.4f).SetEase(Ease.Linear);

#if ADS
        MakePopUp(getX2Button, Vector3.one); 
        claimButton.gameObject.SetActive(false);
#else
        MakePopUp(claimButton, Vector3.one);
        getX2Button.gameObject.SetActive(false);
#endif

        MakeShortPopUp(victory, Vector3.one);

        foreach (var star in stars)
        {
            MakePopUp(star, Vector3.one);
        }

        MakePopUp(inGround, Vector3.up);

        MakePopUp(rewards, Vector3.one, 1f);

#if ADS
        noThanksButton.GetComponent<Image>().DOFade(1, 0.4f).SetEase(Ease.Linear)/*.SetDelay(3f)*/.OnStart(() =>
        {
            noThanksButton.gameObject.SetActive(true);
        }); 
#endif

        int rewardsCount = 0;
        float delay = 1.6f;

        foreach (var rewardUnit in rewardUnits)
        {
            float change = CaveGameManager.Instance.GetResourceChange(rewardUnit.type);
            rewardUnit.amount.text = $"{CaveGameManager.Instance.GetResourceChange(rewardUnit.type)}";
            if (change <= 0)
            {
                rewardUnit.amount.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                rewardsCount++;
                delay = 1.6f + 0.2f * (rewardsCount);
                StartCoroutine(WaitForPopUpRewardUnits(delay, rewardUnit));
                MakePopUp(rewardUnit.amount.transform.parent, Vector3.one, delay);
            }
        }
    }

    private IEnumerator WaitForPopUpRewardUnits(float delay, RewardUnit rewardUnit)
    {
        RewardUnit localUnit = rewardUnit;
        yield return new WaitForSeconds(delay);
        Transform rewardParticle = localUnit.amount.transform.parent.GetLastChild();
        rewardParticle.gameObject.SetActive(true);
        rewardParticle.SetParent(canvas.transform);
        rewardParticle.localScale = Vector3.one;
    }

    public void MakePopUp(Transform target, Vector3 dir, float initialDelay = .4f)
    {
        target.transform.DOScale(1.05f * dir + (Vector3.one - dir), 0.5f).SetDelay(initialDelay).SetEase(Ease.Linear).OnComplete(() =>
        {
            target.transform.DOScale(0.95f * dir + (Vector3.one - dir), 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                target.transform.DOScale(1.025f * dir + (Vector3.one - dir), 0.15f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    target.transform.DOScale(1f, 0.05f).SetEase(Ease.Linear);
                });
            });
        });
    }

    public void MakeShortPopUp(Transform target, Vector3 dir, float initialDelay = 0.4f)
    {
        target.transform.DOScale(1.05f * dir + (Vector3.one - dir), 0.15f).SetDelay(initialDelay).SetEase(Ease.Linear).OnComplete(() =>
        {
            target.transform.DOScale(0.95f * dir + (Vector3.one - dir), 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                target.transform.DOScale(1.025f * dir + (Vector3.one - dir), 0.15f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    target.transform.DOScale(1f, 0.1f).SetEase(Ease.Linear);
                });
            });
        });
    }

    public void ShowRewarded()
    {
        void OnSucceed()
        {
            MultiplyRewards();
            CaveTraveller.Instance.LoadCaveLoadingScene();
        }
        void OnFailed()
        {
            CaveTraveller.Instance.LoadCaveLoadingScene();
        }

       // AdsContainer.Instance.ShowRewarded(OnSucceed, OnFailed);
        AdsContainer.Instance.ShowRewardedYso(OnSucceed, OnFailed);//yso rewarded
    }

    private void MultiplyRewards()
    {
        foreach (var rewardUnit in rewardUnits)
        {
            var change = CaveGameManager.Instance.GetResourceChange(rewardUnit.type);
            if (change > 0)
            {
                ResourceStorage.Instance.ChangeResourceAmount(rewardUnit.type, (int)change);
            }
        }
    }

    [System.Serializable]
    public class RewardUnit
    {
        public ResourceTypes type;
        public TextMeshProUGUI amount;
    }
}
