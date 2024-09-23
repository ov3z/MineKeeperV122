using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PickUnitUI : PoolableObject
{
    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private Image icon;
    [SerializeField] private CanvasGroup canvasGroup;

    public void SetUnit(float income, ResourceTypes type)
    {
        incomeText.text = $"+{income}";
        icon.sprite = ResourceSpriteStorage.Instance.GetIcon(type);
    }
    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        transform.localPosition = Vector3.zero;
        canvasGroup.alpha = 1f;
        transform.DOScale(1.1f, 0.2f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            transform.DOScale(1, 0.05f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                canvasGroup.DOFade(0, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            });
        });
    }
}
