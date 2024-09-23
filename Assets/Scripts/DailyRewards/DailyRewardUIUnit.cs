using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardUIUnit : MonoBehaviour
{
    [SerializeField] private GuidComponent guid;
    [SerializeField] private int dayToUnlock;
    [SerializeField] private DailyRewardUnit rewardUnit;

    [SerializeField] private TextMeshProUGUI[] rewardAmount;
    [SerializeField] private Image[] rewardImage;

    [SerializeField] private Transform availablePanel;
    [SerializeField] private Transform recievedPanel;

    [SerializeField] private TextMeshProUGUI rewardDay;
    [SerializeField] private TextMeshProUGUI rewardDayCompleted;

    private Button getRewardButton;

    private string uniqueID => guid.GetGuid().ToString();

    private void Start()
    {
        for (int i = 0; i < rewardUnit.rewards.Count; i++)
        {
            rewardAmount[i].text = $"{rewardUnit.rewards[i].item2}";
            rewardImage[i].sprite = DailyRewardManager.Instance.GetRewardSprite(rewardUnit.rewards[i].item1);
        }

        rewardDay.text = $"DAY {dayToUnlock + 1}";
        rewardDayCompleted.text = $"DAY {dayToUnlock + 1}";

        DailyRewardManager.Instance.OnNeedUpdateDailyRewards += UpdateRewardStates;

        getRewardButton = availablePanel.GetComponent<Button>();
        getRewardButton.onClick.RemoveAllListeners();
        getRewardButton.onClick.AddListener(GetRewards);

        CheckAndEnableExcMark();
    }

    private void UpdateRewardStates()
    {
        bool didGetrewards = PlayerPrefs.GetInt($"DidGet{uniqueID}", 0) == 1;

        var daysLoggedIn = DailyRewardManager.Instance.daysLoggedIn;

        if (dayToUnlock <= daysLoggedIn)
        {
            SetAvailable();
        }
        if (didGetrewards)
        {
            SetRewardsRecieved();
        }
    }

    private void GetRewards()
    {
        getRewardButton.onClick.RemoveAllListeners();
        PlayerPrefs.SetInt($"DidGet{uniqueID}", 1);
        SetRewardsRecieved();

        DailyRewardManager.Instance.AskToGetReward(rewardUnit);
        DailyRewardManager.Instance.DisableExclamationMark();
#if UNITY_WEBGL || PLATFORM_WEBGL
        CrazyGames.CrazyEvents.Instance.HappyTime();
#endif
    }

    private void SetAvailable()
    {
        availablePanel.gameObject.SetActive(true);

        CheckAndEnableExcMark();
    }

    private void CheckAndEnableExcMark()
    {
        bool didGetrewards = PlayerPrefs.GetInt($"DidGet{uniqueID}", 0) == 1;

        var daysLoggedIn = DailyRewardManager.Instance.daysLoggedIn;
        if (dayToUnlock <= daysLoggedIn && !didGetrewards)
        {
            DailyRewardManager.Instance.EnableExclamationMark();
        }
    }

    private void SetRewardsRecieved()
    {
        availablePanel.gameObject.SetActive(false);
        recievedPanel.gameObject.SetActive(true);
        rewardDay.gameObject.SetActive(false);
        rewardDayCompleted.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        DailyRewardManager.Instance.OnNeedUpdateDailyRewards -= UpdateRewardStates;
    }
}
