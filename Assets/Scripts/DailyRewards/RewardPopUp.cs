using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopUp : MonoBehaviour
{
    [SerializeField] private Transform rewardImageParent;
    [SerializeField] private ParticleSystem popUpEffect;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextMeshProUGUI rewardAmount;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        CloseDailyrewardPopUp();
    }

    public void MakePrewardPopUp(List<SerializablePair<DailyRewardTypes, int>> rewards)
    {
        canvasGroup.alpha = 1;

        rewardImage.sprite = DailyRewardManager.Instance.GetRewardSpriteForPopUp(rewards[0].item1);
        rewardAmount.text = $"{rewards[0].item2}";

        popUpEffect.Play();

        rewardImageParent.transform.localScale = Vector3.one * 0.4f;
        rewardImageParent.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.Linear);
    }

    public void CloseDailyrewardPopUp()
    {
        canvasGroup.alpha = 0;
    }
}
