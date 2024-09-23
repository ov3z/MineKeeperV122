using System.Collections;
using TMPro;
using UnityEngine;

public class CaveLevelUI : MonoBehaviour
{
    public static CaveLevelUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private Transform countdown;

    private Coroutine countdownCoroutine = null;

    private void Awake()
    {
        Instance = this;
    }

    public void SetLevelText(int level)
    {
        levelText.text = $"Level {level}";
    }

    public void UpdateCountDown(int timeRemainig)
    {
        countDownText.text = $"{timeRemainig}";
    }

    public void DisableCountdown()
    {
        if (countdownCoroutine != null)
        {
            countdown.gameObject.SetActive(false);
            StopCoroutine(countdownCoroutine);
        }
    }

    public void EnableCountdown()
    {
        if (countdownCoroutine == null)
        {
            countdown.gameObject.SetActive(true);
            countdownCoroutine = StartCoroutine(CountdownRoutine());
        }
    }

    private IEnumerator CountdownRoutine()
    {
        for (int i = 5; i >= 0; i--)
        {
            if (i == 0)
            {
                CaveGameManager.Instance.ShowWinPanel();
                CaveGameManager.Instance.FireOnPlayerLose();
                countdown.gameObject.SetActive(false);
                yield break;
            }
            yield return new WaitForSeconds(1);
            UpdateCountDown(i);
        }
    }
}
