using System.Runtime.InteropServices;
using UnityEngine;

public class IdleState : StateBase
{
    private const string IDLE_ANIM_KEY = "Idle";

    private Animator animator=> ownerController.Animator;
    private Transform chacterTransform;

    private float interactionTimer;
    private float attackDistance;

    public IdleState(PlayerController playerController) : base(playerController)
    {
        chacterTransform = ownerController.transform;
        attackDistance = ownerController.AttackRange;
    }

    public override void OnStateStart()
    {
        ownerController.OnPlayerTriggerEnter += OwnerController_OnPlayerTriggerEnter;
        ownerController.OnPlayerTriggerStay += OwnerController_OnPlayerTriggerStay;
        ownerController.OnPlayerTriggerExit += OwnerController_OnPlayerTriggerExit;
        animator.SetBool(IDLE_ANIM_KEY, true);
    }

    private void OwnerController_OnPlayerTriggerEnter(Collider obj)
    {
        if (obj.TryGetComponent<ICollectable>(out var collectable))
        {
            ownerController.AddCollectable(collectable);
            ownerController.SetState(States.Gather);

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

    private void OwnerController_OnPlayerTriggerStay(Collider obj)
    {
        IInteractable activeInteractable = ownerController.GetInteractionTarget();
        if (activeInteractable != null)
        {
            interactionTimer -= Time.deltaTime;
            if (interactionTimer <= 0)
            {
                interactionTimer = activeInteractable.GetInteractionTimerMax();
                activeInteractable.Interact(chacterTransform);
                ownerController.RegisterInteraction(activeInteractable);
            }
        }

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
        ListenForMoveCommand();
        LookToTheEnemyIfThereIsAny();
        CheckIsThereAvailableCollectibles();
    }

    private void ListenForMoveCommand()
    {
        if (!PlayerController.Instance.IsLying)
        {
            if (Input.GetMouseButtonDown(0) && JoystickVisibilityPermitter.Instance.CanShowJoystick())
            {
                ownerController.SetState(States.Move);
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                ownerController.SetState(States.Move);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ownerController.SetState(States.Move);
            }
        }
    }

    private void CheckIsThereAvailableCollectibles()
    {
        if (ownerController.GetClosestCollectable() != null && ownerController.GetClosestDamagealble() == null)
            ownerController.SetState(States.Gather);
    }

    private void LookToTheEnemyIfThereIsAny()
    {
        IDamageable enemy = ownerController.GetClosestDamagealble();
        if (enemy != null)
        {
            Vector3 dirToEnemy = (enemy.transform.position - ownerController.transform.position).normalized;
            float angle = Mathf.Atan2(dirToEnemy.x, dirToEnemy.z) * Mathf.Rad2Deg;
            Vector3 eulerAngles = new Vector3(ownerController.transform.eulerAngles.x, angle, ownerController.transform.eulerAngles.z);
            ownerController.transform.eulerAngles = eulerAngles;
        }
    }

    public override void OnStateEnd()
    {
        ownerController.OnPlayerTriggerEnter -= OwnerController_OnPlayerTriggerEnter;
        ownerController.OnPlayerTriggerStay -= OwnerController_OnPlayerTriggerStay;
        ownerController.OnPlayerTriggerExit -= OwnerController_OnPlayerTriggerExit;
        animator.SetBool(IDLE_ANIM_KEY, false);
    }
}
