using UnityEngine;

[CreateAssetMenu(fileName = "New Player Upgrade Quest", menuName = "QuestSystem/Quests/PlayerUpgradeQuest")]
public class UpgradePlayerStatsQuest : Quest
{
    [SerializeField] private UpgradeButtonTypes upgradeButton;

    private int nullCaseCounter;

    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnPlayerUpgarde += RegisterProgress;
        GameManager.Instance.OnPlayerUpgradePanleOpen += FireQuestPanelOpen;
        GameManager.Instance.OnPlayerUpgradePanleClose += FireQuestPanelClose;
        nullCaseCounter = 0;

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        Transform questTarget = QuestTargetSystem.Instance.GetUpgradeTarget(BuildingTypes.Tavern, playerPosition);

        return questTarget;
    }

    public override Transform GetQuestCanvasTarget()
    {
        return QuestTargetSystem.Instance.GetUpgradeButton(upgradeButton);
    }

    private void RegisterProgress(UpgradeButtonTypes type)
    {
        if (type == upgradeButton)
        {
            _currentAmount++;

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
        QuestEvents.OnPlayerUpgarde -= RegisterProgress;
    }
}
