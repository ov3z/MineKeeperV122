using System.Collections.Generic;
using UnityEngine;

public class TraderInteractor : MonoBehaviour, IInteractable
{
    [SerializeField] List<ResourceTypes> resourcesForSale;
    [SerializeField] private bool canInteractInMotion = false;
    [SerializeField] private Transform trader;
    [SerializeField] private Transform wagon;

    private float interactionTimerMax = 0.1f;
    private Transform intercatorCopy;


    public bool CanInteractInMotion() => canInteractInMotion;

    public float GetInteractionTimerMax() => interactionTimerMax;

    public Vector3 GetPosition() => wagon.position;

    public void Interact(Transform interactor)
    {
        intercatorCopy = interactor;

        foreach (var availableResource in resourcesForSale)
        {
            if (ResourceStorage.Instance.GetResourceBalance(availableResource) > 0)
            {
                ResourceStorage.Instance.ChangeResourceAmount(availableResource, -1);

                PoolableObject resourceInstance = PoolingSystem.Instance.GetResourcePool(availableResource).GetObject();
                resourceInstance.transform.position = PlayerController.Instance.transform.position;
                (resourceInstance as ResourceUnit).SetDestination(PlayerController.Instance.GetInteractionTarget().GetPosition());
                (resourceInstance as ResourceUnit).SetJumpDuration(0.7f);
                (resourceInstance as ResourceUnit).OnMotionEnd += OnResourceUnitMotionEnd;
                resourceInstance.gameObject.SetActive(true);
                break;
            }
        }
    }

    private void OnResourceUnitMotionEnd(ResourceUnit sender)
    {
        PoolableObject resource = PoolingSystem.Instance.GetCollectiblePool(ResourceTypes.Coins).GetObject();

        (resource as FruitCollectable).SetCollector(intercatorCopy);

        (resource as FruitCollectable).SetShouldntJumpAndReturn();

        resource.transform.position = trader.position;
        resource.gameObject.SetActive(true);

        sender.OnMotionEnd -= OnResourceUnitMotionEnd;
    }

    public void SetActive(bool state) => gameObject.SetActive(state);
}
