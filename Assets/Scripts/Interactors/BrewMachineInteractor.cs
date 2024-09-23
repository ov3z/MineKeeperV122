using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrewMachineInteractor : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform objectTransform;
    [SerializeField] private BrewingMachine brewingMachine;


    private float interactionTimerMax = 0.2f;
    private bool canInteractInMotion = false;

    public bool CanInteractInMotion() => canInteractInMotion;

    public float GetInteractionTimerMax() => interactionTimerMax;

    public Vector3 GetPosition() => objectTransform.position;

    public void Interact(Transform interactor)
    {
        if (!brewingMachine.IsBrewingTankFull())
        {
            ResourceTypes resourceTypeForBrew = brewingMachine.GetResourceTypeForBrewing();
            float resourceAmountForBrew = brewingMachine.GetResourceAmountForBrewing();

            if (ResourceStorage.Instance.GetResourceBalance(resourceTypeForBrew) > 0)
            {
                ResourceStorage.Instance.ChangeResourceAmount(resourceTypeForBrew, -1);
                brewingMachine.ReservePlaceForProduct(resourceTypeForBrew, 1);

                PoolableObject resourceInstance = PoolingSystem.Instance.GetResourcePool(resourceTypeForBrew).GetObject();
                resourceInstance.transform.position = PlayerController.Instance.transform.position;
                (resourceInstance as ResourceUnit).SetDestination(PlayerController.Instance.GetInteractionTarget().GetPosition());
                (resourceInstance as ResourceUnit).SetJumpDuration(0.7f);
                (resourceInstance as ResourceUnit).OnMotionEnd += OnResourceUnitMotionEnd;
                resourceInstance.gameObject.SetActive(true);
            }
        }
    }

    private void OnResourceUnitMotionEnd(ResourceUnit sender)
    {
        brewingMachine.AddProducts(sender.GetResourceType(), 1);
        sender.OnMotionEnd -= OnResourceUnitMotionEnd;
    }

    public void SetActive(bool state)
    {
        gameObject.SetActive(state);
    }
}
