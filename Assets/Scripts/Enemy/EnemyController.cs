using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, IDamageable
{
    public event Action<Collider> NotifyOnTriggerEnter;
    public event Action<Collider> NotifyOnTriggerStay;
    public event Action<Collider> NotifyOnTriggerExit;
    public event Action<IDamageable> OnDeath;
    public event Action<float> OnHealthChange;
    public event Action<DamageEffect> OnTakeDamage;

    [SerializeField] protected EnemyStats stats;
    [SerializeField] protected EnemyAnimationEventHandler animationEventHandler;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator animator;
    [SerializeField] protected CapsuleCollider trigger;
    [SerializeField] protected ParticleSystem deathParticle;

    protected float health;
    protected float damage;
    protected float speed;
    protected float chasingRange;
    protected float attackRange;

    protected Vector3 initialPosition;
    protected Quaternion initialRotation;

    protected Dictionary<EnemyStates, EnemyState> enemyStatesMap = new Dictionary<EnemyStates, EnemyState>();
    protected List<IDamageable> activeTargets = new List<IDamageable>();

    protected EnemyState currentState;

    public float Damage => damage;
    public float ChasingRange => chasingRange;
    public float AttackRange => attackRange;
    public int ActiveTargetsCount => activeTargets.Count;
    public Vector3 InitialPositon => initialPosition;
    public Quaternion InitialRotation => initialRotation;
    public EnemyAnimationEventHandler AnimationEventHandler => animationEventHandler;
    public Animator Animator => animator;
    public NavMeshAgent Agent => agent;
    public Collider Trigger => trigger;
    public ParticleSystem DeathParticle => deathParticle;

    protected void Awake()
    {
        health = stats.health;
        damage = stats.damage;
        speed = stats.speed;
        chasingRange = stats.chaseRange;
        attackRange = stats.attackRange;

        agent.speed = speed;
        trigger.radius = chasingRange;
    }

    protected void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        InitializeStatesMap();
        currentState = enemyStatesMap[EnemyStates.Idle];

        if (GameManager.Instance.IsInCave)
            CaveGameManager.Instance.RegisterEnemy(this);

        agent.Warp(transform.position);
    }

    protected virtual void InitializeStatesMap()
    {
        enemyStatesMap.Add(EnemyStates.Idle, new EnemyIdleState(this));
        enemyStatesMap.Add(EnemyStates.ReturnHome, new EnemyReturnHomeState(this));
        enemyStatesMap.Add(EnemyStates.Chase, new EnemyChasingState(this));
        enemyStatesMap.Add(EnemyStates.Attack, new EnemyAttackingState(this));
        enemyStatesMap.Add(EnemyStates.Dead, new EnemyDeadState(this));
        enemyStatesMap.Add(EnemyStates.Tackled, new EnemyTackledState(this));
    }

    protected void Update()
    {
        currentState.Execute();
    }

    public void TakeDamage(float damage, DamageEffect effect = DamageEffect.None)
    {
        if (health > 0)
        {
            health -= damage;
            var normalizedHealth = (health / stats.health).ClampNormalized();
            OnHealthChange?.Invoke(normalizedHealth);
            OnTakeDamage?.Invoke(effect);

            if (health <= 0)
            {
                health = 0;
                OnDeath?.Invoke(this);
                SwitchState(EnemyStates.Dead);
            }
            else
            {
                //SwitchState(EnemyStates.Tackled);
            }
        }
    }

    public float GetHealth() => health;

    protected void OnTriggerEnter(Collider other) => NotifyOnTriggerEnter?.Invoke(other);
    protected void OnTriggerStay(Collider other) => NotifyOnTriggerStay?.Invoke(other);
    protected void OnTriggerExit(Collider other) => NotifyOnTriggerExit?.Invoke(other);

    public void RegisterEnemy(IDamageable enemy) => activeTargets.Add(enemy);
    public void DiscardEnemy(IDamageable enemy) => activeTargets.Remove(enemy);

    public void SwitchState(EnemyStates newState)
    {
        if (enemyStatesMap[newState] != currentState)
        {
            currentState.OnStateEnd();
            currentState = enemyStatesMap[newState];
            currentState.OnStateStart();
        }
    }

    public IDamageable GetClosestDamageable()
    {
        IDamageable closest = null;
        float closestDistance = float.MaxValue;

        foreach (var enemy in activeTargets)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closest = enemy;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private void OnDestroy()
    {
        OnHealthChange = (value) => { };
    }
}
