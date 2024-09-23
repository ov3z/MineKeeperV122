using UnityEngine;

public class UIQuestTargetController : MonoBehaviour
{
    [SerializeField] private Transform hand;

    private bool canUpdateHandPos;
    private Vector3 handOffset = new Vector3(137, -97, 0);

    private void Start()
    {
        QuestController.Instance.OnCurrentQuestPanelOpen += EnableHand;
        QuestController.Instance.OnCurrentQuestPanelClose += DisableHand;
        QuestController.Instance.OnQuestCompletion += DisableHand;
    }

    private void EnableHand()
    {
        ActivateHand(true);
    }

    private void DisableHand()
    {
        ActivateHand(false);
    }

    private void ActivateHand(bool isActive)
    {
        hand.gameObject.SetActive(isActive);
        canUpdateHandPos = isActive;
    }

    private void Update()
    {
        if (canUpdateHandPos)
        {
            Transform targetButton = QuestController.Instance.GetCurrentQuestCanvasTarget();
            hand.transform.position = Vector3.Lerp(hand.transform.position, targetButton.TransformPoint(handOffset), 15 * Time.deltaTime);
        }
    }
}
