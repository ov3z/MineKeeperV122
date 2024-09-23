using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Spend Quest", menuName = "QuestSystem/Quests/SpendQuest")]
public class SpendQuest : Quest
{
    [SerializeField] private List<ResourceTypes> spendingType = new List<ResourceTypes>();
    [SerializeField] private SpendingType spendingMethod;

    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnResourceSpend += OnResourceSpend;

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        return QuestTargetSystem.Instance.GetSpendingTarget(spendingMethod);
    }

    private void OnResourceSpend(ResourceTypes type, float amount)
    {
        if (spendingType.Contains(type))
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
        QuestEvents.OnResourceSpend -= OnResourceSpend;
    }
}
