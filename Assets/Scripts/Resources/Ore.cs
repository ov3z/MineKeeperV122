using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ore : MonoBehaviour, ICollectable
{
    public event Action<ICollectable> OnDevastation;

    [SerializeField] Transform orePiecesParent;
    [SerializeField] OreConfig config;
    [SerializeField] Animator animator;
    [SerializeField] Transform gatherPointsParent;

    private const string SHAKE_ANIM_KEY = "Shake";

    private List<Transform> gatherPoints = new();
    private List<Transform> gatherPointsReserve = new();
    private List<Transform> pieces = new();
    private ResourceTypes oreType;
    private new Collider collider;
    private string gatherAnimKey;
    private int collectedPiecesCount;
    private int piecesCountMax;
    private int piecesReserved;
    private float powerForPiece;
    private float piecesHealth;
    private Coroutine collectRoutine;

    private OreVisualUpdater visualUpdater;

    private bool isOreEmpty => collectedPiecesCount == piecesCountMax || piecesReserved == piecesCountMax;
    public string GatherAnimKey => gatherAnimKey;

    private void Awake()
    {
        visualUpdater = new OreVisualUpdater(config.type);
        ResetGatherPointsList();
    }

    private void Start()
    {
        piecesCountMax = config.piecesCountMax;

        foreach (Transform child in orePiecesParent)
            pieces.Add(child);

        SetUpFieldsFromConfig();

        if (GameManager.Instance.IsInCave)
            CaveGameManager.Instance.AddOre(this);

        collider = GetComponent<Collider>();
    }

    private void SetUpFieldsFromConfig()
    {
        oreType = config.type;
        gatherAnimKey = config.GATHER_ANIM_KEY;
        powerForPiece = config.powerForPiece;
        piecesHealth = powerForPiece;
    }

    private void ResetGatherPointsList()
    {
        gatherPoints.Clear();
        foreach (Transform child in gatherPointsParent)
            gatherPoints.Add(child);
        foreach (Transform child in gatherPointsParent)
            gatherPointsReserve.Add(child);
    }

    public bool Collect(ICollector collector)
    {
        bool isCollectionSuccessfull = false;
        if (!isOreEmpty)
        {
            float collectorPower = collector.GetGatherPower();
            int countForCollection = (int)collectorPower / (int)powerForPiece;

            countForCollection = Mathf.Clamp(countForCollection, 0, piecesCountMax - piecesReserved);

            if (countForCollection > 0)
            {
                collectRoutine = StartCoroutine(CollectPiecesRoutine(countForCollection, collector));
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
                    CollectPiece(collector);
                    isCollectionSuccessfull = true;
                }
            }

            animator.SetTrigger(SHAKE_ANIM_KEY);
        }
        return isCollectionSuccessfull;
    }

    IEnumerator CollectPiecesRoutine(int countForCollection, ICollector collector)
    {
        for (int i = 0; i < countForCollection; i++)
        {
            CollectPiece(collector);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void CollectPiece(ICollector collector)
    {
        collector.OnResourceCollect(oreType, 1);

        Transform berryToBeCollected = pieces[Mathf.Clamp(Mathf.CeilToInt(pieces.Count * collectedPiecesCount / piecesCountMax), 0, pieces.Count)];

        collectedPiecesCount++;

        UpdateOreVisual(collector.GetDestinationTransform().position);

        if (isOreEmpty)
        {
            if (devastateRoutine == null)
            {
                StartCoroutine(DevastateRoutine());
                OnDevastation?.Invoke(this);
            }
        }
    }

    private Coroutine devastateRoutine;

    private IEnumerator DevastateRoutine()
    {
        yield return new WaitUntil(() => collectedPiecesCount == piecesCountMax);

        collider.enabled = false;
        PlayerController.Instance.RemoveCollectable(this);

        if (GameManager.Instance.IsInCave)
            CaveGameManager.Instance.RemoveOre(this);

        if (collectRoutine != null)
            StopCoroutine(collectRoutine);

        if (GameManager.Instance.IsInCave)
            CaveGameManager.Instance.RebakeNavmesh();
    }

    private void UpdateOreVisual(Vector3 collectorPosition)
    {
        visualUpdater.UpdateVisauls(ref collectedPiecesCount, ref piecesCountMax, ref pieces, collectorPosition);
    }

    public Vector3 GetPosition() => transform.position;

    public OreConfig GetPlantConfig() => config;

    public ResourceTypes GetResourceType() => config.type;

    public float GetVisibilityAngle() => config.visibilityAngle;

    public bool IsDevastated() => isOreEmpty;

    public string GetGatherAnimKey() => config.GATHER_ANIM_KEY;

    public Transform GetClosestGatherPoint(Transform requestPosition)
    {
        float closestDistance = Mathf.Infinity;
        Transform closestPoint = null;

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

        foreach(var point in gatherPointsReserve)
            gatherPoints.Add(point);
    }
}