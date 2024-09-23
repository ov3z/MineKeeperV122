using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIController : MonoBehaviour
{
    [SerializeField] RectTransform questUI;

    [SerializeField] RectTransform questUIPortrait;
    [SerializeField] RectTransform questUILandscape;

    [SerializeField] Image icon;
    [SerializeField] Image progressFill;
    [SerializeField] Image questBG;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI rewardAmountText;
    [SerializeField] Transform coinResourceTransform;
    [SerializeField] RectTransform coinPrefab;
    [SerializeField] ParticleSystem completionParticle;
    [SerializeField] ParticleSystem rewardParticle;
    [SerializeField] List<RewardVisualUnit> visualUnits;

    [SerializeField] Sprite whiteBG;
    [SerializeField] Sprite completedBG;
    [SerializeField] Button questButton;

    [SerializeField] Transform handTutorial;
    [SerializeField] RectTransform questUiParent;
    [SerializeField] Transform completedPanel;

    private RectTransform rewardUI;
    private int rewardAmount;
    private Coroutine showCoinsCoroutine;
    private Coroutine showTutorialHandCoroutine;
    private bool canUpdateFill = true;
    private float questUIInitialAnchorPosY => Screen.width > Screen.height ? questUILandscape.anchoredPosition.y : questUIPortrait.anchoredPosition.y;
    private bool IsLandscape => Screen.width > Screen.height ? true : false;

    private Dictionary<ResourceTypes, RewardVisualUnit> visualUnitsMap = new();

    private Tween showRewardUITween;

    private Sequence showUISequence;

    private void Start()
    {
        questButton.onClick.RemoveAllListeners();
        questButton.onClick.AddListener(CameraFocusManager.Instance.FocusCamOnQuestTarget);
        questButton.onClick.AddListener(PlayButtonSound);
        SubscribeCamFocusEvents();

        foreach (var item in visualUnits)
        {
            visualUnitsMap.Add(item.type, item);
        }
    }

    private void SubscribeCamFocusEvents()
    {
        CameraFocusManager.Instance.OnFocusChainRegister += DisableQuestButton;
        CameraFocusManager.Instance.OnFocusEnd += EnableQuestButton;
    }

    private void UnsubscribeCamFocusEvents()
    {
        CameraFocusManager.Instance.OnFocusChainRegister -= DisableQuestButton;
        CameraFocusManager.Instance.OnFocusEnd -= EnableQuestButton;
    }

    private void DisableQuestButton()
    {
        questButton.enabled = false;
    }

    private void EnableQuestButton()
    {
        questButton.enabled = true;
    }

    public void SetQuestIcon(Sprite iconImage)
    {
        try
        {
            icon.sprite = iconImage;
        }
        catch { }
    }

    public void SetQuestBackgrounf(Sprite bgImage)
    {
        try
        {
            questBG.sprite = bgImage;
        }
        catch { }
    }

    public void SetProgressFill(float fillAmount)
    {
        if (canUpdateFill)
        {
            try
            {
                progressFill.fillAmount = fillAmount;
            }
            catch { }
        }
    }
    public void SetProgressText(string progresstext)
    {
        try
        {
            progressText.text = progresstext;
        }
        catch { }
    }
    public void SetDescriptionText(string description)
    {
        try
        {
            descriptionText.text = description;
        }
        catch { }
    }

    public void SetRewardText(int amount)
    {
        try
        {
            rewardAmount = amount;
            rewardAmountText.text = $"{amount}";
        }
        catch { }
    }

    public void SwitcgBGToUncompletedState()
    {
        questBG.sprite = whiteBG;


        if (showTutorialHandCoroutine != null)
            StopCoroutine(showTutorialHandCoroutine);
    }

    public void SwitchBgToCompletedState()
    {
        questBG.sprite = completedBG;

        showTutorialHandCoroutine = StartCoroutine(ShowTutorialHandRoutine());
        questButton.onClick.RemoveAllListeners();

        if (!QuestController.Instance.IsThereAutoComplete)
        {
            questButton.onClick.AddListener(QuestController.Instance.StartNextQuestLoadingCoroutine);
            questButton.onClick.AddListener(PlayButtonSound);
            UnsubscribeCamFocusEvents();
            MoveQuestUIIfLandscape();

            completedPanel.gameObject.SetActive(true);

            EnableQuestButton();
        }
    }

    private void MoveQuestUIIfLandscape()
    {
        if (IsLandscape || true) // makes completed quest appear at the bottom
        {
            if (IsLandscape)
                questUiParent.DOAnchorPosY(-900, 1f);
            else
                questUiParent.DOAnchorPosY(-(Screen.height / 2 - 250) * 2, 1f);

        }
    }


    private IEnumerator ShowTutorialHandRoutine()
    {
        yield return new WaitForSeconds(1.1f);
        handTutorial.gameObject.SetActive(true);
    }

    public void DisableHandTutorial()
    {
        StopCoroutine(showTutorialHandCoroutine);
        handTutorial.gameObject.SetActive(false);
    }

    public void DisableQuestCompletionButton()
    {
        questButton.onClick.RemoveAllListeners();
        questButton.enabled = false;
    }

    public void HideQuestUI()
    {
        try
        {
            questButton.onClick.RemoveAllListeners();
            questButton.enabled = false;

            DisableHandTutorial();

            showUISequence?.Kill();
            showUISequence = DOTween.Sequence();
            showUISequence.Pause();

            showUISequence.Append(questUI.transform.DOScale(0, 0.3f).SetEase(Ease.Linear)).OnComplete(() =>
            {
                questUI.anchoredPosition = new Vector2(questUI.anchoredPosition.x, questUIInitialAnchorPosY - 250);
                questUiParent.anchoredPosition = Vector3.zero;

                TryDisableCompletedPanel();

                SetProgressFill(0);
            });

            showUISequence.Play();
        }
        catch { }
    }

    private void TryDisableCompletedPanel()
    {
        if (completedPanel != null)
            completedPanel.gameObject.SetActive(false);
    }

    public void ShowRewardUI(ResourceTypes type)
    {
        showRewardUITween.Complete();

        rewardUI = visualUnitsMap[type].rewardUI;

        showRewardUITween = rewardUI.transform.DOScale(1, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (showCoinsCoroutine != null)
                StopCoroutine(showCoinsCoroutine);

            showCoinsCoroutine = StartCoroutine(ShowRewardAndHidePanel(rewardAmount, type));
        });
    }

    private IEnumerator ShowRewardAndHidePanel(int rewardAmount, ResourceTypes type)
    {
        InstanciateCoins(rewardAmount, type);
        yield return new WaitForSeconds(0.2f);
        HideRewardUI();
    }

    private void InstanciateCoins(int amount, ResourceTypes type)
    {
        float delay = 0;
        for (int i = 0; i < amount; i++)
        {
            delay = 0.1f * i;
            var requiredPrefab = visualUnitsMap[type].rewardUnitPrefab;
            var target = visualUnitsMap[type].rewardTarget;
            RectTransform rewardVisual = Instantiate(requiredPrefab, requiredPrefab.parent);
            rewardVisual.gameObject.SetActive(true);
            rewardVisual.SetAsFirstSibling();
            rewardVisual.DOMove(target.position, 0.35f).SetEase(Ease.Linear).SetDelay(delay).OnComplete(() =>
            {
                RegisterRecievedReward(type);
                Destroy(rewardVisual.gameObject);
            });
        }
    }

    private void RegisterRecievedReward(ResourceTypes type)
    {
        if (type == ResourceTypes.XP)
        {
            PlayerLevelManager.Instance.AddXP(1);
        }
        else
        {
            ResourceStorage.Instance.ChangeResourceAmount(type, 1);
        }
    }

    public void HideRewardUI()
    {
        rewardUI.transform.localScale = Vector3.zero;
    }

    public void ShowQuestUI()
    {
        showUISequence?.Kill();
        showUISequence = DOTween.Sequence();
        showUISequence.Pause();

        showUISequence.Append(questUI.transform.DOScale(1.4f, 0.4f).SetEase(Ease.Linear));
        showUISequence.Append(questUI.transform.DOScale(1.15f, 0.15f).SetEase(Ease.Linear));
        showUISequence.Append(questUI.transform.DOScale(1.35f, 0.12f).SetEase(Ease.Linear));
        showUISequence.Append(questUI.transform.DOScale(1.2f, 0.12f).SetEase(Ease.Linear));
        showUISequence.Append(questUI.transform.DOScale(1.25f, 0.08f).SetEase(Ease.Linear));
        showUISequence.Append(questUI.transform.DOScale(1f, 0.5f).SetEase(Ease.Linear).SetDelay(0.8f));
        showUISequence.Join(questUI.DOAnchorPosY(questUIInitialAnchorPosY, 0.5f).SetEase(Ease.Linear));

        showUISequence.OnComplete(() =>
        {
            questButton.enabled = true;
            questButton.onClick.AddListener(CameraFocusManager.Instance.FocusCamOnQuestTarget);
            questButton.onClick.AddListener(PlayButtonSound);
            SubscribeCamFocusEvents();
        });

        showUISequence.Play();
    }

    private void PlayButtonSound()
    {
        SoundManager.Instance.Play(SoundTypes.Button);
    }

    public void ShowQuestCompletionEffect()
    {
        completionParticle.Play();
    }

    public void ShowRewardEffect()
    {
        rewardParticle.Play();
    }

    [Serializable]
    public class RewardVisualUnit
    {
        public ResourceTypes type;
        public RectTransform rewardUnitPrefab;
        public RectTransform rewardUI;
        public Transform rewardTarget;
    }
}