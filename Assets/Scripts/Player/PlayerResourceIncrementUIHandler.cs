using UnityEngine;

public class PlayerResourceIncrementUIHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform playerResourceIncrementCanvas;

    private bool hasCollectedResource;

    private ResourceTypes collectedType;
    private float collectedAmount;
    private float resourceCollectionIntervalTimer;
    private float resourceCollectionIntervalTimerMax = 0.15f;


    private void Start()
    {
        playerController.NotifyOnResourceCollect += RegisterResourceCollection;
    }

    private void Update()
    {
        if (hasCollectedResource)
        {
            resourceCollectionIntervalTimer += Time.deltaTime;
            if (resourceCollectionIntervalTimer >= resourceCollectionIntervalTimerMax)
            {
                ShowPickUI(collectedType, collectedAmount);
                hasCollectedResource = false;
                resourceCollectionIntervalTimer = 0;
            }
        }
    }

    private void RegisterResourceCollection(ResourceTypes type, float amount)
    {
        if (!hasCollectedResource)
        {
            collectedType = type;
            hasCollectedResource = true;
        }
        else
        {
            if (collectedType != type)
            {
                ShowPickUI(collectedType, collectedAmount);
            }
            collectedType = type;
        }
        collectedAmount += amount;
        resourceCollectionIntervalTimer = 0;
    }

    private void ShowPickUI(ResourceTypes type, float amountToShow)
    {
        PickUnitUI pickUnitUI = PoolingSystem.Instance.GetPickUIUnit() as PickUnitUI;
        pickUnitUI.SetUnit(amountToShow, type);
        pickUnitUI.transform.SetParent(playerResourceIncrementCanvas.transform);
        pickUnitUI.gameObject.SetActive(true);
        collectedAmount = 0;
    }
}
