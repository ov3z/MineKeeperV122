using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MapWindow : MonoBehaviour
{
    public static MapWindow Instance { get; private set; }

    public event Action OnClose;

    [SerializeField] private Transform context;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private List<CaveMapUnit> caveMapUnits;

    [SerializeField] private Transform exploreButton;
    [SerializeField] private Transform returnButton;

    [SerializeField] private TextMeshProUGUI levelText_1;
    [SerializeField] private TextMeshProUGUI levelText_2;

    [SerializeField] private Button escButton;

    private Action OnInterstitialComplete;

    private bool IsLanscape => Screen.width > Screen.height;

    public void Awake()
    {
        Instance = this;

        bool isFromCave = PlayerPrefs.GetInt("IsFromCave", 0) == 0 ? false : true;
        bool isFromInitialLoadScene = PlayerPrefs.GetInt("IsFromInitialLoadScene", 1) == 1;

        if (isFromCave && !isFromInitialLoadScene)
        {
            exploreButton.gameObject.SetActive(false);
            returnButton.gameObject.SetActive(true);

            escButton.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        var vertPos = (IsLanscape)
            ? PlayerPrefs.GetFloat("VertPosLandscape", 1f)
            : PlayerPrefs.GetFloat("VertPosPortrait", 1);
        scrollRect.verticalNormalizedPosition = vertPos;
    }

    public void Open()
    {
        if (PlayerController.Instance.inAfterCaveAB)
        {
            AdsContainer.Instance.ShowInsterstitialYso(LoadGoToMineMap); //yso interstitial
            Debug.Log("opens map menu of cave ");
        }
        else
        {
            LoadGoToMineMap();
        }
    }

    private void LoadGoToMineMap()
    {
        if (PlayerPrefs.GetInt("LevelCave", 0) < 15)
        {
            var vertPos = (IsLanscape)
                ? PlayerPrefs.GetFloat("VertPosLandscape", 1f)
                : PlayerPrefs.GetFloat("VertPosPortrait", 1);
            scrollRect.verticalNormalizedPosition = vertPos;

            context.gameObject.SetActive(true);
            UpdateCaveLevel();
        }
        else
        {
            ShowInterstitialAndLoadScene();
        }
    }

    public void ShowInterstitialAndLoadScene()
    {
#if ADS
        OnInterstitialComplete += () => { LoadCave(); };
        //AdsContainer.Instance.ShowInsterstitial(OnInterstitialComplete);
        AdsContainer.Instance.ShowInsterstitialYso(OnInterstitialComplete); //yso interstitial
#else
        LoadCave();
#endif
    }

    public void LoadCave()
    {
        StartCoroutine(LoadCaveLoaderScene());
    }

    private IEnumerator LoadCaveLoaderScene()
    {
        yield return new WaitForSeconds(0.1f);
        OnInterstitialComplete = () => { };
        CaveTraveller.Instance.LoadCaveLoadingScene();
    }

    public void ShowIntersatitalAndClose()
    {
#if ADS
        OnInterstitialComplete += () =>
        {
            Close();
            escButton.gameObject.SetActive(true);
        };
        //AdsContainer.Instance.ShowInsterstitial(OnInterstitialComplete);
        AdsContainer.Instance.ShowInsterstitialYso(OnInterstitialComplete); //yso interstitial
#else
        Close();
        escButton.gameObject.SetActive(true);
#endif
    }

    public void Close()
    {
        escButton.gameObject.SetActive(true);
        context.gameObject.SetActive(false);
        OnClose?.Invoke();

        if (returnButton.gameObject.activeSelf)
        {
            returnButton.gameObject.SetActive(false);
            exploreButton.gameObject.SetActive(true);
        }
    }

    public void RegisterExplorationResult()
    {
        StartCoroutine(MapUpdateRoutine());
    }

    private IEnumerator MapUpdateRoutine()
    {
        int currentCave = PlayerPrefs.GetInt("LevelCave", 0);
        int recievedStars = PlayerPrefs.GetInt("Stars", 0);

        var replayLevel = PlayerPrefs.GetInt("ReplayedLevel", -1);
        if (replayLevel >= 0)
        {
            PlayerPrefs.SetInt("ReplayedLevel", -1);
        }

        if (currentCave > 0)
        {
            var previousCaveIcon = (replayLevel >= 0) ? caveMapUnits[replayLevel] : caveMapUnits[currentCave - 1];
            previousCaveIcon.EnablePanelTickPanel();
            previousCaveIcon.ShowStars(recievedStars, true, true);

            SoundManager.Instance.Play(SoundTypes.QuestComplete);
        }

        yield return new WaitForSeconds((recievedStars + 1) * 0.3f + 0.3f);

        float maxVertPosNorm;
        float scrollStep;
        int cavesOnTheScreen;

        if (IsLanscape)
        {
            maxVertPosNorm = 0.9f;
            cavesOnTheScreen = 1;
            scrollStep = 0.05f;
        }
        else
        {
            maxVertPosNorm = 0.8f;
            cavesOnTheScreen = 4;
            scrollStep = 0.1f;
        }

        if (currentCave == cavesOnTheScreen)
        {
            float vertPosNorm = scrollRect.verticalNormalizedPosition;
            DOTween.To(() => vertPosNorm, x => vertPosNorm = x, maxVertPosNorm, 1).SetEase(Ease.Linear).OnUpdate(() =>
            {
                scrollRect.verticalNormalizedPosition = vertPosNorm;
            }).OnComplete(() =>
            {
                var key = (IsLanscape) ? "VertPosLandscape" : "VertPosPortrait";
                PlayerPrefs.SetFloat(key, vertPosNorm);
            });
        }
        else if (currentCave > cavesOnTheScreen)
        {
            var index = currentCave - cavesOnTheScreen;
            float vertPosNorm = scrollRect.verticalNormalizedPosition;
            DOTween.To(() => vertPosNorm, x => vertPosNorm = x, maxVertPosNorm - scrollStep * index, 0.8f)
                .SetEase(Ease.Linear).OnUpdate(() => { scrollRect.verticalNormalizedPosition = vertPosNorm; })
                .OnComplete(() =>
                {
                    var key = (IsLanscape) ? "VertPosLandscape" : "VertPosPortrait";
                    PlayerPrefs.SetFloat(key, vertPosNorm);
                });
            yield return new WaitForSeconds(0.8f);
        }

        yield return new WaitForSeconds(0.2f);

        var currentCaveIcon = caveMapUnits[currentCave];
        currentCaveIcon.SetAsCurrentTarget();
    }

    private void UpdateCaveLevel()
    {
        var currentLevel = PlayerPrefs.GetInt("LevelCave", 0) + 1;

        levelText_1.text = $"Cave {currentLevel}";
        levelText_2.text = $"Cave {currentLevel}";
    }

    public void ReplayLevel(int levelForReplay, bool isFree)
    {
        PlayerPrefs.SetInt("ReplayedLevel", levelForReplay);
        if (isFree)
        {
            ShowInterstitialAndLoadScene();
        }
        else
        {
            void OnRewardedComplete()
            {
                StartCoroutine(LoadCaveLoaderScene());
            }

            // AdsContainer.Instance.ShowRewarded(OnRewardedComplete);
            AdsContainer.Instance.ShowRewardedYso(OnRewardedComplete); //yso rewarded
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReplayLevel(0, true);
        }
    }
}