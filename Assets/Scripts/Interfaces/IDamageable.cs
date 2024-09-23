using StylizedWater2;
using System;
using UnityEngine;

public interface IDamageable
{
    public event Action<IDamageable> OnDeath;
    public event Action<float> OnHealthChange;
    public event Action<DamageEffect> OnTakeDamage;
 
    public Transform transform { get; }
    public GameObject gameObject { get; }
 
    public void TakeDamage(float damage, DamageEffect damageEffect = DamageEffect.None);
    public float GetHealth();

}

public enum DamageEffect
{
    None,
    Fire
}
