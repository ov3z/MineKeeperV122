using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeliveryGuyUnloadState : DeliveryGuyState
{
    private float interactionTimer;
    private Transform characterTransform;
    private int collectedResources;
    private ResourceTypes delivererType;

    private bool IsEmpty => collectedResources == 0;


    public DeliveryGuyUnloadState(DeliveryGuy controller) : base(controller)
    {
        characterTransform = controller.transform;
        delivererType = controller.CollectorType;

    }

    public override void OnStateStart()
    {
        collectedResources = (int)controller.GetResourceBalance(delivererType);
        controller.OnDelivererTriggerStay += OnTriggerStay;
    }

    private void OnTriggerStay(Collider other)
    {
        IInteractable activeInteractable = controller.GetInteractionTarget();

        if (activeInteractable != null && !IsEmpty)
        {
            interactionTimer -= Time.deltaTime;
            if (interactionTimer <= 0)
            {

                interactionTimer = activeInteractable.GetInteractionTimerMax();
                activeInteractable.Interact(characterTransform);
                controller.UpdateVisuals();
                collectedResources = (int)controller.GetResourceBalance(delivererType);

                if (IsEmpty)
                {
                    controller.SwitchState(DeliveryGuyStates.GoToStack);
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
