using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryGuyCollectState : DeliveryGuyState
{
    private float interactionTimer;
    private Transform characterTransform;
    private int collectedResources;
    private int maxCapacity;
    private ResourceTypes delivererType;

    private bool IsThereSpace => collectedResources < maxCapacity;

    public DeliveryGuyCollectState(DeliveryGuy controller) : base(controller)
    {
        characterTransform = controller.transform;
        maxCapacity = controller.MaxCapacity;
        delivererType = controller.CollectorType;
    }

    public override void OnStateStart()
    {
        controller.OnDelivererTriggerStay += OnTriggerStay;
        collectedResources = (int)controller.GetResourceBalance(delivererType);
    }

    private void OnTriggerStay(Collider other)
    {
        IInteractable activeInteractable = controller.GetInteractionTarget();
        if (activeInteractable != null)
        {
            interactionTimer -= Time.deltaTime;
            if (interactionTimer <= 0)
            {
                collectedResources = (int)controller.GetResourceBalance(delivererType);
                if (IsThereSpace)
                {
                    interactionTimer = activeInteractable.GetInteractionTimerMax();
                    activeInteractable.Interact(characterTransform);
                }
            }
        }
    }

    public override void OnStateUpdate()
    {

    }

    public override void OnStateEnd()
    {
        controller.OnDelivererTriggerStay -= OnTriggerStay;
    }
}
