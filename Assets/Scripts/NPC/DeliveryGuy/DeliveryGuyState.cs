using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DeliveryGuyState
{
    public DeliveryGuy controller;

    public DeliveryGuyState(DeliveryGuy controller)
    {
        this.controller = controller;
    }

    public abstract void OnStateStart();
    public abstract void OnStateUpdate();
    public abstract void OnStateEnd();
}

public enum DeliveryGuyStates
{
    Collect,
    GoToMarket,
    Unload, 
    GoToStack,
    Idle
}