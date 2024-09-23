using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoldierController : MonoBehaviour, IDamageable, ICollector
{
    public event Action<Collider> NotifyOnTriggerEnter;
    public event Action<Collider> NotifyOnTriggerStay;
    public event Action<Collider> NotifyOnTriggerExit;
    public event Action<IDamageable> OnDeath;
    public event Action<ResourceTypes, float> NotifyOnResourceCollect;
    public event Action<float> OnHealthChange;
    public event Action<DamageEffect> OnTakeDamage;

    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private AnimationEventHandler animationEventHandler;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem deathParticle;
    [SerializeField] private Transform weapon;
    [SerializeField] private Transform tool;
    [SerializeField] private SoldierType soldierType;
    [SerializeField] private Rigidbody rb;
    [SerializeField] new private Renderer renderer;

    private float health;
    private float damage;
    private float speed;
    private float chasingRange;
    private float attackRange;
    private float maxHealth;

    private float minDistanceToThePlayer = 3f;
    private float maxDistanceToThePlayer = 7f;

    private int gatherPower = 1;
    private int soldierLevel;

    private bool didGameStarted;

    private Dictionary<SoldierStates, SoldierState> soldierStatesMap = new Dictionary<SoldierStates, SoldierState>();
    private List<IDamageable> activeTargets = new List<IDamageable>();

    private Transform homeCell;
    private Transform playerTransform;
    private SoldierState currentState;

    private bool canFollowPlayer => didGameStarted && currentState.NeedFollowPlayer;

    public float Damage => damage;
    public float ChasingRange => chasingRange;
    public float AttackRange => attackRange;
    public float MinDistanceToThePlayer => minDistanceToThePlayer;
    public float MaxDistanceToThePlayer => maxDistanceToThePlayer;
    public int ActiveTargetsCount => activeTargets.Count;
    public AnimationEventHandler AnimationEventHandler => animationEventHandler;
    public Animator Animator => animator;
    public NavMeshAgent Agent => agent;
    public Transform HomeCell { get => homeCell; set { homeCell = value; } }
    public Transform Weapon => weapon;
    public Transform Tool => tool;
    public Transform PlayerTransform => playerTransform;
    public ParticleSystem DeathParticle => deathParticle;
    public bool DidGameStarted => didGameStarted;
    public Renderer Renderer => renderer;
    public Tween toolAnimTween { get; set; }

    private void Awake()
    {
        soldierLevel = PlayerPrefs.GetInt($"UpgradeLevel{(int)soldierType}", 0);

        maxHealth = characterStats.health + 10 * soldierLevel;
        health = maxHealth;
        damage = characterStats.damage + 2 * soldierLevel;

        speed = characterStats.speed;
        attackRange = characterStats.attackRange;

        agent.speed = speed;
    }

    private void Start()
    {
        agent.updatePosition = false;

        InitializeStatesMap();

        if (GameManager.Instance.IsInCave)
        {
            currentState = soldierStatesMap[SoldierStates.Idle];
            RegisterAndGetReadyToFollow();
        }
        else
        {
            currentState = soldierStatesMap[SoldierStates.GoToHomeCell];
        }

        currentState.OnStateStart();

        playerTransform = PlayerController.Instance.transform;

        if (GameManager.Instance.IsInCave)
            CaveGameManager.Instance.OnPlayerLose += () =>
            {
                SwitchState(SoldierStates.Death);
            };
    }

    private void InitializeStatesMap()
    {
        soldierStatesMap.Add(SoldierStates.Idle, new SoldierIdleState(this));
        soldierStatesMap.Add(SoldierStates.Chase, new SoldierChaseState(this));
        soldierStatesMap.Add(SoldierStates.Fight, new SoldierFightState(this));
        soldierStatesMap.Add(SoldierStates.Death, new SoldierDeathState(this));
        soldierStatesMap.Add(SoldierStates.Mine, new SoldierMineState(this));
        soldierStatesMap.Add(SoldierStates.GoToOre, new SoldierGoToOreState(this));
        soldierStatesMap.Add(SoldierStates.GoToHomeCell, new SoldierGoToHomeCellState(this));
        soldierStatesMap.Add(SoldierStates.FollowPlayer, new SoldierFollowPlayerState(this));
    }

    private void RegisterAndGetReadyToFollow()
    {
        CaveGameManager.Instance.AddSoldier(this);
        PlayerController.Instance.OnStateChange += StartFollowing;
    }

    private void StartFollowing(States newState)
    {
        if (newState == States.Move)
        {
            PlayerController.Instance.OnStateChange -= StartFollowing;
            MarkGameAsStarted();
            SwitchState(SoldierStates.FollowPlayer);
        }
    }

    private void Update()
    {
        currentState?.Execute();

        rb.velocity = agent.velocity;

        float speedVelocityNormalized = agent.velocity.magnitude / agent.speed;
        animator.SetFloat("Speed", speedVelocityNormalized);

        if (GameManager.Instance.IsInCave)
            SortTargetPriority();
    }

    private void SortTargetPriority()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        ICollectable closestOre = GetClosestOre();
        IDamageable closestDamageable = GetClosestDamageable();

        float distanceToClosestOre = float.MaxValue;
        float distanceToClosestDamageable = float.MaxValue;

        if (closestOre != null)
        {
            distanceToClosestOre = Vector3.Distance(playerTransform.position, closestOre.transform.position);
        }
        if (closestDamageable != null)
        {
            distanceToClosestDamageable = Vector3.Distance(playerTransform.position, closestDamageable.transform.position);
        }

        if (distanceToClosestDamageable < distanceToClosestOre)
        {
            if (distanceToClosestDamageable < maxDistanceToThePlayer)
            {
                if (currentState.canChaseEnemies)
                {
                    SwitchState(SoldierStates.Chase);
                    return;
                }
            }
            else if (distanceToPlayer > currentState.distanceToFollowPlayer && currentState.NeedFollowPlayer)
            {
                SwitchState(SoldierStates.FollowPlayer);
            }
        }
        else
        {
            if (distanceToClosestOre < maxDistanceToThePlayer)
            {
                if (currentState.canGoToMine)
                {
                    SwitchState(SoldierStates.GoToOre);
                    return;
                }
            }
            else if (distanceToPlayer > currentState.distanceToFollowPlayer && currentState.NeedFollowPlayer)
            {
                SwitchState(SoldierStates.FollowPlayer);
            }
        }
    }

    public void TakeDamage(float damage, DamageEffect effect = DamageEffect.None)
    {
        if (health > 0)
        {
            health -= damage;
            var normalizedHealth = (health / maxHealth).ClampNormalized();
            OnHealthChange?.Invoke(normalizedHealth);

            if (health <= 0)
            {
                health = 0;
                OnDeath?.Invoke(this);
                SwitchState(SoldierStates.Death);
            }
        }
    }

    public float GetHealth() => health;

    private void OnTriggerEnter(Collider other) => NotifyOnTriggerEnter?.Invoke(other);
    private void OnTriggerStay(Collider other) => NotifyOnTriggerStay?.Invoke(other);
    private void OnTriggerExit(Collider other) => NotifyOnTriggerExit?.Invoke(other);

    public void SwitchState(SoldierStates newState)
    {
        if (currentState.GetType() == soldierStatesMap[newState].GetType())
            return;

        currentState.OnStateEnd();
        currentState = soldierStatesMap[newState];
        currentState.OnStateStart();
    }

    public IDamageable GetClosestDamageable() => CaveGameManager.Instance.GetClosestEnemyToThePoint(transform.position);
    public Ore GetClosestOre() => CaveGameManager.Instance.GetClosestOreToPoint(transform.position);

    public void OnResourceCollect(ResourceTypes type, float collectedAmount)
    {
        ResourceStorage.Instance.ChangeResourceAmount(type, (int)collectedAmount);
        NotifyOnResourceCollect?.Invoke(type, collectedAmount);
    }

    public void OnResourceCollect(ResourceUnit sender) { }

    public Transform GetDestinationTransform() => transform;

    public float GetGatherPower() => gatherPower;

    public void MarkGameAsStarted() => didGameStarted = true;
}