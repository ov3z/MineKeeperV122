using System;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BonusForAd : MonoBehaviour
{
    [SerializeField] private Transform interactionTimerParent;
    [SerializeField] private Image interactionTimerFill;
    [SerializeField] private Transform videoIcon;

    private float interactionTimer;
    private float interactionTimerMax = 2f;

    private Tween interactionReverseTimerTween;
    private Coroutine interactionTimerCoroutine;
    private Action pendingReward, OnFailReward;

    private void Start()
    {
#if !ADS
        gameObject.SetActive(false);
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            if (!interactionTimerParent.gameObject.activeSelf)
            {
                interactionTimerParent.gameObject.SetActive(true);
                videoIcon.gameObject.SetActive(false);
            }
            interactionReverseTimerTween?.Kill();

            interactionTimerCoroutine = StartCoroutine(InteractionTimerRoutine());
        }
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
        GiveRewardResult();

        pendingReward -= GiveReward;
        transform.gameObject.SetActive(false);
    }

    protected virtual void GiveRewardResult()
    {

    }

    public void RemoveRewardOnFail()
    {
        pendingReward -= GiveReward;
        transform.gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
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
}
