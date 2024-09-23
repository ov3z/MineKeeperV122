using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

public class CaveMapUnit : MonoBehaviour
{
    [SerializeField] private GuidComponent guid;
    [SerializeField] private Image image;
    [SerializeField] private Transform starsParent;
    [SerializeField] private Transform attackTarget;
    [SerializeField] private ParticleSystem unlockParticle;

    [SerializeField] private Sprite tickSprite;
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private Sprite blankSprite;

    [SerializeField] private Sprite rewindButton;
    [SerializeField] private Sprite adButton;
    [SerializeField] private bool isRewindFree;
    [SerializeField] private Button replayButton;
    [SerializeField] private Image repayIcon;

    private List<Transform> stars = new();
    private int activeStars;

    private string uniqueID => guid.GetGuid().ToString();

    private void Awake()
    {
#if !ADS
        isRewindFree = true;
        Debug.Log(isRewindFree);
#endif
    }

    private void Start()
    {
        foreach (Transform child in starsParent)
        {
            stars.Add(child);
        }

        var wasCaveCleared = PlayerPrefs.GetInt($"wasCompleted{uniqueID}", 0) == 1 ? true : false;
        var hasBeenUnlocked = PlayerPrefs.GetInt($"hasBeenUnlocked{uniqueID}", 0) == 1 ? true : false;

        if (wasCaveCleared)
        {
            unlockParticle.gameObject.SetActive(false);
            EnablePanelTickPanel();
        }
        else
        {
            int currentCaveLevel = PlayerPrefs.GetInt("LevelCave", 0);
            var canUnlock = hasBeenUnlocked || transform.GetSiblingIndex() == 0;

            if (transform.GetSiblingIndex() == currentCaveLevel && canUnlock)
            {
                SetAsCurrentTarget();
            }
        }

        activeStars = PlayerPrefs.GetInt($"activeStars{uniqueID}", 0);
        if (activeStars > 0)
        {
            ShowStars(activeStars, false);
        }
    }

    public void EnablePanelTickPanel()
    {
        image.enabled = true;
        image.sprite = tickSprite;
        PlayerPrefs.SetInt($"wasCompleted{uniqueID}", 1);
        unlockParticle.Play();
    }

    public void SetAsCurrentTarget()
    {
        image.enabled = false;
        attackTarget.gameObject.SetActive(true);
        PlayerPrefs.SetInt($"hasBeenUnlocked{uniqueID}", 1);
    }

    public void ShowStars(int starCount, bool hasdelay, bool needSound = false)
    {
        starsParent.gameObject.SetActive(true);
        for (int i = 0; i < starCount; i++)
        {
            ShowStar(i, hasdelay, needSound);
        }

        TryActivateReplay(starCount);
    }

    private void TryActivateReplay(int recievedStars)
    {
        if (recievedStars < 3)
        {
            replayButton.gameObject.SetActive(true);
            repayIcon.sprite = (isRewindFree) ? rewindButton : adButton;
            replayButton.onClick.AddListener(ReplayLevel);
            image.sprite = blankSprite;
        }
    }

    private void ReplayLevel()
    {
        var levelIndex = transform.GetSiblingIndex();
        MapWindow.Instance.ReplayLevel(levelIndex, isRewindFree);
    }

    private void ShowStar(int index, bool hasDelay, bool needSound)
    {
        int delayKoef = hasDelay ? 1 : 0;
        stars[index].localScale = Vector3.zero;
        stars[index].DOScale(1.15f, 0.3f).SetEase(Ease.Linear).SetDelay(delayKoef * index * 0.3f).OnStart(() =>
        {
            if (needSound)
            {
                SoundManager.Instance.Play(SoundTypes.PopUp);
            }
        }).OnComplete(() =>
        {
            stars[index].DOScale(1f, 0.1f).SetEase(Ease.Linear);
        });

        PlayerPrefs.SetInt($"activeStars{uniqueID}", index + 1);
    }
}
