using UnityEngine;

[CreateAssetMenu(fileName = "New Win Battles Quest", menuName = "QuestSystem/Quests/WinBattlesQuest")]
public class WinBatllesQuest : Quest
{
    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnWinBattle += OnWinBattle;

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        return QuestTargetSystem.Instance.GetGoToMineTarget();
    }

    private void OnWinBattle(int amount)
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
        QuestEvents.OnWinBattle -= OnWinBattle;
    }
}
