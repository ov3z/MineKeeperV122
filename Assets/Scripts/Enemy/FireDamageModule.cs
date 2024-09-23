using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDamageModule : MonoBehaviour
{
    [SerializeField] private Transform fireParticle;
    [SerializeField] private float dps = 1;
    [SerializeField] private float effectDuration = 5;

    private IDamageable damageable;
    private Coroutine dpsCoroutine;
    private bool didFireSwordPicked;

    private void Start()
    {
        damageable = GetComponent<IDamageable>();
        damageable.OnTakeDamage += OnTakeDamage;
        damageable.OnHealthChange += OnHealthChange;
    }

    private void OnTakeDamage(DamageEffect effect)
    {
        if (effect == DamageEffect.Fire)
        {
            if (dpsCoroutine == null)
            {
                dpsCoroutine = StartCoroutine(ApplyDamageRoutine());
            }
        }
    }

    private void OnHealthChange(float healthNormalized)
    {
        if (healthNormalized <= 0)
        {
            ResetFireEffect();
        }
    }

    private IEnumerator ApplyDamageRoutine()
    {
        fireParticle.gameObject.SetActive(true);

        var time = 0f;
        var damageTimer = 0f;
        while (time < effectDuration)
        {
            time += Time.deltaTime;
            damageTimer += Time.deltaTime;
            if (damageTimer >= 1f)
            {
                damageable.TakeDamage(dps);
                damageTimer = 0f;
            }
            yield return null;
        }

        fireParticle.gameObject.SetActive(false);
        dpsCoroutine = null;
    }

    private void ResetFireEffect()
    {
        if (dpsCoroutine != null)
            StopCoroutine(dpsCoroutine);

        fireParticle.gameObject.SetActive(false);
    }
}
