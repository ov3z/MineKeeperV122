using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollector
{
    public void OnResourceCollect(ResourceTypes type, float collectedAmount);
    public void OnResourceCollect(ResourceUnit sender);
    public Transform GetDestinationTransform();
    public float GetGatherPower();
}
