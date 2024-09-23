using DG.Tweening;
using UnityEngine;

public class FightState : StateBase
{
    private Animator animator => ownerController.Animator;
    private float damage;
    private float attackRange;
    private IDamageable target;
    private Transform sword;
    private Transform toolTrail;
    private Tween swordScaleTween;
    private DamageEffect damageEffect = DamageEffect.None;

    private const string TOOL_KEY = "Sword";


    public FightState(PlayerController playerController) : base(playerController)
    {
        damage = playerController.Damage;
        attackRange = playerController.AttackRange;

        ownerController.OnStateChange += HandleOnStateChange;
        GameManager.Instance.OnFireSwordPick += DamageEffectPick;
    }

    private void DamageEffectPick(DamageEffect effect)
    {
        damageEffect = effect;
    }

    private void HandleOnStateChange(States newState)
    {
        if (newState == States.Gather)
            swordScaleTween?.Complete();
    }

    public override void OnStateStart()
    {
        animator.SetLayerWeight(1, 1);
        animator.SetBool(TOOL_KEY, true);

        ownerController.OnEnableTrailAnimEvent += EnableToolTrail;
        ownerController.OnDisableTrailAnimEvent += DisableToolTrail;

        ownerController.OnAttackAnimEvent += OnAttackAnimEvent;
        target = ownerController.GetClosestDamagealble();
        sword = ownerController.GetToolTransform(TOOL_KEY);

        if (sword.GetChild(sword.childCount - 2).GetComponent<TrailRenderer>())
        {
            toolTrail = sword.GetChild(sword.childCount - 2);
        }

        swordScaleTween?.Kill();
        swordScaleTween = sword.DOScale(1, 0.2f).SetEase(Ease.Linear);
    }

    private void EnableToolTrail()
    {
        if (toolTrail)
            toolTrail.gameObject.SetActive(true);
    }

    private void DisableToolTrail()
    {
        if (toolTrail)
            toolTrail.gameObject.SetActive(false);
    }


    public override void Execute()
    {
        float distanceToTarget = Vector3.Distance(ownerController.transform.position, target.transform.position);
        if (distanceToTarget > attackRange)
        {
            ownerController.ResetSubstate();
        }
    }

    private void OnAttackAnimEvent()
    {
        target.TakeDamage(damage, damageEffect);

        SoundManager.Instance.Play(SoundTypes.Sword);
        ownerController.RegisterInteraction();

        if (target.GetHealth() <= 0)
        {
            target = ownerController.GetClosestDamagealble();

            if (target == null)
            {
                ownerController.ResetSubstate();
            }
        }

        HapticManager.Instance.PlayHaptics(HapticIntensity.Medium);


        TryPlayHitParticle();
    }

    private bool TryPlayHitParticle()
    {
        bool isThereHitParticle = false;
        if (sword.GetLastChild().TryGetComponent<ParticleSystem>(out var hitParticle))
        {
            PoolableObject hitParticleinstance = PoolingSystem.Instance.GetParticlePool(TOOL_KEY).GetObject();
            hitParticleinstance.transform.SetPositionAndRotation(hitParticle.transform.position, hitParticle.transform.rotation);
            hitParticleinstance.gameObject.SetActive(true);
            isThereHitParticle = true;
        }
        return isThereHitParticle;
    }

    public override void OnStateEnd()
    {
        animator.SetLayerWeight(1, 0);
        animator.SetBool(TOOL_KEY, false);

        ownerController.OnEnableTrailAnimEvent -= EnableToolTrail;
        ownerController.OnDisableTrailAnimEvent -= DisableToolTrail;

        ownerController.OnAttackAnimEvent -= OnAttackAnimEvent;

        swordScaleTween?.Kill();
        swordScaleTween = sword.DOScale(0, 0.3f).SetEase(Ease.Linear).SetDelay(0.7f);
        DisableToolTrail();
    }
}
