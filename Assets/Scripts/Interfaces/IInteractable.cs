using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void Interact(Transform interactor);
    public Vector3 GetPosition();

    public void SetActive(bool state);

    public float GetInteractionTimerMax();

    public bool CanInteractInMotion();
}
