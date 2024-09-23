using UnityEngine;
using DG.Tweening;

public class VerticalFloatAnimation : MonoBehaviour
{
    private RectTransform rect;

    [SerializeField] private float posYMax = 225;
    [SerializeField] private float posYMin = 185;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        rect.DOAnchorPosY(posYMax, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            AnimationChain();
        });
    }

    private void AnimationChain()
    {
        rect.DOAnchorPosY(posYMin, 1f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            rect.DOAnchorPosY(posYMax, 1f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                AnimationChain();
            });
        });
    }
}
