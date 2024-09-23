using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface ICollectable
{
    public event Action<ICollectable> OnDevastation;
    public bool Collect(ICollector collector);
    public Vector3 GetPosition();
    public Transform GetClosestGatherPoint(Transform requestPosition);
    public void ReleaseGatherPoint();
    public ResourceTypes GetResourceType();
    public float GetVisibilityAngle();
    public string GetGatherAnimKey();
    public bool IsDevastated();
    public Transform transform { get; }
    public GameObject gameObject { get; }
}
