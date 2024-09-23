using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceUnit : PoolableObject
{
    public event Action<ResourceUnit> OnMotionEnd;

    [SerializeField] ResourceTypes resourceType;

    protected Vector3 destinationPosition = Vector3.zero;
    protected float jumpDuration;
    protected float jumpPower = 2;

    private Tween jumpTween;
    private Tween rotateTween;

    public void SetDestination(Vector3 destinationPositionParam) => destinationPosition = destinationPositionParam;
    public void SetJumpDuration(float jumpDurationParam) => jumpDuration = jumpDurationParam;
    public void SetJumpPower(float jumpPowerParam) => jumpPower = jumpPowerParam;
    public ResourceTypes GetResourceType() => resourceType;

    private void OnEnable()
    {
        if (destinationPosition == Vector3.zero)
            destinationPosition = PlayerController.Instance.GetInteractionTarget().GetPosition();

        transform.DOKill();

        jumpTween = transform.DOJump(destinationPosition, jumpPower, 1, jumpDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            FireOnMotionEnd(this);
        });
        rotateTween = transform.DORotate(transform.localEulerAngles + GetRandomEulerAngles(), jumpDuration + 0.01f, RotateMode.FastBeyond360).SetEase(Ease.Linear).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void FireOnMotionEnd(ResourceUnit resourceUnit) => OnMotionEnd?.Invoke(resourceUnit);

    public override void OnDisable()
    {
        base.OnDisable();
        OnMotionEnd = null;
    }

    private Vector3 GetRandomEulerAngles()
    {
        float randomAngleX = Random.Range(100, 300);
        float randomAngleY = Random.Range(100, 300);
        float randomAngleZ = Random.Range(100, 300);

        Vector3 randomEulerAngles = new Vector3(randomAngleX, randomAngleY, randomAngleZ);
        return randomEulerAngles;
    }
}
