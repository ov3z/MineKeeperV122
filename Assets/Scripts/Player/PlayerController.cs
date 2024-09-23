using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using YsoCorp.GameUtils;

public class PlayerController : MonoBehaviour, ICollector, IDamageable, ITradeMaker
{
    public static PlayerController Instance { get; private set; }

    public event Action<Collider> OnPlayerTriggerEnter;
    public event Action<Collider> OnPlayerTriggerExit;
    public event Action<Collider> OnPlayerTriggerStay;

    public event Action<ResourceTypes, float> NotifyOnResourceCollect;
    public event Action<States> OnStateChange;
    public event Action OnGatherAnimEvent;
    public event Action OnAttackAnimEvent;
    public event Action OnEnableTrailAnimEvent;
    public event Action OnDisableTrailAnimEvent;
    public event Action<IDamageable> OnDeath;
    public event Action<float> OnHealthChange;
    public event Action<DamageEffect> OnTakeDamage;
    public event Action<PlayerSkinSetup> OnPlayerSkinChange;
    public event Action OnSkinChange;
    public event Action<float> OnSpeedChangeNormalized;

    [SerializeField] List<GenderSkinSet> skins = new();

    [SerializeField] CharacterStats stats;
    [SerializeField] List<PlayerFollowPoint> followPoints;
    [SerializeField] Transform resourcePickCanvas;
    [SerializeField] ParticleSystem deathParticle;
    [SerializeField] CinemachineVirtualCamera mainVCam;
    [SerializeField] PlayerGender gender;

    private List<PlayerToolUnit> toolUnits;
    private Dictionary<States, StateBase> statesMap = new();
    private Dictionary<string, Transform> toolsMap = new();
    private Dictionary<string, SoundTypes> toolsSoundsMap = new();
    private List<ICollectable> collectables = new();
    private List<IDamageable> attackTargets = new();
    private Dictionary<PlayerGender, List<PlayerSkinSetup>> genderSkinMap = new();

    private AnimationEventHandler animationEventHandler;
    private Renderer playerRenderer;
    private IInteractable interactionTarget;
    private StateBase currentState;
    private StateBase currentSubState;
    private Transform visual;
    private bool isDoingAnyJob;
    private bool hasSubstate;
    private bool isLying;
    private float gatherPower;
    private float health;
    private float damage;

    private const string intersitialKey = "DidUnlockInterstitials";
    private float interstitalTimer;
    private float interstitalTimerMax = 60;

    private float abAfterCaveInterstitialTimer = 0;
    private float abAfterCaveInterstitialTimerMax = 60;
    public bool inAfterCaveAB;

    private float abOneInteractionInterstitialTimer = 0;
    private float abOneInteractionInterstitialTimerMax = 60;

    private PlayerSkinSetup skinSetup;

    public Animator Animator { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public CharacterStats Stats => stats;
    public bool IsDoingAnyJob => isDoingAnyJob;
    public bool HasntSubstate => !hasSubstate;
    public float Damage => damage;
    public float AttackRange => stats.attackRange;
    public ParticleSystem DeathParticle => deathParticle;
    public bool isVisible => playerRenderer.isVisible;
    public bool IsLying => isLying;
    public IInteractable InteractionTarget => GetInteractionTarget();
    public bool IsThereFreeSpace => InventoryController.Instance.IsThereSpaceInTheInventory;
    public PlayerSkinSetup SkinSetup => skinSetup;

    public Action ResetIntersatials =>
        () =>
        {
            interstitalTimer = 0;
            PlayerPrefs.SetFloat("InterstitialTimer", interstitalTimer);
        };

    public bool CanShowIntersatital { get; private set; }

    private int SelectedSkinData
    {
        get { return PlayerPrefs.GetInt("SelectedSkin", PlayerPrefs.GetInt("Gender", 0) * 10); }
    }

    private bool DidUnlckInterstitial
    {
        get { return PlayerPrefs.GetInt(intersitialKey, 0) == 1; }
        set
        {
            if (value == true)
            {
                PlayerPrefs.SetInt(intersitialKey, 1);
            }
            else
            {
                PlayerPrefs.SetInt(intersitialKey, 0);
            }
        }
    }

    private void Awake()
    {
        if (PlayerPrefs.GetInt("Gender", 0) != (int)gender)
        {
            Destroy(gameObject);
            return;
        }

        InitializeGenderSkinMap();

        Instance = this;

        var skinIndex = SelectedSkinData % 10;
        var selectedGender = (SelectedSkinData / 10 == 0) ? PlayerGender.Male : PlayerGender.Female;

        ChangeSkin(skinIndex, selectedGender);

        Rigidbody = GetComponent<Rigidbody>();

        gatherPower = PlayerPrefs.GetFloat("Axe", stats.power);
        health = PlayerPrefs.GetFloat("Health", stats.health);
        damage = PlayerPrefs.GetFloat("Attack", stats.damage);

        interstitalTimer = PlayerPrefs.GetFloat("InterstitialTimer", 0);

        SaveStats();

        //InitializeToolsMap();
    }

    private void InitializeToolsMap()
    {
        toolsMap.Clear();
        toolsSoundsMap.Clear();
        foreach (var tool in toolUnits)
        {
            toolsMap.Add(tool.animationKeyForTool, tool.toolTransform);
            toolsSoundsMap.Add(tool.animationKeyForTool, tool.toolSound);
        }
    }

    private void InitializeGenderSkinMap()
    {
        foreach (var skinSet in skins)
        {
            genderSkinMap.Add(skinSet.gender, skinSet.skins);
        }
    }

    private void SetUpCamTarget(Transform target)
    {
        mainVCam.Follow = target;
        mainVCam.LookAt = target;
    }

    private void Start()
    {
        InitializeSatesMap();
        SetState(States.Idle);

        GameManager.Instance.OnFireSwordPick += ShowSword;
    }

    private void FireGatherAnimEvent() => OnGatherAnimEvent?.Invoke();
    private void FireAttackAnimEvent() => OnAttackAnimEvent?.Invoke();
    private void OnTurnOnAnimationEvent() => OnEnableTrailAnimEvent?.Invoke();
    private void FireTurnOffAnimationEvent() => OnDisableTrailAnimEvent?.Invoke();

    private void ShowSword(DamageEffect _)
    {
        SetState(States.ShowSword);
    }

    private void InitializeSatesMap()
    {
        statesMap.Add(States.Idle, new IdleState(this));
        statesMap.Add(States.Move, new MoveState(this));
        statesMap.Add(States.Gather, new GatherState(this));
        statesMap.Add(States.Fight, new FightState(this));
        statesMap.Add(States.Death, new DeathState(this));
        statesMap.Add(States.ShowSword, new ShowSwordState(this));
    }

    private void Update()
    {
        currentState?.Execute();
        currentSubState?.Execute();

        AfterCaveStartIntersitialUnlockRoutine();
        if (_oneInteraction)
        {
            Debug.Log($"one interaction in update {_oneInteraction}");
            abOneInteractionInterstitialTimer += Time.deltaTime;
            Debug.Log($"timer of one ab {abOneInteractionInterstitialTimer}");
        }
    }

    private void FixedUpdate()
    {
        currentState.FixedExecute();
    }

    private void OnTriggerEnter(Collider other) => OnPlayerTriggerEnter?.Invoke(other);
    private void OnTriggerStay(Collider other) => OnPlayerTriggerStay?.Invoke(other);
    private void OnTriggerExit(Collider other) => OnPlayerTriggerExit?.Invoke(other);

    public void RemoveCollectable(ICollectable collectable) => collectables.Remove(collectable);

    public void AddCollectable(ICollectable collectable)
    {
        if (!collectables.Contains(collectable))
        {
            collectables.Add(collectable);
        }
    }

    public ICollectable GetClosestCollectable()
    {
        ICollectable closestCollectable = null;

        if (collectables.Count == 0) return closestCollectable;

        float closestDistance = Mathf.Infinity;
        foreach (var collectable in collectables)
        {
            float distance = Vector3.Distance(collectable.GetPosition(), transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollectable = collectable;
            }
        }

        return closestCollectable;
    }

    public List<ICollectable> GetCollectablesInSightOfView()
    {
        List<ICollectable> visibleCollectibles = new List<ICollectable>();

        ICollectable closestcollectable = GetClosestCollectable();

        if (closestcollectable == null)
            return visibleCollectibles;

        visibleCollectibles.Add(closestcollectable);

        float maxVisibilityAngle = closestcollectable.GetVisibilityAngle();

        if (maxVisibilityAngle >= 60)
        {
            foreach (var collectable in collectables)
            {
                if (!visibleCollectibles.Contains(collectable) &&
                    collectable.GetResourceType() == closestcollectable.GetResourceType())
                {
                    Vector3 dirTotarget = (transform.position - collectable.GetPosition()).normalized;
                    float cosOfAngleTotarget = Vector3.Dot(-transform.forward, dirTotarget);
                    float minCosToTarget = Mathf.Cos(closestcollectable.GetVisibilityAngle() * Mathf.Deg2Rad);

                    if (cosOfAngleTotarget > minCosToTarget &&
                        Vector3.Distance(closestcollectable.GetPosition(), transform.position) < 1.3f)
                    {
                        if (!visibleCollectibles.Contains(collectable))
                        {
                            visibleCollectibles.Add(collectable);
                        }
                    }
                }
            }
        }

        return visibleCollectibles;
    }

    public void RemoveAttackTarget(IDamageable target) => attackTargets.Remove(target);

    public void AddAttackTarget(IDamageable target)
    {
        if (!attackTargets.Contains(target))
        {
            attackTargets.Add(target);

            target.OnDeath += (IDamageable sender) => { RemoveAttackTarget(sender); };
        }
    }

    public IDamageable GetClosestDamagealble()
    {
        IDamageable closestDamageable = null;
        float closestDistance = float.MaxValue;

        foreach (var target in attackTargets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < closestDistance && target is not SoldierController)
            {
                closestDistance = distance;
                closestDamageable = target;
            }
        }

        return closestDamageable;
    }

    public void SetInteractionTarget(IInteractable interactable)
    {
        interactionTarget = interactable;
    }

    public void ResetInteractionTarget() => interactionTarget = null;
    public IInteractable GetInteractionTarget() => interactionTarget;

    public void SetState(States state)
    {
        if (currentState == statesMap[state]) return;

        currentState?.OnStateEnd();
        currentState = statesMap[state];
        currentState.OnStateStart();

        OnStateChange?.Invoke(state);
    }

    public void SetSubState(States subState)
    {
        if (currentSubState == statesMap[subState]) return;

        currentSubState?.OnStateEnd();
        currentSubState = statesMap[subState];
        currentSubState.OnStateStart();

        OnStateChange?.Invoke(subState);
        hasSubstate = true;
    }

    public void ResetSubstate()
    {
        currentSubState?.OnStateEnd();
        currentSubState = null;
        hasSubstate = false;
    }

    public void OnResourceCollect(ResourceTypes type, float collectedAmount)
    {
        ResourceStorage.Instance.ChangeResourceAmount(type, (int)collectedAmount);
        NotifyOnResourceCollect?.Invoke(type, collectedAmount);
    }

    public void OnResourceCollect(ResourceUnit sender)
    {
        /*implemented in interface, only for stickmen*/
    }

    public Transform GetDestinationTransform() => transform;

    public Transform GetToolTransform(string animKeyForToolUse) => toolsMap[animKeyForToolUse];
    public SoundTypes GetToolSound(string animKeyForToolUse) => toolsSoundsMap[animKeyForToolUse];

    public float GetGatherPower() => gatherPower;

    public float GetHealth() => health;

    public void TakeDamage(float damage, DamageEffect effect = DamageEffect.None)
    {
        health -= damage;

        var normalizedHealth = (health / stats.health).ClampNormalized();
        OnHealthChange?.Invoke(normalizedHealth);

        if (health <= 0)
        {
            SetState(States.Death);
        }
    }

    public void RestoreHealth()
    {
        health = stats.health;

        var normalizedHealth = (health / stats.health).ClampNormalized();
        OnHealthChange?.Invoke(normalizedHealth);
    }

    public void SetGatherPower(int newGatherPower)
    {
        gatherPower = newGatherPower;
    }

    public PlayerFollowPoint GetClosestPoint(Vector3 requestPosition)
    {
        float minDistance = float.MaxValue;
        PlayerFollowPoint closestPoint = null;
        foreach (var item in followPoints)
        {
            var distance = Vector3.Distance(item.GetPosition(), requestPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = item;
            }
        }

        followPoints.Remove(closestPoint);
        return closestPoint;
    }

    public void ReturnFollowPoint(PlayerFollowPoint point)
    {
        followPoints.Add(point);
    }

    private bool _afterCave;
    private bool _oneInteractionInternal;

    private bool _oneInteraction
    {
        get => _oneInteractionInternal;
        set
        {
            Debug.Log($"changing {_oneInteractionInternal} to {value}");
            _oneInteractionInternal = value;
        }
    }

    public void LieDown()
    {
        Animator.SetTrigger("LieDown");
        isLying = true;

#if ADS

        if (YCManager.instance.abTestingManager.IsPlayerSample("oneInteraction"))
        {
            _oneInteraction = true;
            StartCoroutine(StartIntersitialUnlockRoutine());
            Debug.Log($"one interaction has started in ab{_oneInteraction}");
        }

        else if (YCManager.instance.abTestingManager.IsPlayerSample("afterCave"))
        {
            AfterCaveStartIntersitialUnlockRoutine();
            inAfterCaveAB = true;
            Debug.Log($"In after cave ab {inAfterCaveAB}");
            Debug.Log("after cave intersititial started ");
            Debug.Log($"after cave boolean {_afterCave}");
        }
        else
        {
            _oneInteraction = false;
            StartCoroutine(StartIntersitialUnlockRoutine());
            Debug.Log("default interstitial started");
            Debug.Log($"one interaction in default {_oneInteraction}");
        }
#endif
    }

    #region ABDEFAULT&ABONEINTERACTION

    private IEnumerator StartIntersitialUnlockRoutine()
    {
        Debug.Log($"Did unlock {DidUnlckInterstitial}");
        yield return new WaitForSeconds(interstitalTimerMax);
#if UNITY_ANDROID || PLATFORM_ANDROID || PLATFORM_IOS || UNITY_IOS
        void OnSucceed()
        {
            DidUnlckInterstitial = true;
            /*if (!_oneInteraction)
            {
                _oneInteraction = true;
            }
            else
            {
                _oneInteraction = false;
            }*/
            Debug.Log($"Did unlock {DidUnlckInterstitial}");
            Debug.Log($"One interaction {_oneInteraction}");
        }

        //AdsContainer.Instance.ShowInsterstitial(OnSucceed);
        AdsContainer.Instance.ShowInsterstitialYso(OnSucceed); //yso interstitial
#elif UNITY_WEBGL || PLATFORM_WEBGL
        DidUnlckInterstitial = true;
        Debug.Log("interstitial showesd");
        CanShowIntersatital = true;
#endif
    }

    public void RegisterInteraction()
    {
        Debug.Log($"Did unlock register interaction  {DidUnlckInterstitial}");
        if (DidUnlckInterstitial)
        {
            interstitalTimer++;

            Debug.Log(interactionTarget);

            PlayerPrefs.SetFloat("InterstitialTimer", interstitalTimer);

            var isInCave = GameManager.Instance.IsInCave;
            if (!isInCave && ((interstitalTimer >= interstitalTimerMax)
                              || (interstitalTimer >= 1 && abOneInteractionInterstitialTimer >=
                                  abOneInteractionInterstitialTimerMax)))
            {
#if ADS
                TriggerIneterstital();
#endif
            }
        }
    }

    private IInteractable registeredInteractable;

    public void RegisterInteraction(IInteractable interactable)
    {
        if (DidUnlckInterstitial && (registeredInteractable != interactable))
        {
            interstitalTimer++;

            Debug.Log(interactionTarget);

            Debug.Log($"interstitial timer {interstitalTimer}");
            var isInCave = GameManager.Instance.IsInCave;
            if (!isInCave && ((interstitalTimer >= interstitalTimerMax)
                              || (interstitalTimer >= 1 && abOneInteractionInterstitialTimer >=
                                  abOneInteractionInterstitialTimerMax)))
            {
#if ADS
                Debug.Log(
                    $"one interaction interstitial timer >= interstitial timer max {interstitalTimer >= interstitalTimerMax}" +
                    $"interstitial >= 1 case {interstitalTimer >= 1}, abTimer {abOneInteractionInterstitialTimer >= abOneInteractionInterstitialTimerMax}");
                TriggerIneterstital();
#endif
            }

            registeredInteractable = interactable;
        }
    }

    private void TriggerIneterstital()
    {
#if UNITY_ANDROID || PLATFORM_ANDROID || PLATFORM_IOS || UNITY_IOS
        void ResetInterstitialTimer()
        {
            if (_oneInteraction)
            {
                Debug.Log($"one interaction of ab in triggerinterstitial {_oneInteraction}");
                abOneInteractionInterstitialTimer = 0;
                interstitalTimer = 0;
            }
            else
            {
                Debug.Log($"one interaction of default in trigger interstitial {_oneInteraction}");
                interstitalTimer = 0;
            }
        }

        //AdsContainer.Instance.ShowInsterstitial(ResetInterstitialTimer);
        AdsContainer.Instance.ShowInsterstitialYso(ResetInterstitialTimer); //yso interstitial
        Debug.Log(
            $"Trigger interstitial timer {interstitalTimer} , ab interstitial timer {abOneInteractionInterstitialTimer}");


#elif UNITY_WEBGL || PLATFORM_WEBGL
                CanShowIntersatital = true;
#endif
    }

    #endregion

    #region ABAFTERCAVE

    private void AfterCaveStartIntersitialUnlockRoutine()
    {
        if (PlayerPrefs.GetInt("LevelCave", 0) >= 1)
        {
            /*AdsContainer.Instance.ShowInsterstitialYso(OnSucceed); */
            OnSucceed();
        }

        void OnSucceed()
        {
            _afterCave = true;
            inAfterCaveAB = false;
            Debug.Log($"After cave {_afterCave} , in after cave ab bool {inAfterCaveAB}");
            AfterCaveInterstitial();
        }
    }


    public void AfterCaveInterstitial()
    {
        if (_afterCave)
        {
            Debug.Log($"after cave boolean {_afterCave}");
            abAfterCaveInterstitialTimer += Time.deltaTime;
            Debug.Log($"interstitial timer {abAfterCaveInterstitialTimer}");
            var isInCave = GameManager.Instance.IsInCave;
            if (abAfterCaveInterstitialTimer >= abAfterCaveInterstitialTimerMax && !isInCave)
            {
                TriggerIneterstitalAB();
            }
        }
    }

    private void TriggerIneterstitalAB()
    {
#if UNITY_ANDROID || PLATFORM_ANDROID || PLATFORM_IOS || UNITY_IOS

        void ResetInterstitialTimer()
        {
            abAfterCaveInterstitialTimer = 0;
            PlayerPrefs.SetFloat("InterstitialTimer", abAfterCaveInterstitialTimer);
        }

        AdsContainer.Instance.ShowInsterstitialYso(ResetInterstitialTimer); //yso interstitial
        Debug.Log($"Trigger interstitial timer {abAfterCaveInterstitialTimer}");
    }
#endif

    #endregion

    public void ChangeSkin(int skinIndex, PlayerGender gender)
    {
        skinSetup?.gameObject.SetActive(false);
        var newSkin = genderSkinMap[gender][skinIndex];
        skinSetup = newSkin;
        skinSetup.gameObject.SetActive(true);

        toolUnits = newSkin.ToolUnits;
        InitializeToolsMap();

        playerRenderer = newSkin.PlayerRenderer;

        if (animationEventHandler != null)
        {
            animationEventHandler.OnGatherAnimationEvent -= FireGatherAnimEvent;
            animationEventHandler.OnFightAnimationEvent -= FireAttackAnimEvent;

            animationEventHandler.OnTurnOnAnimationEvent -= OnTurnOnAnimationEvent;
            animationEventHandler.OnTurnOffAnimationEvent -= FireTurnOffAnimationEvent;
        }

        animationEventHandler = newSkin.AnimationEventHandler;

        animationEventHandler.OnGatherAnimationEvent += FireGatherAnimEvent;
        animationEventHandler.OnFightAnimationEvent += FireAttackAnimEvent;

        animationEventHandler.OnTurnOnAnimationEvent += OnTurnOnAnimationEvent;
        animationEventHandler.OnTurnOffAnimationEvent += FireTurnOffAnimationEvent;


        SetUpCamTarget(newSkin.VisualTransform);
        Animator = newSkin.Animator;

        OnSkinChange?.Invoke();
    }

    public void GetUp()
    {
        Animator.SetTrigger("GetUp");
        StartCoroutine(DisableLyingRoutine());
    }

    public void SpeedUp()
    {
        stats.speed = 12;
    }

    public void SlowDown()
    {
        stats.speed = 8;
    }

    public void FireOnNormalizedSpeedChange(float normalizedSpeed)
    {
        OnSpeedChangeNormalized?.Invoke(normalizedSpeed);
    }

    private IEnumerator DisableLyingRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        isLying = false;
    }

    private void SaveStats()
    {
        PlayerPrefs.SetFloat("Axe", gatherPower);
        PlayerPrefs.SetFloat("Health", health);
        PlayerPrefs.SetFloat("Attack", damage);
    }

    public float GetResourceBalance(ResourceTypes resourceType)
    {
        return ResourceStorage.Instance.GetResourceBalance(resourceType);
    }

    public void ChangeResourceAmount(ResourceTypes resourceTypes, int amount)
    {
        ResourceStorage.Instance.ChangeResourceAmount(resourceTypes, amount);
    }

    [System.Serializable]
    public class PlayerToolUnit
    {
        public string animationKeyForTool;
        public Transform toolTransform;
        public SoundTypes toolSound;
    }

    [System.Serializable]
    public class GenderSkinSet
    {
        public PlayerGender gender;
        public List<PlayerSkinSetup> skins;
    }

    public enum PlayerGender
    {
        Male,
        Female
    }
}