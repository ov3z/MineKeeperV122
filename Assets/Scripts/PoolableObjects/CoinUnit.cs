using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoinUnit : ResourceUnit
{
    private bool hasToRotateHorizontally = true;

    private void OnEnable()
    {
        if (destinationPosition == Vector3.zero)
            destinationPosition = PlayerController.Instance.GetInteractionTarget().GetPosition();

        transform.DOJump(destinationPosition, jumpPower, 1, jumpDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            FireOnMotionEnd(this);
        });
        transform.DORotate(transform.localEulerAngles + GetRandomEulerAngles(), jumpDuration + 0.01f, RotateMode.FastBeyond360).SetEase(Ease.Linear).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void SetHasToRotateHorizontally(bool hasToRotate) => this.hasToRotateHorizontally = hasToRotate;

    private Vector3 GetRandomEulerAngles()
    {
        float randomAngle;
        randomAngle = Random.Range(500, 700);
        Vector3 randomEulerAngles;
        if (hasToRotateHorizontally)
        {
            randomEulerAngles = Vector3.right * randomAngle;
        }
        else
        {
            randomEulerAngles = Vector3.up * randomAngle;
        }
        return randomEulerAngles;
    }
}
