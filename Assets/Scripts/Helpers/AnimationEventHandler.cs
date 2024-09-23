using System;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public event Action OnGatherAnimationEvent;
    public event Action OnFightAnimationEvent;

    public event Action OnTurnOnAnimationEvent;
    public event Action OnTurnOffAnimationEvent;
    
    public void FireGatherAnimationEvent() => OnGatherAnimationEvent?.Invoke();
    public void FireFightAnimationEvent() => OnFightAnimationEvent?.Invoke();

    public void TurnOnAnimationEvent() => OnTurnOnAnimationEvent?.Invoke();
    public void TurnOffAnimationEvent() => OnTurnOffAnimationEvent?.Invoke();
}
