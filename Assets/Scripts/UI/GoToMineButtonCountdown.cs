using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoToMineButtonCountdown : MonoBehaviour
{
    [SerializeField] private Button goTofightButton;
    [SerializeField] private Button exploreButton;
    [SerializeField] private Transform goToFightVisual;
    [SerializeField] private Transform countdownVisual;
    [SerializeField] private TextMeshProUGUI countdownText;

    private float caveEnterCooldown = 180; //was 180
    private float remainingTime;
    private DateTime closeTime;

    private void Awake()
    {
        exploreButton.onClick.AddListener(ResetRemainingTime);
        exploreButton.onClick.AddListener(SaveCloseTime);
    }

    private void OnEnable()
    {
        InitializeTimerData();
    }

    private void InitializeTimerData()
    {
        remainingTime = PlayerPrefs.GetInt("CaveCooldownRemaning", 0);

        closeTime = DateTime.Parse(PlayerPrefs.GetString("CloseTime", DateTime.Now.ToString()));

        var loggedOutTime = (float)(DateTime.Now - closeTime).TotalSeconds;

        remainingTime -= loggedOutTime;

        if (remainingTime <= 0)
        {
            countdownVisual.gameObject.SetActive(false);
            goTofightButton.enabled = true;
            goToFightVisual.gameObject.SetActive(true);
        }
        else
        {
            countdownVisual.gameObject.SetActive(true);
            goTofightButton.enabled = false;
            goToFightVisual.gameObject.SetActive(false);
            UpdareTimer();
            StartCoroutine(Countdown());
        }
    }

    private IEnumerator Countdown()
    {
        var time = 0f;
        while (remainingTime > 0)
        {
            time += Time.deltaTime;
            if (time >= 1)
            {
                remainingTime -= 1;
                time = 0;
                if (remainingTime <= 0)
                {
                    countdownVisual.gameObject.SetActive(false);
                    goTofightButton.enabled = true;
                    goToFightVisual.gameObject.SetActive(true);
                    SaveRemaningTime();
                }

                UpdareTimer();
                SaveRemaningTime();
            }
            yield return null;
        }
    }

    private void UpdareTimer()
    {
        var minutes = Mathf.FloorToInt(remainingTime / 60);
        var seconds = Mathf.FloorToInt(remainingTime % 60);

        var remainingTimeString = string.Format("{0:00}:{1:00}", minutes, seconds);
        countdownText.text = remainingTimeString;
    }

    private void OnDisable()
    {
        Disable();
        SaveCloseTime();
    }

    private void Disable()
    {
        countdownVisual.gameObject.SetActive(false);
        goTofightButton.enabled = false;
        goToFightVisual.gameObject.SetActive(false);
    }

    private void SaveRemaningTime()
    {
        PlayerPrefs.SetInt("CaveCooldownRemaning", (int)remainingTime);
    }

    public void SaveCloseTime()
    {
        PlayerPrefs.SetString("CloseTime", DateTime.Now.ToString());
    }

    public void ResetRemainingTime()
    {
        PlayerPrefs.SetInt("CaveCooldownRemaning", (int)caveEnterCooldown);

        InitializeTimerData();

        Debug.Log("Reset timer");
    }

    private void OnDestroy()
    {
        Disable();
    }
}
