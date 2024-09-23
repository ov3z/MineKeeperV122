using UnityEngine;

[CreateAssetMenu(fileName = "New Build Quest", menuName = "QuestSystem/Quests/BuildQuest")]
public class BuildQuest : Quest
{
    [SerializeField] private BuildingTypes questType;

    private int nullCaseCounter;

    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnBuilding += RegisterProgress;
        nullCaseCounter = 0;

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        Vector3 playerPosition = PlayerController.Instance.transform.position;

        Transform questTarget = QuestTargetSystem.Instance.GetBuildingTarget(questType, playerPosition);

        int focusQueueLenght = CameraFocusManager.Instance.GetQueueLenght();

        if (questTarget == null)
        {
            nullCaseCounter++;
            if (nullCaseCounter >= focusQueueLenght * 250 + 70)
                RegisterProgress(questType);
        }

        return questTarget;
    }

    private void RegisterProgress(BuildingTypes buildingType)
    {
        if (buildingType == questType)
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
        QuestEvents.OnBuilding -= RegisterProgress;
    }
}
