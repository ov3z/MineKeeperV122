using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelManager : MonoBehaviour
{
    public static PlayerLevelManager Instance { get; private set; }

    public event Action<int> OnLevelUp;

    [SerializeField] private Image levelBarFill;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI levelProgressText;
    [SerializeField] private TextMeshProUGUI levelPanelLevelText;
    [SerializeField] private TextMeshProUGUI xpIncrement;
    [SerializeField] private List<SerializableList<PayoutValidation>> unlockOnLevelList;
    [SerializeField] private UpgradePanel levelUpPanel;

    private int level;
    private int currentXp;
    private int maxXpForThisLevel => 35 + 25 * (level - 1); // 5 was 25 , and 2 was 35
    private bool wasLevelUpped;
    private bool didRecieveReward;

    public int Level => level;

    /// <summary>
    /// the one needed for GameStart
    /// </summary>
    private void Awake()
    {
        Instance = this;
        level = PlayerPrefs.GetInt("Level", 1);
        currentXp = PlayerPrefs.GetInt("CurrentXp", 0);
        wasLevelUpped = PlayerPrefs.GetInt("WasLevelUpped", 0) == 1 ? true : false;
        didRecieveReward = PlayerPrefs.GetInt("DidRecieveReward", 0) == 1 ? true : false;

        UpdateLevelUI();
    }

    private IEnumerator Start()
    {
        if (wasLevelUpped && !GameManager.Instance.IsInCave)
        {
            wasLevelUpped = false;
            PlayerPrefs.SetInt("WasLevelUpped", 0);
            OnLevelUp?.Invoke(Level);
            QuestEvents.FireOnEarnXPForLevel(1);

            yield return new WaitForSeconds(1f);

            levelUpPanel.OpenSettingsPanel();
            levelUpPanel.OnOpen += UpdateLevelUpPanelText;
            UpdateLevelUpPanelText();
            levelUpPanel.OnClose += UnlockRewardsViaPanel;

            PlayerPrefs.SetInt("DidRecieveReward", 1);
        }
        else if (didRecieveReward)
        {
            UnlockRewards();
        }
    }

    private void UnlockRewardsViaPanel()
    {
        levelUpPanel.OnClose -= UnlockRewardsViaPanel;
        UnlockRewards();
    }

    private void UnlockRewards()
    {
        PlayerPrefs.SetInt("DidRecieveReward", 0);

        if (level >= 2)
            foreach (var building in unlockOnLevelList[level - 2].items)
            {
                building.OnPayout();
            }
    }

    private void UpdateLevelUI()
    {
        UpdateLevel();
        UpdateProgressFill();
        UpdateProgressText();
    }

    private void UpdateLevel()
    {
        level = PlayerPrefs.GetInt("Level", 1);

        if (GameManager.Instance.IsInCave)
        {
            levelText.text = $"Level\n{level}";
        }
        else
        {
            levelText.text = $"Level {level}";
        }
    }

    private void UpdateProgressText()
    {
        if (GameManager.Instance.IsInCave)
            levelProgressText.text = $"{currentXp}/{maxXpForThisLevel}";
    }

    private void UpdateProgressFill()
    {
        levelBarFill.fillAmount = (float)currentXp / maxXpForThisLevel;
    }

    private void UpdateProgressFill(int progressIncrement)
    {
        float progress = (float)(currentXp - progressIncrement) / maxXpForThisLevel;
        DOTween.To(() => progress, x => progress = x, (float)(currentXp) / maxXpForThisLevel, 0.5f).SetEase(Ease.Linear)
            .SetDelay(1f).OnUpdate(() => { levelBarFill.fillAmount = progress; });
    }

    public void AddXP(int xp)
    {
        currentXp += xp;

        if (GameManager.Instance.IsInCave)
            xpIncrement.text = $"{xp}";

        if (currentXp >= maxXpForThisLevel)
        {
           // YsoCorp.GameUtils.YCManager.instance.OnGameFinished(true); // yso corp finish level
            Debug.Log($"Level Has finished: {currentXp >= maxXpForThisLevel}");
            
            currentXp -= maxXpForThisLevel;
            wasLevelUpped = true;
            level++;

            if (GameManager.Instance.IsInCave)
            {
                levelBarFill.fillAmount = 1;
                levelProgressText.text = "LEVEL UP!";
            }
            else
            {
                levelBarFill.fillAmount = currentXp / maxXpForThisLevel;
                levelUpPanel.OpenSettingsPanel();
                levelUpPanel.OnOpen += UpdateLevelUpPanelText;
                UpdateLevelUpPanelText();
                levelUpPanel.OnClose += UnlockRewardsViaPanel;

                SoundManager.Instance.Play(SoundTypes.LevelUp);

                UpdateProgressFill(xp);
                UpdateProgressText();

                OnLevelUp?.Invoke(level);
                
              //  YsoCorp.GameUtils.YCManager.instance.OnGameStarted(Level);//yso corp
                Debug.Log($"New level started: {Level} , and the currentXP after starting new level is: {currentXp}");
                
            }
        }
        else
        {
            //YsoCorp.GameUtils.YCManager.instance.OnGameFinished(false); // yso corp finish level
            UpdateProgressFill(xp);
            UpdateProgressText();
        }

        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("CurrentXp", currentXp);
        PlayerPrefs.SetInt("WasLevelUpped", wasLevelUpped ? 1 : 0);

        UpdateLevel();
    }

    private void UpdateLevelUpPanelText()
    {
        levelUpPanel.OnOpen -= UpdateLevelUpPanelText;
        levelPanelLevelText.text = $"{level}";
    }
}

[System.Serializable]
public class SerializableList<T>
{
    public List<T> items = new List<T>();
}

[System.Serializable]
public class SerializablePair<T1, T2>
{
    public T1 item1;
    public T2 item2;

    public void RegisterOnMap(ref Dictionary<T1, T2> map)
    {
        map.Add(item1, item2);
    }
}