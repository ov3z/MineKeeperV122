#if UNITY_WEBGL || PLATFORM_WEBGL
using CrazyGames;
#endif
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CaveTraveller : MonoBehaviour
{
    public static CaveTraveller Instance { get; private set; }

    private event Action OnFadeFromBlackComplete;

    [SerializeField] private Transform fadeToBlack;
    [SerializeField] private CanvasGroup fadeToBlackGroup;
    [SerializeField] private bool isInCave;
    private bool isLoadingInProcess;


    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        bool isRightAfterIntro = PlayerPrefs.GetInt("IsFirstLoad", 0) == 1 ? true : false;
        bool isFromInitialLoadScene = PlayerPrefs.GetInt("IsFromInitialLoadScene", 1) == 0 ? false : true;

        if (isInCave)
        {
            FadeFromBlack();
        }
        else if (!isFromInitialLoadScene && !isRightAfterIntro)
        {
            FadeFromBlack();
            StartCoroutine(OpenMapCoroutine());
        }
    }

    private IEnumerator OpenMapCoroutine()
    {
        yield return null;
        OpenMapMenu();
    }

    private void OpenMapMenu()
    {
        if (MapWindow.Instance)
        {
            MapWindow.Instance.Open();
            OnFadeFromBlackComplete += UpdateMap;
        }
    }

    private void UpdateMap()
    {
        Debug.Log("map update traveller");
        OnFadeFromBlackComplete -= UpdateMap;
        MapWindow.Instance.RegisterExplorationResult();
        MapWindow.Instance.OnClose += OnMapWindowClose;
    }

    private void FadeFromBlack()
    {
        fadeToBlack.gameObject.SetActive(true);

        float alpha = 1;
        fadeToBlackGroup.alpha = alpha;

        DOTween.To(() => alpha, x => alpha = x, 0, 1f).SetDelay(0.4f).SetEase(Ease.Linear).OnUpdate(() =>
        {
            fadeToBlackGroup.alpha = alpha;
        }).OnComplete(() =>
        {
            fadeToBlack.gameObject.SetActive(false);
            OnFadeFromBlackComplete?.Invoke();
        });
    }

    private void OnMapWindowClose()
    {
        MapWindow.Instance.OnClose -= OnMapWindowClose;

        if (PlayerPrefs.GetInt("Stars", 0) > 0)
            StartCoroutine(FireOnBattleWinCoroutine());
    }

    private IEnumerator FireOnBattleWinCoroutine()
    {
        yield return new WaitForSeconds(0.25f);
        QuestEvents.FireOnWinBattle(1);
    }

    public void LoadCaveLoadingScene()
    {
        StartCoroutine(WaitForRewardAndLoad());
    }

    private IEnumerator WaitForRewardAndLoad()
    {
        yield return null;

        if (isLoadingInProcess) yield break;

        isLoadingInProcess = true;

        fadeToBlack.gameObject.SetActive(true);

        float alpha = 0;

        DOTween.To(() => alpha, x => alpha = x, 1, 1f).SetEase(Ease.Linear).OnUpdate(() =>
        {
            fadeToBlackGroup.alpha = alpha;
        }).OnComplete(() =>
        {
            DOTween.KillAll();
            LoadScene_2();
        });
    }

    public void LoadCloudScene()
    {
        SceneManager.LoadScene(7);
    }

    private void LoadScene_2()
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        CrazySDK.Instance.GameplayStop();
#endif
        SceneManager.LoadScene(2);
    }
}