using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour, ICollectable
{
    public event Action<ICollectable> OnDevastation;

    [SerializeField] Transform fruitsParent;
    [SerializeField] PlantConfig config;
    [SerializeField] Animator animator;
    [SerializeField] Transform gatherPointsParent;

    private const string CHOP_ANIM_KEY = "Chop";
    private const string BLOOM_ANIM_KEY = "Bloom";

    private List<Transform> gatherPoints = new();
    private List<Transform> gatherPointsReserve = new();
    private List<Transform> fruits = new();
    private ResourceTypes plantType;
    private new Collider collider;
    private string gatherAnimKey;
    private int collectedPiecesCount;
    private int piecesCountMax;
    private int piecesReserved;
    private float timeForResetting;
    private float timeToReset;
    private float powerForPiece;
    private float piecesHealth;
    private Coroutine collectRoutine;

    private PlantVisualUpdater visualUpdater;


    private bool isBushEmpty => collectedPiecesCount == piecesCountMax || piecesReserved == piecesCountMax;
    public string GatherAnimKey => gatherAnimKey;

    private void Awake()
    {
        visualUpdater = new PlantVisualUpdater(config.type);
        ResetGatherPointsList();
    }

    private void Start()
    {
        piecesCountMax = config.piecesCountMax;

        foreach (Transform child in fruitsParent)
            fruits.Add(child);

        SetUpFieldsFromConfig();

        GameManager.Instance.AddPlant(plantType, this);
        QuestTargetSystem.Instance.AddResource(plantType, transform);

        collider = GetComponent<Collider>();
    }

    private void SetUpFieldsFromConfig()
    {
        plantType = config.type;
        timeForResetting = config.timeForResetting;
        gatherAnimKey = config.GATHER_ANIM_KEY;
        powerForPiece = config.powerForPiece;
        piecesHealth = powerForPiece;
    }

    private void ResetGatherPointsList()
    {
        gatherPoints.Clear();
        gatherPointsReserve.Clear();

        foreach (Transform child in gatherPointsParent)
            gatherPoints.Add(child);
        foreach (Transform child in gatherPointsParent)
            gatherPointsReserve.Add(child);
    }

    private void Update()
    {
        if (isBushEmpty)
        {
            timeToReset += Time.deltaTime;
            if (timeToReset >= timeForResetting)
            {
                timeToReset = 0;
                collectedPiecesCount = 0;
                piecesReserved = 0;
                collider.enabled = true;
                GameManager.Instance.AddPlant(plantType, this);
                QuestTargetSystem.Instance.AddResource(plantType, transform);
                animator.SetTrigger(BLOOM_ANIM_KEY);

                ResetGatherPointsList();
                ResetBushVisual();
            }
        }
    }

    private void ResetBushVisual()
    {
        visualUpdater.ResetVisuals(ref fruits);
    }

    public bool Collect(ICollector collector)
    {
        bool isCollectionSuccessfull = false;
        if (!isBushEmpty)
        {
            float collectorPower = collector.GetGatherPower();
            int remainingBerries = (piecesCountMax - piecesReserved);
            int countForCollection = Mathf.Clamp((int)collectorPower / (int)powerForPiece, 0, remainingBerries);

            if (countForCollection > 0)
            {
                collectRoutine = StartCoroutine(CollectFruitsRoutine(countForCollection, collector));
                isCollectionSuccessfull = true;
                piecesReserved += countForCollection;
            }
            else
            {
                piecesHealth -= collectorPower;
                if (piecesHealth <= 0)
                {
                    piecesHealth = powerForPiece;
                    piecesReserved++;
                    CollectFruit(collector);
                    isCollectionSuccessfull = true;
                }
            }

            animator.SetTrigger(CHOP_ANIM_KEY);
        }
        return isCollectionSuccessfull;
    }

    IEnumerator CollectFruitsRoutine(int countForCollection, ICollector collector)
    {
        for (int i = 0; i < countForCollection; i++)
        {
            CollectFruit(collector);
            yield return new WaitForSeconds(0.025f);
        }
    }

    private void CollectFruit(ICollector collector)
    {
        PoolableObject collectable;
        if (collector is PlayerController)
        {
            PrepareCollectableForPlayer(out collectable, collector);
        }
        else
        {
            PrepareCollectableForNPC(out collectable, collector);
        }

        Transform berryToBeCollected = fruits[Mathf.Clamp(Mathf.CeilToInt(fruits.Count * collectedPiecesCount / piecesCountMax), 0, fruits.Count - 1)];
        collectable.transform.SetPositionAndRotation(transform.position, berryToBeCollected.rotation);
        collectable.gameObject.SetActive(true);

        collectedPiecesCount++;

        UpdatePlantVisual();

        if (isBushEmpty)
        {
            collider.enabled = false;
            PlayerController.Instance.RemoveCollectable(this);
            GameManager.Instance.RemovePlant(plantType, this);
            QuestTargetSystem.Instance.DiscardResource(plantType, transform);

            if (devastateRoutine == null)
            {
                devastateRoutine = StartCoroutine(DevastationRoutine());
                OnDevastation?.Invoke(this);
            }
        }
    }

    private Coroutine devastateRoutine;

    private IEnumerator DevastationRoutine()
    {
        yield return new WaitUntil(() => collectedPiecesCount == piecesCountMax);

        if (collectRoutine != null)
            StopCoroutine(collectRoutine);
    }

    private void PrepareCollectableForPlayer(out PoolableObject collectable, ICollector collector)
    {
        collectable = PoolingSystem.Instance.GetCollectiblePool(plantType).GetObject();
        (collectable as FruitCollectable).SetCollector(collector.GetDestinationTransform());
    }

    private void PrepareCollectableForNPC(out PoolableObject collectable, ICollector collector)
    {
        collectable = PoolingSystem.Instance.GetResourcePool(plantType).GetObject();
        (collectable as ResourceUnit).SetDestination(collector.GetDestinationTransform().position);
        (collectable as ResourceUnit).SetJumpDuration(0.3f);
        (collectable as ResourceUnit).SetJumpPower(0f);
        (collectable as ResourceUnit).OnMotionEnd += collector.OnResourceCollect;
    }

    private void OnDisable()
    {
        GameManager.Instance.RemovePlant(plantType, this);
    }

    private void UpdatePlantVisual()
    {
        visualUpdater.UpdateVisauls(ref collectedPiecesCount, ref piecesCountMax, ref fruits);
    }

    public Vector3 GetPosition() => transform.position;

    public PlantConfig GetPlantConfig() => config;

    public ResourceTypes GetResourceType() => config.type;

    public float GetVisibilityAngle() => config.visibilityAngle;

    public bool IsDevastated() => isBushEmpty;

    public string GetGatherAnimKey() => config.GATHER_ANIM_KEY;

    public Transform GetClosestGatherPoint(Transform requestPosition)
    {
        float closestDistance = Mathf.Infinity;
        Transform closestPoint = null;

        if (gatherPoints.Count == 0)
        {
            ResetGatherPointsList();
        }

        foreach (Transform point in gatherPoints)
        {
            float distance = Vector3.Distance(requestPosition.position, point.position);
            if (distance < closestDistance)
            {
                closestPoint = point;
                closestDistance = distance;
            }
        }

        gatherPoints.Remove(closestPoint);
        return closestPoint;
    }

    public void ReleaseGatherPoint()
    {
        gatherPoints.Clear();

        foreach (var point in gatherPointsReserve)
        {
            gatherPoints.Add(point);
        }
    }
}