using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToMineScaleAnim : MonoBehaviour
{
    [SerializeField] private float minScale = 1.15f;
    [SerializeField] private float maxScale = 1.4f;
    [SerializeField] private float animationhalfPeriod = 0.3f;

    void Start()
    {
        AnimateScale();
    }

    private void AnimateScale()
    {
        transform.DOScale(maxScale, animationhalfPeriod).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.DOScale(minScale, animationhalfPeriod).SetEase(Ease.Linear).OnComplete(() =>
            {
                AnimateScale();
            });
        });
    }
}
