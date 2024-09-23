using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;
using YsoCorp.GameUtils;
#if UNITY_WEBGL || PLATFORM_WEBGL
using CrazyGames;
#endif

public class SettingsUI : MonoBehaviour
{
    [SerializeField] Transform settingsButton;
    [SerializeField] Transform settingsPanelParent;
    [SerializeField] Transform settingsPanel;
    [SerializeField] CanvasGroup settingsPanelGroup;
    [SerializeField] CanvasGroup settingsPanelParentGroup;
    [SerializeField] private TextMeshProUGUI ysoText;


    private Tween shakeTween;
    private Tween fadeTween;

    private IEnumerator Start()
    {
        ysoText.text = SettingManager.Instance.tVersion.text;
        yield return null;
        settingsPanelParent.gameObject.SetActive(false);
        settingsPanelParent.GetComponent<CanvasGroup>().alpha = 1;
    }

    public void OpenSettingsPanel()
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        CrazySDK.Instance.GameplayStop();
#endif

        settingsPanelParent.gameObject.SetActive(true);
        shakeTween = settingsPanel.DOScale(settingsPanel.localScale + 0.05f * Vector3.one, 0.075f).SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                settingsPanel.DOScale(settingsPanel.localScale - 0.1f * Vector3.one, 0.125f).SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        settingsPanel.DOScale(settingsPanel.localScale + 0.05f * Vector3.one, 0.05f)
                            .SetEase(Ease.OutQuad).OnComplete(() => { });
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

    public void CloseSettingsPanel()
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        CrazySDK.Instance.GameplayStop();
#endif

        shakeTween?.Kill();
        fadeTween?.Kill();
        float groupAlpha = 1;
        fadeTween = DOTween.To(() => groupAlpha, x => groupAlpha = x, 0, 0.3f)
            .OnUpdate(() => { settingsPanelParentGroup.alpha = groupAlpha; }).OnComplete(() =>
            {
                settingsPanelParent.gameObject.SetActive(false);
            });

        SoundManager.Instance.Play(SoundTypes.Button);
    }
}