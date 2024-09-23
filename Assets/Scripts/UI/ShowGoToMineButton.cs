using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowGoToMineButton : MonoBehaviour
{
    [SerializeField] private Transform goToMineButton;
    [SerializeField] private Transform goToMineButtonParent;
    [SerializeField] private CanvasGroup buttonCanvasGroup;
    [SerializeField] private GoToMineScaleAnim anim;

    private Tween shakeTween;
    private Tween fadeTween;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var playerController))
            ShowGoToMineUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var playerController))
            HideGoToMineUI();
    }

    private void ShowGoToMineUI()
    {
        goToMineButtonParent.GetChild(0).gameObject.SetActive(true);

        goToMineButton.localScale = Vector3.one;
        shakeTween = goToMineButton.DOScale(1.05f, 0.075f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            goToMineButton.DOScale(0.95f, 0.125f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                goToMineButton.DOScale(1f, 0.05f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    anim.enabled = true;
                });
            });
        });


        float groupAlpha = 0;
        fadeTween = DOTween.To(() => groupAlpha, x => groupAlpha = x, 1, 0.4f).SetEase(Ease.Linear).OnUpdate(() =>
        {
            buttonCanvasGroup.alpha = groupAlpha;
        });
    }

    public void HideGoToMineUI()
    {
        shakeTween?.Kill();
        fadeTween?.Kill();
        float groupAlpha = 1;
        fadeTween = DOTween.To(() => groupAlpha, x => groupAlpha = x, 0, 0.3f).OnUpdate(() =>
        {
            buttonCanvasGroup.alpha = groupAlpha;
        }).OnComplete(() =>
        {
            goToMineButtonParent.GetChild(0).gameObject.SetActive(false);
            anim.enabled = false;
        });
    }

    public void DisableGoToMineButtonOnGround()
    {
        gameObject.SetActive(false);
    }
}
