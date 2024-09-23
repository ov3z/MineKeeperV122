using UnityEngine;
using DG.Tweening;

public class FruitCollectable : PoolableObject
{
    [SerializeField] ResourceTypes collectableType;

    private Vector3 initialPosition;
    private float maxJumpHeigt = 4.5f;
    private bool canFollowCollector;
    private bool shouldJumpAndReturn = true;
    private Transform collector;
    private Collider interactionTrigger;
    private int idleCycleCount;

    private bool CanItemBePicked
    {
        get
        {
            if(collector.TryGetComponent<ITradeMaker>(out var tradeMaker))
            {
                return tradeMaker.IsThereFreeSpace;
            }
            else
            {
                return false;
            }
        }
    }

    private void OnEnable()
    {
        SetUpPrefab();

        Vector3 dirToCollector = (collector.transform.position - initialPosition).normalized;
        float distanceToCollector = (collector.transform.position - initialPosition).magnitude;

        float randomTurnAngle = Random.Range(0, 360) * Mathf.Deg2Rad;
        Vector3 turnedVector = new Vector3(Mathf.Cos(randomTurnAngle), 0, Mathf.Sin(randomTurnAngle));

        Vector3 onGroundDisplacement = Vector3.up * 0.7f + turnedVector * distanceToCollector / 3;

        if (shouldJumpAndReturn)
        {
            transform.DOJump(initialPosition + onGroundDisplacement, maxJumpHeigt, 1, 0.8f).SetEase(Ease.Linear).OnComplete(() =>
            {
                TryToFollowPlayer();
            });
        }
        else
        {
            transform.DOMove(initialPosition + Vector3.up * 3f, 0.35f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                if (!TryToFollowPlayer())
                {
                    transform.DOMove(initialPosition, 0.35f).SetEase(Ease.InQuad);
                }
            });
        }

        transform.DORotate(transform.localEulerAngles + GetRandomEulerAngles(), 0.8f, RotateMode.FastBeyond360).SetEase(Ease.Linear);
    }

    private void SetUpPrefab()
    {
        interactionTrigger = GetComponent<Collider>();
        interactionTrigger.enabled = false;
        transform.GetChild(0).DOKill();
        transform.DOKill();
        transform.localScale = Vector3.one;

        canFollowCollector = false;
        initialPosition = transform.position + Vector3.up * 0.3f;
    }

    private bool TryToFollowPlayer()
    {
        var result = false;
        if (CanItemBePicked || collectableType == ResourceTypes.Coins)
        {
            canFollowCollector = true;
            result = true;
        }
        else
        {
            interactionTrigger.enabled = true;
            StartIdleAnim();
        }
        return result;
    }

    private void StartIdleAnim()
    {
        idleCycleCount++;
        var targetTransform = transform.GetChild(0);
        targetTransform.DOKill();
        targetTransform.DOMoveY(initialPosition.y + 0.3f, 0.8f).SetEase(Ease.Linear).OnComplete(() =>
        {
            targetTransform.DOMoveY(initialPosition.y - 0.6f, 1.6f).SetEase(Ease.Linear).OnComplete(() =>
            {
                targetTransform.DOMoveY(initialPosition.y + 0.3f, 0.8f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    if (idleCycleCount > 3)
                    {
                        transform.DOScale(0, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            gameObject.SetActive(false);
                        });
                        return;
                    }
                    StartIdleAnim();
                });
            });
        });
    }

    private void Update()
    {
        if (canFollowCollector)
        {
            float distanceModificator = 5f / Vector3.Distance(transform.position, collector.position + Vector3.up);
            transform.position = Vector3.Lerp(transform.position, collector.position + Vector3.up, (7.5f + distanceModificator) * Time.deltaTime);
            if (Vector3.Distance(transform.position, collector.position + Vector3.up) < 0.55f)
            {
                if (collector.TryGetComponent<ICollector>(out var collectorEntity))
                {
                    canFollowCollector = false;
                    if (CanItemBePicked || collectableType == ResourceTypes.Coins)
                    {
                        collectorEntity.OnResourceCollect(collectableType, 1);
                    }
                }
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() && collectableType != ResourceTypes.Coins)
        {
            if (CanItemBePicked)
            {
                canFollowCollector = true;
                interactionTrigger.enabled = false;
            }
        }
    }

    private Vector3 GetRandomEulerAngles()
    {
        float randomAngleX = Random.Range(100, 360);
        float randomAngleY = Random.Range(100, 360);
        float randomAngleZ = Random.Range(100, 360);

        Vector3 randomEulerAngles = new Vector3(randomAngleX, randomAngleY, randomAngleZ);
        return randomEulerAngles;
    }

    public void SetCollector(Transform collector) => this.collector = collector;
    public void SetShouldntJumpAndReturn() => shouldJumpAndReturn = false;
}