using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance { get; private set; }

    public Action OnNeedUpdateDailyRewards;
    public Action OnRewardPanelClose;

    [SerializeField] private Button getRewardButton;
    [SerializeField] private UpgradePanel panelUX;
    [SerializeField] private RewardPopUp rewardPopUp;
    [SerializeField] private UpgradePanel getRewardPanel;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Transform exclamtionMark;
    [SerializeField] private EnableDailyRewardsButton openDailyRewardButton;

    [SerializeField] private List<SerializablePair<DailyRewardTypes, Sprite>> rewardSprites;
    [SerializeField] private List<SerializablePair<DailyRewardTypes, Sprite>> rewardPopUpSprites;

    [SerializeField] private GuidComponent dragonGuid;

    private Dictionary<DailyRewardTypes, Sprite> rewardSpritesMap = new();
    private Dictionary<DailyRewardTypes, Sprite> rewardPopUpSpritesMap = new();

    private DateTime previousExitTime;
    private int secondInAnHour = 3600;
    private int secondsToRecieveReward;

    private float timerUpdateTimer;
    private DailyRewardUnit currentReward;
    private List<SerializablePair<DailyRewardTypes, int>> currentRewardList = new();

    public int daysLoggedIn { get; private set; }
    public Button GetRewardButton => getRewardButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (var item in rewardSprites)
            item.RegisterOnMap(ref rewardSpritesMap);

        foreach (var item in rewardPopUpSprites)
            item.RegisterOnMap(ref rewardPopUpSpritesMap);

        panelUX.OnOpen += FireOnPanelOpen;
        panelUX.OnClose += FireOnPanelClose;
    }

    private void FireOnPanelOpen()
    {
        OnNeedUpdateDailyRewards?.Invoke();
    }

    private void FireOnPanelClose()
    {
        OnRewardPanelClose?.Invoke();
    }

    private void Start()
    {
        var areMissionsComplete = PlayerPrefs.GetInt("DidCompleteDailyMissions", 0) == 1;
        if (areMissionsComplete)
            return;

        var firstLoginDate = PlayerPrefs.GetString("FirstLoginDate", "");
        if (firstLoginDate == "")
        {
            PlayerPrefs.SetString("FirstLoginDate", DateTime.Now.ToString());
        }
        else
        {
            var secsAfterFirstLogin = (DateTime.Now - DateTime.Parse(firstLoginDate)).TotalSeconds;
            daysLoggedIn = (int)secsAfterFirstLogin / (secondInAnHour * 24);
        }

        secondsToRecieveReward = PlayerPrefs.GetInt("scondsToRecieveReward", 24 * secondInAnHour);
        var lastEnterStringTime = PlayerPrefs.GetString("PreviousEnterTime", "");

        if (lastEnterStringTime != "")
        {
            var lastEnterTime = DateTime.Parse(lastEnterStringTime);
            var intervalBetweenSessions = Mathf.RoundToInt((float)(DateTime.Now - lastEnterTime).TotalSeconds);
            secondsToRecieveReward -= intervalBetweenSessions;
        }

        CheckRewardAvailability();
        UpdateRemainingTime();

        if (PlayerPrefs.GetInt("HasUnlockedDailyQuests", 0) == 1)
        {
            openDailyRewardButton.EnabledailyRewardButton();
        }
        else
        {
            TradeStation.Instance.OnBuild += OpenDailyRewardPanel;
        }
    }

    private void CheckRewardAvailability()
    {
        if (secondsToRecieveReward <= 0)
        {
            secondsToRecieveReward += 24 * secondInAnHour;
            UnlockNextReward();
            CheckRewardAvailability();

            EnableExclamationMark();
        }
    }

    public void EnableExclamationMark()
    {
        if (!exclamtionMark.gameObject.activeSelf)
            exclamtionMark.gameObject.SetActive(true);
    }

    public void DisableExclamationMark()
    {
        if (exclamtionMark.gameObject.activeSelf)
            exclamtionMark.gameObject.SetActive(false);
    }

    private void OpenDailyRewardPanel()
    {
        if (PlayerPrefs.GetInt("HasUnlockedDailyQuests", 0) == 0)
        {
            PlayerPrefs.SetInt("HasUnlockedDailyQuests", 1);
            StartCoroutine(OpenDailyRewardPanelCoroutine());
        }
    }

    private IEnumerator OpenDailyRewardPanelCoroutine()
    {
        yield return new WaitForSeconds(8.6f);

        panelUX.OpenSettingsPanel();
        openDailyRewardButton.EnabledailyRewardButton();
    }

    private void Update()
    {
        timerUpdateTimer += Time.deltaTime;
        if (timerUpdateTimer >= 1)
        {
            timerUpdateTimer = 0;

            secondsToRecieveReward--;

            CheckRewardAvailability();
            UpdateRemainingTime();
        }
    }

    private void UnlockNextReward()
    {

    }

    private void UpdateRemainingTime()
    {
        var remainingHours = secondsToRecieveReward / secondInAnHour;
        var remainingMinutes = (secondsToRecieveReward % secondInAnHour) / 60;
        var remainingSeconds = (secondsToRecieveReward % secondInAnHour) % 60;

        var hourPrefix = (remainingHours < 10) ? "0" : "";
        var minutePrefix = (remainingMinutes < 10) ? "0" : "";
        var secondPrefix = (remainingSeconds < 10) ? "0" : "";

        var remainingTimeString = $"{hourPrefix}{remainingHours} : {minutePrefix}{remainingMinutes} : {secondPrefix}{remainingSeconds}";
        timerText.text = $"Next reward in {remainingTimeString}";
    }

    public void UnlockDragon()
    {
        PlayerPrefs.SetInt($"IsUnlocked{dragonGuid.GetGuid().ToString()}", 1);
    }

    private void DisabledailyRewards()
    {
        openDailyRewardButton.DisableDailyRewards();
        PlayerPrefs.SetInt("DidCompleteDailyMissions", 1);
    }

    public void AskToGetReward(DailyRewardUnit reward)
    {
        currentReward = reward;
        currentRewardList = currentReward.rewards;

        if (currentRewardList.Count > 0)
        {
            OpenRewardsPopUpPanel();
            PopUpReward();
        }
    }

    private void OpenRewardsPopUpPanel()
    {
        getRewardPanel.OpenSettingsPanel();
    }

    private void PopUpReward()
    {
        PlayRewardPopUpAnimation();
        GetReward();
        if (currentRewardList.Count > 0)
        {
            getRewardButton.onClick.RemoveAllListeners();
            getRewardButton.onClick.AddListener(PopUpReward);
        }
        else
        {
            getRewardButton.onClick.RemoveAllListeners();
            getRewardButton.onClick.AddListener(CloseGetrewardPopUP);
            DisableExclamationMark();
        }
    }

    private void GetReward()
    {
        if (currentRewardList[0].item1 != DailyRewardTypes.Dragon)
        {
            var rewardtype = DailyRewardToResourceConverter.Convert(currentRewardList[0].item1);
            ResourceStorage.Instance.ChangeResourceAmount(rewardtype, currentRewardList[0].item2);
        }
        else
        {
            Instance.UnlockDragon();
            Instance.DisabledailyRewards();
        }
        currentRewardList.RemoveAt(0);
    }

    private void CloseGetrewardPopUP()
    {
        getRewardPanel.CloseUpgradePanel();
        rewardPopUp.CloseDailyrewardPopUp();
    }

    private void PlayRewardPopUpAnimation()
    {
        rewardPopUp.MakePrewardPopUp(currentRewardList);
    }

    public Sprite GetRewardSprite(DailyRewardTypes type)
    {
        return rewardSpritesMap[type];
    }

    public Sprite GetRewardSpriteForPopUp(DailyRewardTypes type)
    {
        return rewardPopUpSpritesMap[type];
    }

    private void OnDestroy()
    {
        previousExitTime = DateTime.Now;
        PlayerPrefs.SetString("PreviousEnterTime", previousExitTime.ToString());
        PlayerPrefs.SetInt("scondsToRecieveReward", secondsToRecieveReward);
        TradeStation.Instance.OnBuild -= OpenDailyRewardPanel;
        panelUX.OnOpen -= FireOnPanelOpen;
    }
}