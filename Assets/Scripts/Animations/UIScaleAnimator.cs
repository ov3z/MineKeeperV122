using UnityEngine;
using DG.Tweening;
public class UIScaleAnimator : MonoBehaviour
{
    private Vector3 initialLocalScale;

    private void Start()
    {
        initialLocalScale = transform.localScale;
        ScaleAnim();
    }

    private void ScaleAnim()
    {
        transform.DOScale(initialLocalScale * 0.9f, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.DOScale(initialLocalScale * 1.1f, 0.6f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.DOScale(initialLocalScale, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    ScaleAnim();
                });
            });
        });

    }

}
