using UnityEngine;

public class IntroductionScene_2_Helper : MonoBehaviour
{
    [SerializeField] private Transform areaCollider;
    [SerializeField] private RabbitController rabbit;

    private void Start()
    {
        QuestController.Instance.OnQuestCompletion += SetUpScene;
    }

    private void SetUpScene()
    {
        areaCollider.gameObject.SetActive(false);
        rabbit.SwitchState(RabbitStates.RunAway);
    }
}
