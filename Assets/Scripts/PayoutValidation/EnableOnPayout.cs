using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnableOnPayout : PayoutValidation
{
    [SerializeField] private GuidComponent guidComponent;
    [SerializeField] private bool isUsedForChildren = false;
    [SerializeField] private bool hasToScale = true;
    [SerializeField] private bool isScaleUpVertical = true;
    [SerializeField] private bool shouldWaitForCam = true;
    [SerializeField] private List<Collider> triggers;
    [SerializeField] private float extraDelay;

    private bool isPayedUp = false;
    private string uniqueID;

    private void Awake()
    {
        uniqueID = guidComponent.GetGuid().ToString();
        isPayedUp = PlayerPrefs.GetInt($"IsBuilded{uniqueID}", 0) == 0 ? false : true;
    }

    private void Start()
    {
        if (!isPayedUp)
        {
            if (isUsedForChildren)
            {
                foreach (Transform child in transform)
                {
                    if (hasToScale)
                        child.localScale = Vector3.zero;
                    else
                        child.gameObject.SetActive(false);
                }
            }
            else
            {
                if (hasToScale)
                    transform.localScale = Vector3.zero;
                else
                    gameObject.SetActive(false);
            }

            foreach (var collider in triggers)
            {
                collider.enabled = false;
            }
        }
    }

    public override void OnPayout()
    {
        if (!isPayedUp)
        {
            isPayedUp = true;
            float delay = CameraFocusManager.Instance.AddFocusTarget(transform) + extraDelay;
            PlayerPrefs.SetInt($"IsBuilded{uniqueID}", 1);

            CoroutineRunner.Instance.StartCoroutine(ShowUnlockedStateRoutine(delay));
        }
    }

    private IEnumerator ShowUnlockedStateRoutine(float delay)
    {
        //float delay = numberInQueue * 2f;

        if (shouldWaitForCam)
            yield return new WaitForSeconds(delay);

        ParticleSystem unlockParticle = Instantiate(GameManager.Instance.GetUnlockSmoke());
        unlockParticle.transform.position = transform.position;
        unlockParticle.transform.localScale = 3 * Vector3.one;

        if (isUsedForChildren)
        {
            if (hasToScale)
            {
                if (isScaleUpVertical)
                {
                    foreach (Transform child in transform)
                    {
                        ScaleZ(child);
                    }
                }
                else
                {
                    foreach (Transform child in transform)
                    {
                        Scale(child);
                    }
                }
            }
            else
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (hasToScale)
            {
                if (isScaleUpVertical)
                {
                    ScaleZ(transform);
                }
                else
                {
                    Scale(transform);

                }
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        FireOnPayoutValidationComplete();

        yield return new WaitForSeconds(CameraFocusManager.Instance.GetQueueLenght() * 2 + 1f);

        foreach (var collider in triggers)
        {
            collider.enabled = true;
        }

        var interactor = transform.GetComponentInChildren<PayingInteractor>();
        if (interactor != null && !interactor.IsUnlocked)
        {
            interactor.DisableTrigger();
        }
    }

    private void ScaleZ(Transform subject)
    {
        subject.localScale = new Vector3(1, 1, 0);
        subject.DOScaleZ(1.1f, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            subject.DOScaleZ(1f, 0.05f).SetEase(Ease.OutCubic);
        });
    }

    private void Scale(Transform subject)
    {
        subject.localScale = new Vector3(0, 0, 0);
        subject.DOScale(1.1f, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            subject.DOScale(1f, 0.05f).SetEase(Ease.OutCubic);
        });
    }
}
