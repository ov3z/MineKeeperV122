using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierFightState : SoldierState
{
    private const string ATTACK_ANIM_KEY = "Attack";

    private Animator animator;
    private AnimationEventHandler animationEventHandler;
    private IDamageable target;
    private float attackDistance;
    private float damage;
    private Transform weapon;

    public override bool NeedFollowPlayer => true;

    public SoldierFightState(SoldierController soldierController) : base(soldierController)
    {
        animator = ownerController.Animator;
        animationEventHandler = ownerController.AnimationEventHandler;
        weapon = ownerController.Weapon;
        attackDistance = ownerController.AttackRange;
        damage = ownerController.Damage;

        canChaseEnemies = false;
        canGoToMine = false;
    }

    public override void OnStateStart()
    {
        animator.SetBool(ATTACK_ANIM_KEY, true);
        animationEventHandler.OnFightAnimationEvent += DealDamage;
        animationEventHandler.OnFightAnimationEvent += PlayHitParticle;
        SetNewTarget();
    }

    private void SetNewTarget()
    {
        target = ownerController.GetClosestDamageable();

        if (target == null)
        {
            ownerController.SwitchState(SoldierStates.FollowPlayer);
            return;
        }

        float distanceTargetPlayer = Vector3.Distance(target.transform.position, PlayerController.Instance.transform.position);

        if (distanceTargetPlayer > ownerController.MaxDistanceToThePlayer)
        {
            ownerController.SwitchState(SoldierStates.FollowPlayer);
        }
        else
        {
            target.OnDeath += TargetDied;
        }
    }

    private void TargetDied(IDamageable target)
    {
        target.OnDeath -= TargetDied;
        SetNewTarget();
    }

    private void DealDamage()
    {
        if (target == null)
        {
            SetNewTarget();
            return;
        }

        SoundManager.Instance.Play(SoundTypes.Sword);
        target.TakeDamage(damage);

        if (target.GetHealth() <= 0)
        {
            SetNewTarget();
        }
    }

    private void PlayHitParticle()
    {
        if (weapon.childCount > 0)
        {
            if (weapon.GetLastChild().TryGetComponent<ParticleSystem>(out var hitParticle))
            {
                PoolableObject hitParticleInstance = PoolingSystem.Instance.GetParticlePool(weapon.name).GetObject();
                hitParticleInstance.transform.SetPositionAndRotation(hitParticle.transform.position, hitParticle.transform.rotation);
                hitParticleInstance.gameObject.SetActive(true);
            }
        }
    }

    public override void Execute()
    {
        if (target == null || target.GetHealth() <= 0)
        {
            SetNewTarget();

            if (target == null)
                return;
        }

        LookToTheAttackTerget();

        float distance = Vector3.Distance(ownerController.transform.position, target.transform.position);

        if (distance >= attackDistance)
        {
            IDamageable closestDamageable = ownerController.GetClosestDamageable();

            if (closestDamageable == null)
            {
                ownerController.SwitchState(SoldierStates.FollowPlayer);
            }
            else
            {
                float distance_2 = Vector3.Distance(ownerController.transform.position, closestDamageable.transform.position);
                if (distance_2 <= attackDistance)
                {
                    SetNewTarget();
                }
                else
                {
                    float targetDistanceToPlayer = Vector3.Distance(ownerController.PlayerTransform.position, closestDamageable.transform.position);
                    if (targetDistanceToPlayer <= ownerController.MaxDistanceToThePlayer)
                    {
                        ownerController.SwitchState(SoldierStates.Chase);
                    }
                    else
                    {
                        ownerController.SwitchState(SoldierStates.FollowPlayer);
                    }
                }
            }
        }
    }

    private void LookToTheAttackTerget()
    {
        if (target != null)
        {
            Vector3 directionToTarget = (target.transform.position - ownerController.transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
            Vector3 eulerAngles = new Vector3(ownerController.transform.eulerAngles.x, angle, ownerController.transform.eulerAngles.z);
            ownerController.transform.rotation = Quaternion.Lerp(ownerController.transform.rotation, Quaternion.Euler(eulerAngles), 8 * Time.deltaTime);
        }
    }

    public override void OnStateEnd()
    {
        animator.SetBool(ATTACK_ANIM_KEY, false);

        animationEventHandler.OnFightAnimationEvent -= DealDamage;
        animationEventHandler.OnFightAnimationEvent -= PlayHitParticle;

        if (target != null)
            target.OnDeath -= TargetDied;
    }
}