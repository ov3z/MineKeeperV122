using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ResourceStock : MonoBehaviour
{
    public Action<Transform> OnGet;

    [SerializeField] private List<ResourceTypes> stockContents;
    [SerializeField] private List<SerializablePair<ResourceTypes, Transform>> contentVisuals;
    [SerializeField] private Transform contentUnit;
    [SerializeField] private Transform interactionTimerParent;
    [SerializeField] private Image interactionTimerFill;
    [SerializeField] private Transform videoIcon;


    private float interactionTimer;
    private float interactionTimerMax = 2f;

    private int greatestRewardCountMinimal = 50;
    private int greatestRewardCount = 50;
    private ResourceTypes[] commodities = { ResourceTypes.Blueberry, ResourceTypes.Wood, ResourceTypes.Emerald };
    private Dictionary<ResourceTypes, int> rewards = new();
    private Dictionary<ResourceTypes, Transform> contentVisualsMap = new();

    private Tween interactionReverseTimerTween;
    private Coroutine interactionTimerCoroutine;
    private Action pendingReward, OnFailReward;

    private void Start()
    {
#if !ADS
        gameObject.SetActive(false);
        return;
#endif

        foreach (var item in contentVisuals)
        {
            item.RegisterOnMap(ref contentVisualsMap);
        }
        OnFailReward += RemoveRewardOnFail;
        InitializeContens();
    }

    private void InitializeContens()
    {
        int rewardIndex = 1;

        var playerLevel = PlayerLevelManager.Instance.Level;
        greatestRewardCount = greatestRewardCountMinimal + (greatestRewardCountMinimal / 2) * (playerLevel - 1);

        var hasDiscoveredOres = PlayerPrefs.GetInt("LevelCave", 0) >= 1;

        foreach (var content in stockContents)
        {
            var reward = content;
            if (content != ResourceTypes.Coins)
            {
                var characterLevel = PlayerLevelManager.Instance.Level;

                if (characterLevel < 4)
                {
                    var rarity = (Random.Range(0, 5));
                    if (rarity == 0)
                    {
                        reward = commodities[0];
                    }
                    else if (rarity == 1 && hasDiscoveredOres)
                    {
                        reward = commodities[2];
                    }
                    else
                    {
                        reward = commodities[1];
                    }
                }
                else
                {
                    reward = commodities[Random.Range(0, commodities.Length)];
                }
            }

            rewards.Add(reward, greatestRewardCount / rewardIndex);

            if (rewardIndex == 1)
            {
                contentVisualsMap[reward].gameObject.SetActive(true);
            }

            var newContentUnit = Instantiate(contentUnit, contentUnit.parent);

            var contextText = newContentUnit.GetChild(0).GetComponent<TextMeshProUGUI>();
            var contextIcon = newContentUnit.GetChild(0).GetChild(0).GetComponent<Image>();

            contextText.text = $"{rewards[reward]}";
            contextIcon.sprite = ResourceSpriteStorage.Instance.GetIcon(reward);

            rewardIndex++;
        }

        contentUnit.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!interactionTimerParent.gameObject.activeSelf)
        {
            interactionTimerParent.gameObject.SetActive(true);
            videoIcon.gameObject.SetActive(false);
        }
        interactionReverseTimerTween?.Kill();

        interactionTimerCoroutine = StartCoroutine(InteractionTimerRoutine());
    }

    private IEnumerator InteractionTimerRoutine()
    {
        while (interactionTimer < interactionTimerMax)
        {
            interactionTimer += Time.deltaTime;
            interactionTimerFill.fillAmount = interactionTimer / interactionTimerMax;
            yield return null;
        }

        pendingReward += GiveReward;
        //AdsContainer.Instance.ShowRewarded(pendingReward, OnFailReward);
        AdsContainer.Instance.ShowRewardedYso(pendingReward, OnFailReward);//yso rewarded
    }

    private void GiveReward()
    {
        foreach (var kvp in rewards)
        {
            OnGet?.Invoke(transform.parent);
            PlayerController.Instance.OnResourceCollect(kvp.Key, kvp.Value);
        }

        pendingReward -= GiveReward;
        transform.gameObject.SetActive(false);
    }
    public void RemoveRewardOnFail()
    {
        pendingReward -= GiveReward;
        transform.gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        interactionReverseTimerTween = DOTween.To(() => interactionTimer, x => interactionTimer = x, 0, interactionTimer / (interactionTimerMax * 4)).OnUpdate(() =>
        {
            interactionTimerFill.fillAmount = interactionTimer / interactionTimerMax;
        }).OnComplete(() =>
        {
            interactionTimerParent.gameObject.SetActive(false);
            videoIcon.gameObject.SetActive(true);
        });

        StopCoroutine(interactionTimerCoroutine);
    }
}
