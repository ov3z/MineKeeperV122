using UnityEngine;

public class SoldierMineState : SoldierState
{
    private Animator animator;
    private AnimationEventHandler animationEventHandler;
    private ICollectable targetOre;
    private Transform tool;
    private float maxDistanceToPlayer;
    private Transform ownerTransform;
    private Transform playerTransform;

    private const string MINE_ANIM_KEY = "Mine";

    public override bool NeedFollowPlayer => true;

    public SoldierMineState(SoldierController soldierController) : base(soldierController)
    {
        animator = ownerController.Animator;
        animationEventHandler = ownerController.AnimationEventHandler;
        tool = ownerController.Tool;
        maxDistanceToPlayer = ownerController.MaxDistanceToThePlayer;

        canGoToMine = false;
        canChaseEnemies = true;
    }

    public override void OnStateStart()
    {
        animator.SetBool(MINE_ANIM_KEY, true);
        animationEventHandler.OnGatherAnimationEvent += Mine;
        targetOre = ownerController.GetClosestOre();

        ownerTransform = ownerController.transform;
        playerTransform = PlayerController.Instance.transform;

        if (targetOre.IsDevastated())
        {
            SearchForNewOre();
        }
    }

    private void SearchForNewOre()
    {
        ICollectable closestCollectable = ownerController.GetClosestOre();
        if (closestCollectable != null)
        {
            float distanceToOre = Vector3.Distance(closestCollectable.GetPosition(), playerTransform.position);
            if (distanceToOre < maxDistanceToPlayer)
            {
                ownerController.SwitchState(SoldierStates.GoToOre);
                return;
            }
        }
        ownerController.SwitchState(SoldierStates.FollowPlayer);
    }

    private void Mine()
    {
        targetOre.Collect(ownerController);
        ShowMineParticle();

        if(ownerController.Renderer.isVisible)
        {
            SoundManager.Instance.Play(SoundTypes.StoneMine);
        }

        if (targetOre.IsDevastated())
        {
            SearchForNewOre();
        }
    }

    private void ShowMineParticle()
    {
        if (tool.GetLastChild().TryGetComponent<ParticleSystem>(out var hitParticle))
        {
            PoolableObject hitParticleInstance = PoolingSystem.Instance.GetParticlePool(MINE_ANIM_KEY).GetObject();
            hitParticleInstance.transform.SetPositionAndRotation(hitParticle.transform.position, hitParticle.transform.rotation);
            hitParticleInstance.gameObject.SetActive(true);
        }
    }

    public override void Execute()
    {
        Vector3 directionToTarget = (targetOre.transform.position - ownerController.transform.position).normalized;
        float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        Vector3 eulerAngles = new Vector3(ownerController.transform.eulerAngles.x, angle, ownerController.transform.eulerAngles.z);

        ownerController.transform.rotation = Quaternion.Lerp(ownerController.transform.rotation, Quaternion.Euler(eulerAngles), 8 * Time.deltaTime);
    }

    public override void OnStateEnd()
    {
        animator.SetBool(MINE_ANIM_KEY, false);
        animationEventHandler.OnGatherAnimationEvent -= Mine;
    }
}