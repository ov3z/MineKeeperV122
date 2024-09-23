using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceForAd : MonoBehaviour
{
    [SerializeField] private ResourceTypes rewardType = ResourceTypes.Coins;
    [SerializeField] private int initialRewardAmount = 150;
    [SerializeField] private int rewardIncreasePeriod = 5;
    [SerializeField] private int rewardIncrement = 50;

    [SerializeField] private TextMeshProUGUI rewardtext_1;
    [SerializeField] private TextMeshProUGUI rewardtext_2;

    private int aquireCount;
    private Action OnRewarardedSucceed;
    private Action OnRewarardedFailed;
    private Button adButton;

    private int RewardAmount => initialRewardAmount + aquireCount / rewardIncreasePeriod * rewardIncrement;

    private void Awake()
    {
        aquireCount = PlayerPrefs.GetInt("AquireCount", 0);

        OnRewarardedSucceed = GiveReward;
        OnRewarardedFailed = () => { };

        adButton = GetComponent<Button>();
        adButton.onClick.AddListener(ShowAd);

        UpdateRewardText();
    }

    private void Start()
    {
#if !ADS
        gameObject.SetActive(false);
#endif
    }

    private void UpdateRewardText()
    {
        rewardtext_1.text = $"+{RewardAmount}";
        rewardtext_2.text = $"+{RewardAmount}";
    }

    private void ShowAd()
    {
        //AdsContainer.Instance.ShowRewarded(OnRewarardedSucceed, OnRewarardedFailed);
        AdsContainer.Instance.ShowRewardedYso(OnRewarardedSucceed, OnRewarardedFailed);//yso rewarded
    }

    private void GiveReward()
    {
        PlayerController.Instance.OnResourceCollect(rewardType, RewardAmount);

        PlayerPrefs.SetInt("AquireCount", ++aquireCount);

        UpdateRewardText();
    }

    private void OnDestroy()
    {
        if (adButton)
        {
            adButton.onClick.RemoveAllListeners();
        }
    }
}
