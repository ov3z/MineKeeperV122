using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    public Action<Transform> OnGet;

    [SerializeField] private List<ResourceTypes> stockContents;
    [SerializeField] private List<SerializablePair<ResourceTypes, Transform>> contentVisuals;
    [SerializeField] private Transform contentUnit;
    [SerializeField] private Transform interactionTimerParent;
    [SerializeField] private Image interactionTimerFill;

    private float interactionTimer;
    private float interactionTimerMax = 2f;

    private int greatestRewardCount = 500;
    private int normalRewardCount = 100;
    private ResourceTypes[] commodities = { ResourceTypes.Blueberry, ResourceTypes.Wood, ResourceTypes.Emerald };
    private Dictionary<ResourceTypes, int> rewards = new();
    private Dictionary<ResourceTypes, Transform> contentVisualsMap = new();

    private Tween interactionReverseTimerTween;
    private Coroutine interactionTimerCoroutine;

    private void Start()
    {
        foreach (var item in contentVisuals)
        {
            item.RegisterOnMap(ref contentVisualsMap);
        }
        InitializeContens();
    }

    private void InitializeContens()
    {
        int rewardIndex = 1;
        foreach (var content in stockContents)
        {
            var reward = content;

            var amount = (rewardIndex == 1) ? greatestRewardCount : normalRewardCount;

            rewards.Add(reward, amount);

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

        GiveReward();
    }

    private void GiveReward()
    {
        foreach (var kvp in rewards)
        {
            OnGet?.Invoke(transform.parent);
            PlayerController.Instance.OnResourceCollect(kvp.Key, kvp.Value);
        }

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
        });

        if (interactionTimerCoroutine != null)
        {
            StopCoroutine(interactionTimerCoroutine);
        }
    }
}
