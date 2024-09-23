using System.Collections;
using UnityEngine;

public class AnimEffect : PoolableObject
{
    [SerializeField] float particleLifetime;

    private void OnEnable()
    {
        StartCoroutine(DeactivateEffet(particleLifetime));
    }

    private IEnumerator DeactivateEffet(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
