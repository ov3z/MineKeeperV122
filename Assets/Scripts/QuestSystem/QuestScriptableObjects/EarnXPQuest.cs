using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Earn XP Quest", menuName = "QuestSystem/Quests/EarnXPQuest")]
public class EarnXPQuest : Quest
{
    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnEarnXPForLevel += OnEarnXPForLevel;
        _currentAmount = PlayerLevelManager.Instance.Level;

        if (IsCompleted)
        {
            FireOnQuestComplete();
        }

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        return QuestTargetSystem.Instance.GetGoToMineTarget();
    }

    private void OnEarnXPForLevel(float amount)
    {
        _currentAmount += amount;

        float progress = Mathf.Clamp01(_currentAmount / _objectiveAmount);
        FireOnProgressUpdate(progress);

        Save();
        if (IsCompleted)
        {
            FireOnQuestComplete();
        }
    }

    public override void Dispose()
    {
        QuestEvents.OnEarnXPForLevel -= OnEarnXPForLevel;
    }
}