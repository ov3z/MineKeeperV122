using System;
using UnityEngine;

public abstract class PayoutValidation : MonoBehaviour, IPayoutValidation
{
    public event Action OnPayoutValidationComplete;

    public abstract void OnPayout();

    public void FireOnPayoutValidationComplete()
    {
        OnPayoutValidationComplete?.Invoke();
    }
}
