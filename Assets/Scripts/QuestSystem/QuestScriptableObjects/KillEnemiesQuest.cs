using UnityEngine;

[CreateAssetMenu(fileName = "New Kill Enemies Quest", menuName = "QuestSystem/Quests/KillEnemiesQuest")]
public class KillEnemiesQuest : Quest
{
    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnEnemyKill += RegisterProgress;

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        Vector3 playerPositon = PlayerController.Instance.transform.position;
        Transform questTarget = (IntroductionQuestTargetSystem.Instance as IntroductionQuestTargetSystem).GetClosestEnemy(playerPositon);
        return questTarget;
    }

    private void RegisterProgress(int killedEnemyAmount)
    {
        _currentAmount += killedEnemyAmount;

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
        QuestEvents.OnEnemyKill -= RegisterProgress;
    }
}
