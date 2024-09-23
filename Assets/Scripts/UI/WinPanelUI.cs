using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinPanelUI : MonoBehaviour
{
    [SerializeField] private Transform getX2Button;
    [SerializeField] private Transform claimButton;
    [SerializeField] private Transform victory;
    [SerializeField] private Transform victoryText;
    [SerializeField] private Transform inGround;
    [SerializeField] private List<Transform> stars;
    [SerializeField] private Transform rewards;
    [SerializeField] private Transform xpBar;
    [SerializeField] private Transform levelUi;
    [SerializeField] private Canvas canvas;

    [SerializeField] private Transform noThanksButton;

    [SerializeField] private List<RewardUnit> rewardUnits;

    [SerializeField] private TextMeshProUGUI xpReward;

    private void Start()
    {
        levelUi.DOScale(0, 0.3f).SetEase(Ease.Linear);

        ShowConfettiParticle();

        var rectangle = transform.GetChild(1);
        var shine = transform.GetChild(0);

        rectangle.localScale = Vector3.one * 0.4f;
        rectangle.DOScale(Vector3.one, 0.4f).SetEase(Ease.Linear);

        shine.localScale = Vector3.one * 0.4f;
        shine.DOScale(Vector3.one, 0.4f).SetEase(Ease.Linear);

#if ADS
        MakePopUp(getX2Button, Vector3.one); 
        claimButton.gameObject.SetActive(false);
#else
        MakePopUp(claimButton, Vector3.one);
        getX2Button.gameObject.SetActive(false);
#endif
        MakePopUp(victoryText, Vector3.one);
        MakePopUp(xpBar, Vector3.one);
        MakeShortPopUp(victory, Vector3.one);

        StartCoroutine(WaitForPopUpXPBar());

        int collectedStars = PlayerPrefs.GetInt("Stars", 0);

        foreach (var star in stars)
        {
            MakePopUp(star, Vector3.one);
        }
        for (int i = 0; i < collectedStars; i++)
        {
            stars[i].GetChild(0).gameObject.SetActive(true);
        }

        MakePopUp(inGround, Vector3.up);

        MakePopUp(rewards, Vector3.one, 1f);

#if ADS
        noThanksButton.GetComponent<Image>().DOFade(1, 0.4f).SetEase(Ease.Linear).SetDelay(3f).OnStart(() =>
        {
            noThanksButton.gameObject.SetActive(true);
        }); 
#endif

        int rewardsCount = 0;
        float delay = 1.6f;
        MakePopUp(xpReward.transform.parent, Vector3.one, delay);

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

        SoundManager.Instance.Play(SoundTypes.Finish);

#if UNITY_WEBGL || PLATFORM_WEBGL
        CrazyGames.CrazyEvents.Instance.HappyTime();
#endif
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

    private void ShowConfettiParticle()
    {
        Transform confettiParticle = transform.GetLastChild();
        confettiParticle.gameObject.SetActive(true);
        confettiParticle.SetParent(canvas.transform);
        confettiParticle.transform.localScale = Vector3.one;
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

    private IEnumerator WaitForPopUpXPBar()
    {
        yield return new WaitForSeconds(0.4f);
        Transform barparticle = xpBar.GetLastChild();
        barparticle.gameObject.SetActive(true);
        barparticle.parent = canvas.transform;
        barparticle.transform.localScale = Vector3.one;
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

    [System.Serializable]
    public class RewardUnit
    {
        public ResourceTypes type;
        public TextMeshProUGUI amount;
    }
}