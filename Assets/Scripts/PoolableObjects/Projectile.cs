using System.Collections;
using UnityEngine;

public class Projectile : PoolableObject
{
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private float lifetime;

    private Vector3 direction;
    private float damage;
    private Coroutine selfDestructionCoroutine;
    public void SetDirection(Vector3 direction)
    {
        this.direction = direction;
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    private void OnEnable()
    {
        rb.velocity = direction * speed;
        ResetHitParticle();
        selfDestructionCoroutine = StartCoroutine(SelfDestrucAfter(lifetime));
    }

    private void ResetHitParticle()
    {
        hitParticle.Clear();
        hitParticle.transform.SetParent(transform);
        hitParticle.transform.localPosition = Vector3.zero;
    }

    private IEnumerator SelfDestrucAfter(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            rb.velocity = Vector3.zero;

            EnableHitParticle();

            gameObject.SetActive(false);
        }
    }

    private void EnableHitParticle()
    {
        hitParticle.Play();
        hitParticle.transform.SetParent(null);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        StopCoroutine(selfDestructionCoroutine);
    }
}

public enum ProjectileTypes
{
    SnakePoision,
    SpiderPoision
}