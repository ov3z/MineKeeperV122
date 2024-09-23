using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryGuyRent : MonoBehaviour
{
    public static DeliveryGuyRent Instance;

    public Action OnGet;
    public Action OnExpire;

    [SerializeField] private Transform interactionTimerParent;
    [SerializeField] private Image interactionTimerFill;
    [SerializeField] private ParticleSystem rentParticle;
    [SerializeField] private DeliveryGuy deliveryGuy;
    [SerializeField] private Transform shineParticle;
    [SerializeField] private DelivererTimer timer;

    private float interactionTimer;
    private float interactionTimerMax = 2f;

    private Tween interactionReverseTimerTween;
    private Coroutine interactionTimerCoroutine;
    private Action pendingReward, OnFailReward;

    [SerializeField] private Transform interactorVisual;
    private Collider interactionCollider;

    private float offerCooldownTime = 150f;
    private float offerCooldownTimeMax = 150f;

    private string DeliveryRemainingTimeKey => $"DeliveryGuy{(int)deliveryGuy.CollectorType}";
    public float DeliveryRentTime => offerCooldownTime;

    private void Awake()
    {
        Instance = this;

        offerCooldownTime = PlayerPrefs.GetFloat(DeliveryRemainingTimeKey, offerCooldownTimeMax);
    }

    private IEnumerator Start()
    {
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
            if (interactorVisual.gameObject.activeSelf)
            {
                if (!interactionTimerParent.gameObject.activeSelf)
                {
                    interactionTimerParent.gameObject.SetActive(true);
                }
                interactionReverseTimerTween?.Kill();

                interactionTimerCoroutine = StartCoroutine(InteractionTimerRoutine());
            }
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
        deliveryGuy.SwitchState(DeliveryGuyStates.GoToStack);
        StartCoroutine(EnableInteractorWithDelay());
        OnGet?.Invoke();
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
        shineParticle.gameObject.SetActive(state);
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
        rentParticle.Play();

        deliveryGuy.SwitchState(DeliveryGuyStates.Idle);
        deliveryGuy.ResetDeliverer();
        ResetDeliveryGuyTransform();


        OnExpire?.Invoke();
    }

    private void ResetDeliveryGuyTransform()
    {
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            if (interactorVisual.gameObject.activeSelf)
            {
                interactionReverseTimerTween = DOTween.To(() => interactionTimer, x => interactionTimer = x, 0, interactionTimer / (interactionTimerMax * 4)).OnUpdate(() =>
                {
                    interactionTimerFill.fillAmount = interactionTimer / interactionTimerMax;
                });

                if (interactionTimerCoroutine != null)
                    StopCoroutine(interactionTimerCoroutine);
            }
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(DeliveryRemainingTimeKey, offerCooldownTime);
    }
}
