using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Earn Quest", menuName = "QuestSystem/Quests/EarnQuest")]
public class EarnQuest : Quest
{
    [SerializeField] private ResourceTypes earningType;
    [SerializeField] private List<EarningType> earningMethod;

    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnResourceEarn += OnResourceEarn;

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        return QuestTargetSystem.Instance.GetEarningTarget(earningType, playerPosition, earningMethod);
    }

    private void OnResourceEarn(ResourceTypes type, float amount)
    {
        if (type == earningType)
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
    }
    public override void Dispose()
    {
        QuestEvents.OnResourceEarn -= OnResourceEarn;
    }
}
