using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade Quest", menuName = "QuestSystem/Quests/UpgradeQuest")]
public class UpgradeQuest : Quest
{
    [SerializeField] private BuildingTypes upgradePlace;
    [SerializeField] private BuildingTypes upgradeTarget;
    [SerializeField] private UpgradeButtonTypes upgradeButton;

    private int nullCaseCounter;

    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnUpgrade += RegisterProgress;
        GameManager.Instance.OnTownHallPanelOpen += FireQuestPanelOpen;
        GameManager.Instance.OnTownHallPanelClose += FireQuestPanelClose;
        nullCaseCounter = 0;

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        Vector3 playerPosition = PlayerController.Instance.transform.position;

        Transform questTarget = QuestTargetSystem.Instance.GetUpgradeTarget(upgradePlace, playerPosition);

        int focusQueueLenght = CameraFocusManager.Instance.GetQueueLenght();

        if (questTarget == null)
        {
            nullCaseCounter++;
            if (nullCaseCounter >= focusQueueLenght * 120 + 70)
                RegisterProgress(upgradeTarget);
        }

        return questTarget;
    }

    public override Transform GetQuestCanvasTarget()
    {
        return QuestTargetSystem.Instance.GetUpgradeButton(upgradeButton);
    }

    private void RegisterProgress(BuildingTypes type)
    {
        if (type == upgradeTarget)
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
        QuestEvents.OnUpgrade -= RegisterProgress;
    }
}
