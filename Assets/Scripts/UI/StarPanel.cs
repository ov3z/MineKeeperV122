using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarPanel : MonoBehaviour
{
    [SerializeField] private Image barFill;
    [SerializeField] private Transform[] stars;
    [SerializeField] private TextMeshProUGUI progressText;

    private int initialEnemyCount;
    private int currentEnemyCount;
    private int activeStars;

    private Tween barFillTween;

    private const string StarsKey = "Stars";

    private IEnumerator Start()
    {
        yield return null;
        initialEnemyCount = CaveGameManager.Instance.GetEnemyCount();
        currentEnemyCount = initialEnemyCount;
        CaveGameManager.Instance.OnAnyEnemyDeath += UpdateEnemyCount;
        UpdateTextAndFill();
        SaveStarCount();
    }

    private void UpdateTextAndFill()
    {
        progressText.text = $"{initialEnemyCount - currentEnemyCount}/{initialEnemyCount}";

        float barCurrentFill = barFill.fillAmount;
        float barTargetFill = (float)(initialEnemyCount - currentEnemyCount) / initialEnemyCount;

        barFillTween?.Kill();
        barFillTween = DOTween.To(() => barCurrentFill, x => barCurrentFill = x, barTargetFill, 0.3f).SetEase(Ease.Linear).OnUpdate(() =>
        {
            barFill.fillAmount = barCurrentFill;
        });
    }

    private void UpdateEnemyCount()
    {
        currentEnemyCount--;

        UpdateTextAndFill();
        float stageProgress = (float)(initialEnemyCount - currentEnemyCount) / initialEnemyCount;

        if (stageProgress > 0.33f)
        {
            ShowStar(0);
            if (stageProgress > 0.66f)
            {
                ShowStar(1);
                if (stageProgress > 0.99f)
                {
                    ShowStar(2);
                    CaveGameManager.Instance.FireOnLevelCleared();
                }
            }
        }
    }

    private void ShowStar(int index)
    {
        if (!stars[index].gameObject.activeSelf)
        {
            activeStars++;
            SaveStarCount();

            stars[index].gameObject.SetActive(true);
            stars[index].localScale = Vector3.zero;
            stars[index].DOScale(1.2f, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                stars[index].DOScale(1, 0.07f).SetEase(Ease.Linear);
            });
        }
    }

    private void SaveStarCount()
    {
        PlayerPrefs.SetInt(StarsKey, activeStars);
    }
}