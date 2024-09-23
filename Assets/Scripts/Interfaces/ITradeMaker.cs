using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITradeMaker
{
    public float GetResourceBalance(ResourceTypes resourceType);
    public void ChangeResourceAmount(ResourceTypes resourceTypes, int amount);
    public IInteractable InteractionTarget => null;
    public bool IsThereFreeSpace => false;
    public Transform transform { get; }
}
