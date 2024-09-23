using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeStationInteractor : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform objectTransform;
    [SerializeField] private TradeStation tradeStation;


    private float interactionTimerMax = 0.2f;
    private bool canInteractInMotion = false;

    public bool CanInteractInMotion() => canInteractInMotion;

    public float GetInteractionTimerMax() => interactionTimerMax;

    public Vector3 GetPosition() => objectTransform.position;

    public void Interact(Transform interactor)
    {
        if(interactor.TryGetComponent<ITradeMaker>(out var tradeMaker))
        {
            foreach (var availableResource in tradeStation.GetResourceTypesForSale(false))
            {
                if (!tradeStation.IsStationFullOf(availableResource))
                {
                    if (tradeMaker.GetResourceBalance(availableResource) > 0)
                    {
                        tradeMaker.ChangeResourceAmount(availableResource, -1);
                        tradeStation.ReservePlaceForProduct(availableResource, 1);

                        PoolableObject resourceInstance = PoolingSystem.Instance.GetResourcePool(availableResource).GetObject();
                        resourceInstance.transform.position = tradeMaker.transform.position;
                        (resourceInstance as ResourceUnit).SetDestination(tradeMaker.InteractionTarget.GetPosition());
                        (resourceInstance as ResourceUnit).SetJumpDuration(0.7f);
                        (resourceInstance as ResourceUnit).OnMotionEnd += OnResourceUnitMotionEnd;
                        resourceInstance.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    private void OnResourceUnitMotionEnd(ResourceUnit sender)
    {
        tradeStation.AddProducts(sender.GetResourceType(), 1);
        sender.OnMotionEnd -= OnResourceUnitMotionEnd;
    }

    public void SetActive(bool state)
    {
        gameObject.SetActive(state);
    }
}
