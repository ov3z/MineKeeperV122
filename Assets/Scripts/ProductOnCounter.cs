using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductOnCounter : MonoBehaviour
{
    public bool IsEnabled { get; private set; }

    private void Awake()
    {
        transform.localScale = Vector3.zero;
        IsEnabled = false;
    }

    public void Enable()
    {
        IsEnabled = true;
        transform.DOKill();
        transform.DOScale(1.1f, 0.25f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            transform.DOScale(1f, 0.02f).SetEase(Ease.OutCubic);
        });
    }

    public void Disable()
    {
        IsEnabled = false;
        transform.DOKill();
        transform.DOScale(1.1f, 0.02f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            transform.DOScale(0f, 0.25f).SetEase(Ease.OutCubic);
        });
    }
}
