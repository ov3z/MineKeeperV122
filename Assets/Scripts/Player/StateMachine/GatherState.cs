using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class GatherState : StateBase
{
    private Animator animator => ownerController.Animator;
    private Transform characterTransform;
    private Transform usedToolParent;
    private SoundTypes usedToolSound;
    private string gatherAnimKey = "";
    private ICollectable closestCollectable;
    private Tween scissorsAppearDisappearAnimation;
    private Transform toolTrail;
    private bool isStateValid;
    private float attackDistance;

    public GatherState(PlayerController playerController) : base(playerController)
    {
        characterTransform = ownerController.transform;
        ownerController.OnStateChange += HandleOnStateChange;
        attackDistance = ownerController.AttackRange;
    }

    private void HandleOnStateChange(States newState)
    {
        if (newState == States.Fight)
            scissorsAppearDisappearAnimation?.Complete();
    }

    public override void OnStateStart()
    {
        isStateValid = true;

        ownerController.ResetSubstate();

        ownerController.OnPlayerTriggerExit += OwnerController_OnPlayerTriggerExit;
        ownerController.OnPlayerTriggerEnter += OwnerController_OnPlayerTriggerEnter;
        ownerController.OnPlayerTriggerStay += OwnerController_OnPlayerTriggerStay;

        ownerController.OnEnableTrailAnimEvent += EnableToolTrail;
        ownerController.OnDisableTrailAnimEvent += DisableToolTrail;

        closestCollectable = ownerController.GetClosestCollectable();

        if (closestCollectable == null)
        {
            ownerController.SetState(States.Idle);
            return;
        }

        gatherAnimKey = closestCollectable.GetGatherAnimKey();
        usedToolParent = ownerController.GetToolTransform(gatherAnimKey);
        usedToolSound = ownerController.GetToolSound(gatherAnimKey);

        if (usedToolParent.GetChild(usedToolParent.childCount - 2).GetComponent<TrailRenderer>())
        {
            toolTrail = usedToolParent.GetChild(usedToolParent.childCount - 2);
        }

        scissorsAppearDisappearAnimation?.Complete();

        ScaleToolAndSetGatherAnim();
    }

    private void OwnerController_OnPlayerTriggerEnter(Collider obj)
    {
        if (obj.TryGetComponent<ICollectable>(out var collectable))
        {
            ownerController.AddCollectable(collectable);
            collectable.OnDevastation += OnDevastation;
        }
    }

    private void OwnerController_OnPlayerTriggerStay(Collider obj)
    {
        if (obj.TryGetComponent<IDamageable>(out var damageable))
        {
            if (damageable is not SoldierController)
            {
                IDamageable closestDamageable = ownerController.GetClosestDamagealble();
                if (closestDamageable != null)
                {
                    float distanceToEnemy = Vector3.Distance(ownerController.transform.position, closestDamageable.transform.position);
                    if (distanceToEnemy < attackDistance)
                    {
                        ownerController.SetSubState(States.Fight);
                    }
                }
            }
        }
    }

    private void OnDevastation(ICollectable sender)
    {
        ownerController.RemoveCollectable(sender);
        sender.OnDevastation -= OnDevastation;
    }

    private void OwnerController_OnPlayerTriggerExit(Collider obj)
    {
        if (obj.TryGetComponent<ICollectable>(out var collectable))
        {
            ownerController.RemoveCollectable(collectable);
        }
    }

    private void EnableToolTrail()
    {
        if (toolTrail)
        {
            toolTrail.gameObject.SetActive(true);
        }
    }

    private void DisableToolTrail()
    {
        if (toolTrail)
            toolTrail.gameObject.SetActive(false);
    }

    private void ScaleToolAndSetGatherAnim()
    {
        if (usedToolParent.localScale != Vector3.one)
        {
            scissorsAppearDisappearAnimation = usedToolParent.DOScale(1, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                if (isStateValid)
                {
                    SetGatherAnim();
                }
            });
        }
        else
        {
            SetGatherAnim();
        }
    }


    private void SetGatherAnim()
    {
        ownerController.OnGatherAnimEvent += OwnerController_OnGatherAnimEvent;
        animator.SetBool(gatherAnimKey, true);
    }

    private void OwnerController_OnGatherAnimEvent()
    {
        if (closestCollectable != null && !closestCollectable.IsDevastated())
        {
            HapticManager.Instance.PlayHaptics(HapticIntensity.Light);

            List<ICollectable> availableCollectables = new List<ICollectable>();

            availableCollectables.AddRange(ownerController.GetCollectablesInSightOfView());

            ownerController.RegisterInteraction();

            foreach (var collectable in availableCollectables)
            {
                collectable.Collect(ownerController);
            }
            if (ownerController.GetClosestCollectable() == null)
            {
                ownerController.SetState(States.Idle);
            }

            SoundManager.Instance.Play(usedToolSound);
        }
        else if (ownerController.GetClosestCollectable() != null)
        {
            closestCollectable = ownerController.GetClosestCollectable();
            if (gatherAnimKey != closestCollectable.GetGatherAnimKey())
            {
                ownerController.SetState(States.Idle);
                return;
            }
        }
        else
        {
            ownerController.SetState(States.Idle);
            return;
        }

        TryPlayHitParticle();
    }

    private bool TryPlayHitParticle()
    {
        bool isThereHitParticle = false;
        if (usedToolParent.GetLastChild().TryGetComponent<ParticleSystem>(out var hitParticle))
        {
            PoolableObject hitParticleinstance = PoolingSystem.Instance.GetParticlePool(gatherAnimKey).GetObject();
            hitParticleinstance.transform.SetPositionAndRotation(hitParticle.transform.position, hitParticle.transform.rotation);
            hitParticleinstance.gameObject.SetActive(true);
            isThereHitParticle = true;
        }
        return isThereHitParticle;
    }

    public override void Execute()
    {
        ListenForMoveCommand();
        LookToTheTarget();
    }

    private void ListenForMoveCommand()
    {
        if (Input.GetMouseButtonDown(0) && JoystickVisibilityPermitter.Instance.CanShowJoystick())
        {
            ownerController.SetState(States.Move);
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            ownerController.SetState(States.Move);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ownerController.SetState(States.Move);
        }
    }

    private void LookToTheTarget()
    {
        var target = CalculateTargetsMidpoint();
        if (target != null)
        {
            Vector3 lookDir = (target - characterTransform.position).normalized;
            float angleY = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
            Vector3 lookEulerAngles = new Vector3(characterTransform.eulerAngles.x, angleY, characterTransform.eulerAngles.z);
            characterTransform.rotation = Quaternion.Lerp(characterTransform.rotation, Quaternion.Euler(lookEulerAngles), 8 * Time.deltaTime);
        }
    }

    private Vector3 CalculateTargetsMidpoint()
    {
        Vector3 midpoint = Vector3.zero;

        List<ICollectable> targetsinView = ownerController.GetCollectablesInSightOfView();
        if (targetsinView.Count > 0)
        {
            for (int i = 0; i < targetsinView.Count; i++)
            {
                midpoint += targetsinView[i].GetPosition();
            }
            midpoint = midpoint / targetsinView.Count;
        }

        return midpoint;
    }

    public override void OnStateEnd()
    {
        isStateValid = false;

        ownerController.OnPlayerTriggerExit += OwnerController_OnPlayerTriggerExit;
        ownerController.OnPlayerTriggerEnter += OwnerController_OnPlayerTriggerEnter;

        ownerController.OnEnableTrailAnimEvent -= EnableToolTrail;
        ownerController.OnDisableTrailAnimEvent -= DisableToolTrail;

        ownerController.OnGatherAnimEvent -= OwnerController_OnGatherAnimEvent;

        animator.SetBool(gatherAnimKey, false);
        scissorsAppearDisappearAnimation = usedToolParent.DOScale(0, 0.3f).SetDelay(0.7f);
        DisableToolTrail();
    }
}