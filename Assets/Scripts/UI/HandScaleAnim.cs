using UnityEngine;
using DG.Tweening;

public class HandScaleAnim : MonoBehaviour
{

    private void OnEnable()
    {
        transform.DOScale(1.1f, 0.4f).OnComplete(() =>
        {
            StartScaleAnim();
        });
    }

    private void StartScaleAnim()
    {
        transform.DOScale(0.9f, 0.8f).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.DOScale(1.1f, 0.8f).SetEase(Ease.Linear).OnComplete(() =>
            {
                StartScaleAnim();
            });

        });
    }

    private void OnDisable()
    {
        transform.DOKill();
    }
}
