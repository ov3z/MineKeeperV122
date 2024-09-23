using System;
using UnityEngine;

public class EnemyAnimationEventHandler : MonoBehaviour
{
    public event Action OnAttackAnimEvent;

    public void Attack() => OnAttackAnimEvent?.Invoke();
}
