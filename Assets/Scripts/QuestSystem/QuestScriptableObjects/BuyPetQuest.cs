using UnityEngine;

[CreateAssetMenu(fileName = "New Buy Pet Quest", menuName = "QuestSystem/Quests/BuyPetQuest")]
public class BuyPetQuest : Quest
{
    [SerializeField] private PetType petType;

    private int nullCaseCounter;

    public override bool Initialize()
    {
        base.Initialize();
        QuestEvents.OnPetBuy += RegisterProgress;
        GameManager.Instance.OnPetPanelOpen += FireQuestPanelOpen;
        GameManager.Instance.OnPetPanelClose += FireQuestPanelClose;
        nullCaseCounter = 0;

        return !IsCompleted;
    }

    public override Transform GetQuestTarget()
    {
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        Transform questTarget = QuestTargetSystem.Instance.GetUpgradeTarget(BuildingTypes.PetHouse, playerPosition);

        return questTarget;
    }

    public override Transform GetQuestCanvasTarget()
    {
        return QuestTargetSystem.Instance.GetPetButton(petType);
    }

    private void RegisterProgress(PetType type)
    {
        if (type == petType)
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
        QuestEvents.OnPetBuy -= RegisterProgress;
    }
}
