using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BoarRent : MonoBehaviour
{
    public static BoarRent Instance;

    public Action OnGet;
    public Action OnExpire;

    [SerializeField] private Transform interactionTimerParent;
    [SerializeField] private Image interactionTimerFill;
    [SerializeField] private ParticleSystem rentParticle;
    [SerializeField] private Transform boarVisual;
    [SerializeField] private DelivererTimer timer;

    private float interactionTimer;
    private float interactionTimerMax = 2f;

    private Tween interactionReverseTimerTween;
    private Coroutine interactionTimerCoroutine;
    private Action pendingReward, OnFailReward;

    private Transform interactorVisual;
    private Collider interactionCollider;

    private float offerCooldownTime = 150f;
    private float offerCooldownTimeMax = 150f;

    private string BoarRemainingTimeKey = "BoarRemainingTimeKey";
    public float BoarRentTime => offerCooldownTime;

    private void Awake()
    {
        Instance = this;

        offerCooldownTime = PlayerPrefs.GetFloat(BoarRemainingTimeKey, offerCooldownTimeMax);
    }

    private IEnumerator Start()
    {
        interactorVisual = transform.GetChild(0);
        interactionCollider = GetComponent<Collider>();

        OnFailReward += RemoveRewardOnFail;

        yield return null;

        if (offerCooldownTime > 0 && offerCooldownTime < offerCooldownTimeMax)
        {
            GiveReward();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            if (!interactionTimerParent.gameObject.activeSelf)
            {
                interactionTimerParent.gameObject.SetActive(true);
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

        pendingReward = GiveReward;
        //AdsContainer.Instance.ShowRewarded(pendingReward, OnFailReward);
        AdsContainer.Instance.ShowRewardedYso(pendingReward, OnFailReward);//yso rewarded
    }

    private void GiveReward()
    {
        pendingReward = () => { };
        SetInteractorActive(false);
        StartCoroutine(EnableInteractorWithDelay());
        OnGet?.Invoke();
        rentParticle.Play();
        PlayerController.Instance.SpeedUp();
    }
    public void RemoveRewardOnFail()
    {
        pendingReward -= GiveReward;
        SetInteractorActive(false);
        StartCoroutine(EnableInteractorWithDelay());
    }

    private void SetInteractorActive(bool state)
    {
        interactorVisual.gameObject.SetActive(state);
        boarVisual.gameObject.SetActive(state);
        interactionCollider.enabled = state;
    }

    private IEnumerator EnableInteractorWithDelay()
    {
        timer.Enable(true);

        while (offerCooldownTime > 0)
        {
            offerCooldownTime -= Time.deltaTime;
            timer.UpdateTimer(1 - offerCooldownTime / offerCooldownTimeMax);
            yield return null;
        }

        timer.Enable(false);

        offerCooldownTime = offerCooldownTimeMax;
        interactionTimer = 0;
        interactionTimerFill.fillAmount = 0;
        SetInteractorActive(true);

        PlayerController.Instance.SlowDown();
        OnExpire?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            interactionReverseTimerTween = DOTween.To(() => interactionTimer, x => interactionTimer = x, 0, interactionTimer / (interactionTimerMax * 4)).OnUpdate(() =>
            {
                interactionTimerFill.fillAmount = interactionTimer / interactionTimerMax;
            });

            if (interactionTimerCoroutine != null)
                StopCoroutine(interactionTimerCoroutine);
        }

    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(BoarRemainingTimeKey, offerCooldownTime);
        PlayerController.Instance.SlowDown();
    }
}
