using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using YsoCorp.GameUtils;

public class AsyncSceneLoader : MonoBehaviour
{
    public static AsyncSceneLoader Instance;


    [SerializeField] private RectTransform cutoutMask;


    [SerializeField] private float showTime = 0.5f;
    [SerializeField] private int sceneToLoad;
    [SerializeField] private bool isThisTravelLoadingScene;

    private AsyncOperation opeartion;
    private float progress;
    private float time;
    public bool withIntro = false;
    public bool result;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
#if ENABLE_AB_TEST
        GetScene();
        Debug.Log($"with or without intro {result}");
#endif
        bool isIntroduction = PlayerPrefs.GetInt("IntroductionPart", 1) == 0 ? false : true;
        bool hasChoosenGender = PlayerPrefs.GetInt("ChoseGender", 0) == 1 ? true : false;

        if (isIntroduction)
        {
            #region ABTESTINGREGION

#if ENABLE_AB_TEST
                         if (!result)
                                    {
                                        if (hasChoosenGender)
                                        {
                                            PlayerPrefs.SetInt("IsFirstLoad", 1);
                                            SceneManager.LoadScene(1);
                                        }
                                        else
                                            sceneToLoad = 7;
                                    }
                                    else
                                    {
                                        if (hasChoosenGender)
                                            sceneToLoad = PlayerPrefs.GetInt("IntroductionPart", 1) + 3;
                                        else
                                            sceneToLoad = 7;
                                    }
#endif

            #endregion

            #region IntroSkipRegion

            if (SkipIntroPart.Instance.skipIntro)
            {
                if (hasChoosenGender)
                {
                    PlayerPrefs.SetInt("IsFirstLoad", 1);
                    SceneManager.LoadScene(1);
                }
                else
                    sceneToLoad = 7;
            }
            else
            {
                if (hasChoosenGender)
                    sceneToLoad = PlayerPrefs.GetInt("IntroductionPart", 1) + 3;
                else
                    sceneToLoad = 7;
            }

            #endregion
            if (hasChoosenGender)
                sceneToLoad = PlayerPrefs.GetInt("IntroductionPart", 1) + 3;
            else
                sceneToLoad = 7;
        }
        else if (isThisTravelLoadingScene)
        {
            bool isFromCave = PlayerPrefs.GetInt("IsFromCave", 0) == 0 ? false : true;

            if (isFromCave)
            {
                sceneToLoad = 1;
            }
            else
                sceneToLoad = 3;
        }
        else
        {
            PlayerPrefs.SetInt("IsFromInitialLoadScene", 1);
        }

        StartCoroutine(WaitcertainTime());
    }

    private IEnumerator WaitcertainTime()
    {
        yield return null;

        opeartion = SceneManager.LoadSceneAsync(sceneToLoad);
        opeartion.allowSceneActivation = false;
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        progress = 0;

        while (progress < 89)
        {
            progress = (int)(opeartion.progress * 100);
            yield return null;
        }

        opeartion.allowSceneActivation = true;
    }

#if ENABLE_AB_TEST
    public void GetScene()
    {
        result = !YCManager.instance.abTestingManager.IsPlayerSample("withoutIntro");
        Debug.Log($"with intro {result}");
    }
#endif
}