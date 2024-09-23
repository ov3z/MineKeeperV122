using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform blurPanel;
    [SerializeField] private RectTransform noRaycastPanel;
    [SerializeField] private List<DialogueUnit> dialogueElements;
    [SerializeField] private Image[] playerIcons;
    [SerializeField] private Sprite girlMinerForDialogue;

    public void EnableBlurPanel()
    {
        StartCoroutine(BlurPanelEnableRutine());

        if (PlayerPrefs.GetInt("Gender", 0) == 1)
        {
            foreach (var item in playerIcons)
            {
                item.sprite = girlMinerForDialogue;
            }
        }

        foreach (var element in dialogueElements)
        {
            element.Initialize();
        }
    }

    public void ShowDialogueElement()
    {
        if (dialogueElements.Count == 0)
        {
            StartCoroutine(MakePlayergetUp());
            blurPanel.gameObject.SetActive(false);
            canvasGroup.alpha = 1f;
            return;
        }

        dialogueElements[0].ShowDialogueElement();
        dialogueElements.RemoveAt(0);

        SoundManager.Instance.Play(SoundTypes.PopUp);
    }

    private IEnumerator BlurPanelEnableRutine()
    {
        canvasGroup.alpha = 0f;
        yield return new WaitForSeconds(0.5f);
        noRaycastPanel.gameObject.SetActive(true);
        blurPanel.gameObject.SetActive(true);
        canvasGroup.alpha = 0.01f;

        ShowDialogueElement();
    }

    private IEnumerator MakePlayergetUp()
    {
        /*Debug.Log("First level start");*/
        PlayerController.Instance.GetUp();
        yield return new WaitForSeconds(1f);
        CameraFocusManager.Instance.FocusCamOnQuestTarget();
        gameObject.SetActive(false);

        /*YsoCorp.GameUtils.YCManager.instance.OnGameStarted(PlayerLevelManager.Instance.Level);//yso sdk start
        Debug.Log($"Level in playerPrefs and yso sdk: {PlayerLevelManager.Instance.Level}");*/
    }

    [System.Serializable]
    public class DialogueUnit
    {
        [SerializeField] private RectTransform DialogueIcon;
        [SerializeField] private RectTransform DialogueText;
        [SerializeField] private TextMeshProUGUI Text;

        public bool needToShowKeyBoard = false;

        public void Initialize()
        {
            DialogueIcon.localScale = Vector3.zero;
            DialogueText.localScale = Vector3.zero;
        }

        private void UpdateTextContext()
        {
            if (Text.text.Contains("Dudu"))
            {
                string playerName = PlayerPrefs.GetString("UserName", "Dudu");
                Text.text = Text.text.Replace("Dudu", playerName);
                PlayPopUpAnimation();
            }
        }

        public void ShowDialogueElement()
        {
#if UNITY_WEBGL || PLATFORM_WEBGL
            UpdateTextContext();
            PlayPopUpAnimation();
#else
            if (needToShowKeyBoard)
            {
                UserNameEnterHandler.Instance.EneableInputField();
                UserNameEnterHandler.Instance.OnNameSubmit += UpdateTextContext;
            }
            else
            {
                UpdateTextContext();
                PlayPopUpAnimation();
            }
#endif
        }

        private void PlayPopUpAnimation()
        {
            var sequence = DOTween.Sequence();
            sequence.Pause();
            sequence.Append(DialogueIcon.DOScale(1.05f, 0.3f)).SetEase(Ease.Linear).OnComplete(() =>
            {
                DialogueIcon.DOScale(1f, 0.05f).SetEase(Ease.Linear);
            });
            sequence.Join(DialogueText.DOScale(1.3f, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                DialogueText.DOScale(1f, 0.1f).SetEase(Ease.Linear);
            }));
            sequence.Play();
        }
    }
}
