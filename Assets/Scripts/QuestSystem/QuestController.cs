using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

#if USING_GA
using GameAnalyticsSDK;
#endif

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }

    public event Action OnQuestCompletion;
    public event Action OnNextQuestLoad;

    public event Action OnCurrentQuestPanelOpen;
    public event Action OnCurrentQuestPanelClose;

    [SerializeField] private List<Quest> quests = new List<Quest>();
    [SerializeField] private List<Quest> reusableQuestsquests = new List<Quest>();
    [SerializeField] private QuestUIController uiController;

    [SerializeField] private Sprite tickImage;
    [SerializeField] private bool isThereAutoComplete;

    private int currentQuestIndex;
    private Quest currentQuest;
    private Coroutine loadNextQuestCoroutine;
    private Reward reward;

    public bool IsThereAutoComplete => isThereAutoComplete;

    public int CurrentQuestIndex => currentQuestIndex;


    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return null;
        LoadCurrentQuest();

        reward = currentQuest.Reward;
    }

    private void LoadCurrentQuest()
    {
        currentQuestIndex = PlayerPrefs.GetInt("CurrentQuest", 0);
#if ENABLE_AB_TEST
         if (!AsyncSceneLoader.Instance.result)
                {
                    int startIndex = 0; 
                    int count = 5;     
                    quests.RemoveRange(startIndex, count);
                    SkipIntroQuests();
                }
#endif
        if (SkipIntroPart.Instance.skipIntro)
        {
            int startIndex = 0; 
            int count = 5;     
            quests.RemoveRange(startIndex, count);
            SkipIntroQuests();
        }
        
        if (currentQuestIndex == 2 || currentQuestIndex == 3 ||
            currentQuestIndex == 4) // this line is temporary, i will make better solution to fix this issue
            currentQuestIndex = 1;

        InitializeQuest();
    }

    private void InitializeQuest()
    {
        if (currentQuestIndex >= quests.Count)
        {
            int newIndex = Random.Range(0, reusableQuestsquests.Count);
            currentQuest = reusableQuestsquests[newIndex];
        }
        else
        {
            currentQuest = quests[currentQuestIndex];
        }


        currentQuest.OnProgressUpdate += OnProgressUpdate;
        currentQuest.OnQuestComplete += FinalizeQuest;
        currentQuest.OnQuestPanelOpen += FireCurrentQuestPanelOpen;
        currentQuest.OnQuestPanelClose += FireCurrentQuestPanelClose;

        var needDeafaultVisual = currentQuest.Initialize();

        SetUpQuestUI(needDeafaultVisual);
    }

    private void SetUpQuestUI(bool needDefaultVisuals)
    {
        uiController.SetDescriptionText(currentQuest.Description);
        uiController.SetProgressFill(currentQuest.ProgressNormalized);
        uiController.SetProgressText(currentQuest.ProgressText);

        if (needDefaultVisuals)
        {
            uiController.SetQuestIcon(currentQuest.Icon);
            uiController.SwitcgBGToUncompletedState();
        }
    }

    private void OnProgressUpdate(float progress)
    {
        uiController.SetProgressFill(progress);
        uiController.SetProgressText(currentQuest.ProgressText);
    }

    private void FinalizeQuest(Reward reward)
    {
        currentQuest.OnQuestComplete -= FinalizeQuest;
        currentQuest.OnProgressUpdate -= OnProgressUpdate;
        currentQuest.OnQuestPanelOpen -= FireCurrentQuestPanelOpen;
        currentQuest.OnQuestPanelClose -= FireCurrentQuestPanelClose;

        if (!GameManager.Instance.IsInCave)
        {
            uiController.SetQuestIcon(tickImage);
            uiController.SwitchBgToCompletedState();
        }

        this.reward = reward;

        if (loadNextQuestCoroutine != null)
        {
            CoroutineRunner.Instance.StopCoroutineLocal(loadNextQuestCoroutine);
        }

        if (isThereAutoComplete)
        {
            StartNextQuestLoadingCoroutine();
        }

        OnQuestCompletion?.Invoke();
    }

    private void TryShowInterstitial()
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        var player = PlayerController.Instance;
        if (player.CanShowIntersatital)
            AdsContainer.Instance.ShowInsterstitial(player.ResetIntersatials);
#endif
    }

    private void FireCurrentQuestPanelOpen() => OnCurrentQuestPanelOpen?.Invoke();

    private void FireCurrentQuestPanelClose() => OnCurrentQuestPanelClose?.Invoke();

    public void StartNextQuestLoadingCoroutine()
    {
        TryShowInterstitial();
        uiController.DisableQuestCompletionButton();
        loadNextQuestCoroutine = CoroutineRunner.Instance.RunCoroutineLocal(LoadNextQuestAsync(reward));
    }

    private IEnumerator LoadNextQuestAsync(Reward reward)
    {
        FireCompleteCurretQuestGA();

        SoundManager.Instance.Play(SoundTypes.QuestComplete);
        yield return new WaitForSeconds(0.2f);
        uiController.ShowQuestCompletionEffect();
        uiController.DisableHandTutorial();
        yield return new WaitForSeconds(0.3f);
        uiController.HideQuestUI();

        float delayForNextQuest = 0.4f;

        if (reward.rewards.Count > 0)
        {
            delayForNextQuest = 0.2f;
            yield return new WaitForSeconds(0.2f);
            uiController.SetRewardText((int)reward.rewards[0].Amount);
            uiController.ShowRewardUI(reward.rewards[0].Type);
            uiController.ShowRewardEffect();
        }

        yield return new WaitForSeconds(delayForNextQuest);
        LoadNextQuest();
        SaveCurrentQuest();

        uiController.ShowQuestUI();
        SoundManager.Instance.Play(SoundTypes.PopUp);

        OnNextQuestLoad?.Invoke();

        if (!isThereAutoComplete)
        {
            //CameraFocusManager.Instance.FocusCamOnQuestTarget();
        }
    }

    private void FireStartCurrentQuestGA()
    {
        YsoCorp.GameUtils.YCManager.instance.OnGameStarted(currentQuestIndex); // for yso sdk*/
        Debug.Log(
            $"Quest have started: {currentQuestIndex}"); // intro scene degishli bolmaz yaly condition gerek entak

#if USING_GA
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, $"Quest {currentQuestIndex}");
#endif
    }

    private void FireCompleteCurretQuestGA()
    {
        YsoCorp.GameUtils.YCManager.instance.OnGameFinished(true); // yso corp finish level
        Debug.Log($"Quest have finished {currentQuestIndex}");
#if USING_GA
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, $"Quest {currentQuestIndex}");
#endif
    }

    private void LoadNextQuest()
    {
        currentQuestIndex++;
        currentQuest.WipeData();
        InitializeQuest();
        FireStartCurrentQuestGA();
    }


    private void SaveCurrentQuest()
    {
        PlayerPrefs.SetInt("CurrentQuest", currentQuestIndex);
    }

    public Transform GetCurrentQuestTarget()
    {
        return currentQuest.GetQuestTarget();
    }

    public Transform GetCurrentQuestCanvasTarget()
    {
        return currentQuest.GetQuestCanvasTarget();
    }

    public void SkipCurrentQuest()
    {
        FireCompleteCurretQuestGA();

        currentQuestIndex++;
        SaveCurrentQuest();

        FireStartCurrentQuestGA();
    }

    private void SkipIntroQuests()
    {
        currentQuestIndex++;
        SaveCurrentQuest();

        FireStartCurrentQuestGA();
    }

    private void OnDestroy()
    {
        currentQuest.OnProgressUpdate -= OnProgressUpdate;
        currentQuest.OnQuestComplete -= FinalizeQuest;
        currentQuest.Dispose();
    }
}