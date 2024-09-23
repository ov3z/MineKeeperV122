using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class MoveState : StateBase
{
    private const string MOVE_ANIM_KEY = "Move";
    private const string MOVE_ANIM_FLOAT = "Speed";

    private Transform ownerTransform;
    private Animator animator => ownerController.Animator;
    private CharacterStats characterStats;
    private Rigidbody rigidbody;
    private float interactionTimer;
    private float attackDistance;
    private float speed;

    private JoystickUI joystickUI => JoystickUI.Instance;
    private bool isJoystickUsed => Input.GetMouseButton(0);

    public MoveState(PlayerController playerController) : base(playerController)
    {
        ownerTransform = ownerController.transform;
        rigidbody = ownerController.Rigidbody;
        characterStats = ownerController.Stats;
        attackDistance = ownerController.AttackRange;
    }

    public override void OnStateStart()
    {
        ownerController.OnPlayerTriggerEnter += OwnerController_OnPlayerTriggerEnter;
        ownerController.OnPlayerTriggerStay += OwnerController_OnPlayerTriggerStay;
        ownerController.OnPlayerTriggerExit += OwnerController_OnPlayerTriggerExit;
        animator.SetBool(MOVE_ANIM_KEY, true);
    }

    private void OwnerController_OnPlayerTriggerStay(Collider obj)
    {
        ChechForEnemies();
        ChechForInteractables();
    }

    private void ChechForInteractables()
    {
        IInteractable activeInteractable = ownerController.GetInteractionTarget();
        if (activeInteractable != null && activeInteractable.CanInteractInMotion())
        {
            interactionTimer -= Time.deltaTime;
            if (interactionTimer <= 0)
            {
                interactionTimer = activeInteractable.GetInteractionTimerMax();
                activeInteractable.Interact(ownerTransform);
                ownerController.RegisterInteraction(activeInteractable);
            }
        }
    }

    private void ChechForEnemies()
    {
        IDamageable closestDamageable = ownerController.GetClosestDamagealble();
        if (closestDamageable != null)
        {
            float distanceToEnemy = Vector3.Distance(ownerController.transform.position, closestDamageable.transform.position);
            if (distanceToEnemy < attackDistance)
            {
                if (closestDamageable is not SoldierController)
                {
                    ownerController.SetSubState(States.Fight);
                }
            }
        }
    }

    private void OwnerController_OnPlayerTriggerEnter(Collider obj)
    {
        if (obj.TryGetComponent<ICollectable>(out var collectable))
        {
            ownerController.AddCollectable(collectable);
            collectable.OnDevastation += OnDevastation;
        }
        else if (obj.TryGetComponent<IInteractable>(out var interactable))
        {
            ownerController.SetInteractionTarget(interactable);
        }
        else if (obj.TryGetComponent<IDamageable>(out var damageable))
        {
            ownerController.AddAttackTarget(damageable);
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
        else if (obj.TryGetComponent<IInteractable>(out var interactable))
        {
            ownerController.ResetInteractionTarget();
        }
        else if (obj.TryGetComponent<IDamageable>(out var damageable))
        {
            ownerController.RemoveAttackTarget(damageable);
        }
    }

    public override void Execute()
    {
        ListenForIdleCommand();
    }

    public override void FixedExecute()
    {
        var velocity = Vector3.zero;

        if (!PlayerController.Instance.IsLying)
        {
            if (isJoystickUsed)
            {
                velocity = GetJoystickBindingVelocity();
            }
            else
            {
                velocity = GetWASDBinding();
            }
            SyncVelocityAndAnimation(velocity);
        }
    }

    private void SyncVelocityAndAnimation(Vector3 velocity)
    {
        Vector3 inheritedVelocity = new Vector3(velocity.x, rigidbody.velocity.y, velocity.z);
        rigidbody.velocity = inheritedVelocity;
        float moveSpeedNormalized = rigidbody.velocity.magnitude / characterStats.speed;
        animator.SetFloat(MOVE_ANIM_FLOAT, moveSpeedNormalized);

        if (ownerController.IsDoingAnyJob) return;

        float angleY = Mathf.Atan2(rigidbody.velocity.x, rigidbody.velocity.z) * Mathf.Rad2Deg;
        Vector3 lookEulerRotation = new Vector3(ownerTransform.eulerAngles.x, angleY, ownerTransform.eulerAngles.z);
        ownerTransform.rotation = Quaternion.Lerp(ownerTransform.rotation, Quaternion.Euler(lookEulerRotation), 8 * Time.deltaTime);

        ownerController.FireOnNormalizedSpeedChange(inheritedVelocity.magnitude / 12);
    }

    private void ListenForIdleCommand()
    {
        if (Input.GetMouseButtonUp(0) || (CameraFocusManager.Instance != null && CameraFocusManager.Instance.IsBlendingBetweenCameras))
        {
            ownerController.SetState(States.Idle);
        }
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0 && !Input.GetMouseButton(0))
        {
            ownerController.SetState(States.Idle);
        }
    }

    public override void OnStateEnd()
    {
        ownerController.OnPlayerTriggerEnter -= OwnerController_OnPlayerTriggerEnter;
        ownerController.OnPlayerTriggerExit -= OwnerController_OnPlayerTriggerExit;

        rigidbody.velocity = Vector3.zero;

        animator.SetBool(MOVE_ANIM_KEY, false);
        animator.SetFloat(MOVE_ANIM_FLOAT, 0f);
        ownerController.FireOnNormalizedSpeedChange(0);
    }
    private Vector3 GetJoystickBindingVelocity()
    {
        Vector3 jostickRelativeDisplacement = joystickUI.GetJoystickDisplacementDirectionWorld() * joystickUI.GetJoystickDisplacementNormalized();
        Vector3 velocity = jostickRelativeDisplacement * characterStats.speed;
        return velocity;
    }
    private Vector3 GetWASDBinding()
    {
        var direction = Vector3.zero;

        direction.x = Input.GetAxis("Horizontal");
        direction.z = Input.GetAxis("Vertical");

        direction.Normalize();

        var velocity = direction * characterStats.speed;

        return velocity;
    }
}
