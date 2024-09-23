using UnityEngine;

[CreateAssetMenu(fileName = "New Follow Quest", menuName = "QuestSystem/Quests/FollowQuest")]
public class FollowQuest : Quest
{
    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnFollowCompletion += RegisterProgress;

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        Transform questTarget = (IntroductionQuestTargetSystem.Instance as IntroductionQuestTargetSystem).GetFollowRabbitTarget();

        return questTarget;
    }

    private void RegisterProgress()
    {
        _currentAmount++;

        Save();
        if (IsCompleted)
        {
            FireOnQuestComplete();
        }
    }

    public override void Dispose()
    {
        QuestEvents.OnFollowCompletion -= RegisterProgress;
    }
}
