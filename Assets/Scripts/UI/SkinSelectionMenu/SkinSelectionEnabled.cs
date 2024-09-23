using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinSelectionEnabled : MonoBehaviour
{
    public static SkinSelectionEnabled Instance { get; private set; }

    [SerializeField] private RectTransform cutoutMask;
    [SerializeField] private Transform skinMenuButton;
    [SerializeField] private Transform tutorial;
    [SerializeField] private UpgradePanel skinMenu;


    private bool AreSkinMenuEnabled
    {
        get { return PlayerPrefs.GetInt("AreSkinsEnabled", 0) == 1; }
        set { PlayerPrefs.SetInt("AreSkinsEnabled", value ? 1 : 0); }
    }

    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        if (SkipIntroPart.Instance.skipIntro)
        {
            EnableSkinMenu();
        }
        else
        {
            if (!AreSkinMenuEnabled)
            {
                skinMenuButton.gameObject.SetActive(false);
                DailyRewardManager.Instance.OnRewardPanelClose += EnableSkinMenu;
            } //just for some time it is not needed 
        }
    }

    public void EnableSkinMenu()
    {
        DailyRewardManager.Instance.OnRewardPanelClose -= EnableSkinMenu;

        skinMenuButton.gameObject.SetActive(true);
        cutoutMask.gameObject.SetActive(true);
        skinMenu.OnOpen += DisableTutorialStuff;

        AreSkinMenuEnabled = true;

        cutoutMask.DOSizeDelta(new Vector2(300, 300), 0.7f).SetEase(Ease.Linear).OnComplete(() =>
        {
            tutorial.gameObject.SetActive(true);
        });
    }

    private void DisableTutorialStuff()
    {
        skinMenu.OnOpen -= DisableTutorialStuff;

        tutorial.gameObject.SetActive(false);
        cutoutMask.gameObject.SetActive(false);
    }
}