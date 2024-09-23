using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStack : MonoBehaviour, IInteractable, IStackHolder, IUpgradable
{
    [SerializeField] private ResourceTypes stackType;
    [SerializeField] private List<Transform> stackVisualElements;
    [SerializeField] private bool isVisualNormalized = false;
    [SerializeField] private GuidComponent guidComponent;
    [SerializeField] private Transform storageFullUI;
    [SerializeField] private int capacityUpgradeIncrement;
    [SerializeField] private float maxCapacity = 24;

    private float interactionTimerMax = 0.05f;
    private float resourceAmountInside;
    private float reservedAmountInside;
    private bool canInteractInMotion = true;
    private string uniqueID;
    private Collider interactionTrigger;

    private void Awake()
    {
        uniqueID = guidComponent.GetGuid().ToString();

        int capacity = PlayerPrefs.GetInt($"Capacity{uniqueID}");

        if (capacity != 0)
        {
            maxCapacity = capacity;
        }

        LoadResourceAmountInside();
        UpdateVisuals();

        interactionTrigger = GetComponent<Collider>();
    }

    public void AddResources(float amount)
    {
        if (amount <= maxCapacity - resourceAmountInside)
        {
            resourceAmountInside += amount;
            SaveResourceAmountInside();

            if (resourceAmountInside == maxCapacity)
                storageFullUI.gameObject.SetActive(true);
        }
    }

    public void ReservePlace(float amount)
    {
        if (amount <= maxCapacity - reservedAmountInside)
        {
            reservedAmountInside += amount;
        }
    }

    public void Interact(Transform interactor)
    {
        if (interactor.TryGetComponent<ITradeMaker>(out var tradeMaker))
        {
            bool isInteractionAllowed = stackType == ResourceTypes.Coins || tradeMaker.IsThereFreeSpace;
            if (resourceAmountInside > 0 && isInteractionAllowed)
            {
                PoolableObject resource = PoolingSystem.Instance.GetCollectiblePool(stackType).GetObject();

                (resource as FruitCollectable).SetCollector(interactor);

                (resource as FruitCollectable).SetShouldntJumpAndReturn();

                int normalizedItemIndex = Mathf.Clamp(Mathf.CeilToInt((stackVisualElements.Count - 1) * resourceAmountInside / maxCapacity), 0, (stackVisualElements.Count - 1));
                resource.transform.position = (isVisualNormalized) ? stackVisualElements[normalizedItemIndex].position : stackVisualElements[Mathf.Clamp((int)resourceAmountInside, 0, (stackVisualElements.Count - 1))].position;
                resource.gameObject.SetActive(true);

                resourceAmountInside--;
                reservedAmountInside--;

                if (resourceAmountInside < maxCapacity)
                    storageFullUI.gameObject.SetActive(false);

                UpdateVisuals();
            }
        }


    }

    private void SaveResourceAmountInside()
    {
        PlayerPrefs.SetFloat($"ResourceAmount{uniqueID}", resourceAmountInside);
    }

    private void LoadResourceAmountInside()
    {
        resourceAmountInside = PlayerPrefs.GetFloat($"ResourceAmount{uniqueID}", 0);
        reservedAmountInside = resourceAmountInside;

        if (resourceAmountInside == maxCapacity)
            storageFullUI.gameObject.SetActive(true);
    }

    public void UpdateVisuals()
    {
        int normalizedItemIndex = Mathf.Clamp(Mathf.CeilToInt((stackVisualElements.Count - 1) * resourceAmountInside / maxCapacity), 0, (stackVisualElements.Count - 1));
        int resourcesToBeShown = (isVisualNormalized) ? normalizedItemIndex : (int)resourceAmountInside;

        resourcesToBeShown = Mathf.Clamp(resourcesToBeShown, 0, (stackVisualElements.Count - 1));

        for (int i = 0; i < resourcesToBeShown; i++)
        {
            if (!stackVisualElements[i].gameObject.activeSelf)
            {
                stackVisualElements[i].gameObject.SetActive(true);
            }
        }

        if (resourcesToBeShown < stackVisualElements.Count)
        {
            for (int i = resourcesToBeShown; i < stackVisualElements.Count; i++)
            {
                if (stackVisualElements[i].gameObject.activeSelf)
                {
                    stackVisualElements[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void Upgrade()
    {
        maxCapacity += capacityUpgradeIncrement;
        PlayerPrefs.SetInt($"Capacity{uniqueID}", (int)maxCapacity);
    }

    public void SetActive(bool state) => gameObject.SetActive(state);
    public Vector3 GetPosition() => transform.position;

    public float RemainingSpaceInTheStack() => maxCapacity - reservedAmountInside;

    public float GetInteractionTimerMax() => interactionTimerMax;

    public bool CanInteractInMotion() => canInteractInMotion;

    public Vector3 GetNextStackElementPosition()
    {
        int normalizedItemIndex = Mathf.CeilToInt((stackVisualElements.Count - 1) * (reservedAmountInside) / maxCapacity);
        int resourcesToBeShown = (isVisualNormalized) ? normalizedItemIndex : (int)reservedAmountInside - 1;
        return stackVisualElements[Mathf.Clamp(resourcesToBeShown, 0, (stackVisualElements.Count - 1))].position;
    }

    public float GetCurrentStat()
    {
        return maxCapacity;
    }

    public float GetUpgradeIncrement()
    {
        return capacityUpgradeIncrement;
    }

    public string GetID()
    {
        return uniqueID;
    }

    public bool IsUnlocked()
    {
        return interactionTrigger.enabled;
    }
}