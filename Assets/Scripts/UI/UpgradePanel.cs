using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    public event Action OnOpen;
    public event Action OnClose;

    [SerializeField] Transform settingsPanelParent;
    [SerializeField] Transform settingsPanel;
    [SerializeField] CanvasGroup settingsPanelGroup;
    [SerializeField] CanvasGroup settingsPanelParentGroup;

    private Tween shakeTween;
    private Tween fadeTween;

    private Vector3 initialScale;

    private IEnumerator Start()
    {
        yield return null;
        settingsPanelParent.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);
        settingsPanelParent.GetComponent<CanvasGroup>().alpha = 1;

        initialScale = settingsPanel.localScale;
    }

    public void OpenSettingsPanel()
    {
        settingsPanelParent.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(true);
        OnOpen?.Invoke();
        settingsPanel.localScale = initialScale;
        shakeTween = settingsPanel.DOScale(initialScale.x + 0.05f, 0.075f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            settingsPanel.DOScale(initialScale.x - 0.05f, 0.125f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                settingsPanel.DOScale(initialScale, 0.05f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                });
            });
        });

        settingsPanelParentGroup.alpha = 1f;

        float groupAlpha = 0;
        fadeTween = DOTween.To(() => groupAlpha, x => groupAlpha = x, 1, 0.4f).SetEase(Ease.Linear).OnUpdate(() =>
        {
            settingsPanelGroup.alpha = groupAlpha;
        });

        SoundManager.Instance.Play(SoundTypes.Button);
    }

    public void CloseUpgradePanel()
    {
        OnClose?.Invoke();

        shakeTween?.Kill();
        fadeTween?.Kill();
        float groupAlpha = 1;
        fadeTween = DOTween.To(() => groupAlpha, x => groupAlpha = x, 0, 0.3f).OnUpdate(() =>
        {
            settingsPanelParentGroup.alpha = groupAlpha;
            settingsPanelGroup.alpha = groupAlpha;
        }).OnComplete(() =>
        {
            settingsPanelParent.gameObject.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
        });

        SoundManager.Instance.Play(SoundTypes.Button);
    }
}
