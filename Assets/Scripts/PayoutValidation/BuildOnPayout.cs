using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BuildOnPayout : PayoutValidation
{
    public event Action OnPayoutComplete;

    [SerializeField] private List<Collider> triggers;
    [SerializeField] private GuidComponent guidComponent;
    [SerializeField] private float triggerEnableDelay;

    private bool isBuilded = false;
    private string uniqueID;

    private void Awake()
    {
        uniqueID = guidComponent.GetGuid().ToString();
        isBuilded = PlayerPrefs.GetInt($"IsBuilded{uniqueID}", 0) == 0 ? false : true;
    }

    private IEnumerator Start()
    {
        yield return null;
        if (!isBuilded)
        {
            transform.localScale = Vector3.zero;
            foreach (var trigger in triggers)
            {
                trigger.enabled = false;
            }
        }
        else
        {
            OnPayoutComplete?.Invoke();
        }
    }

    public override void OnPayout()
    {
        if (!isBuilded)
        {
            isBuilded = true;
            OnPayoutComplete?.Invoke();

            ParticleSystem unlockParticle = Instantiate(GameManager.Instance.GetUnlockSmoke());
            unlockParticle.transform.position = transform.position;
            unlockParticle.transform.localScale = 4 * Vector3.one;

            transform.DOScale(1.1f, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                transform.DOScale(1f, 0.05f).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    PlayerPrefs.SetInt($"IsBuilded{uniqueID}", 1);
                    StartCoroutine(TriggerEnableRoutine(triggerEnableDelay));
                });
            });

            FireOnPayoutValidationComplete();
        }
    }

    private IEnumerator TriggerEnableRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var trigger in triggers)
        {
            trigger.enabled = true;
        }
    }
}
