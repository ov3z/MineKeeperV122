using System.Collections;
using UnityEngine;

public class QuestTargetPointer : MonoBehaviour
{
    [SerializeField] private Transform questArrow;
    [SerializeField] private Transform worldArrowCanvas;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform canvas;

    private Transform playerTransform;
    private bool areSystemsInitialized;
    private bool shouldUpdate = true;

    private IEnumerator Start()
    {
        QuestController.Instance.OnQuestCompletion += DisablePointers;
        QuestController.Instance.OnNextQuestLoad += StartUpdatingPointer;
        playerTransform = PlayerController.Instance.transform;

        yield return null;

        areSystemsInitialized = true;
    }

    private void DisablePointers()
    {
        worldArrowCanvas.gameObject.SetActive(false);
        questArrow.gameObject.SetActive(false);

        shouldUpdate = false;
    }

    private void StartUpdatingPointer()
    {
        shouldUpdate = true;
    }


    private void LateUpdate()
    {
        if (!shouldUpdate) return;

        if (playerTransform != null && areSystemsInitialized && QuestController.Instance.GetCurrentQuestTarget() != null)
        {
            if (!worldArrowCanvas.gameObject.activeSelf)
            {
                worldArrowCanvas.gameObject.SetActive(true);
            }

            Vector3 targetPos = QuestController.Instance.GetCurrentQuestTarget().position;
            worldArrowCanvas.position = targetPos;
        }
    }

    private Vector3 GetArrowRotation(Vector3 arrowPos)
    {
        Vector3 canvasCenter = new Vector3(canvas.rect.width / 2, canvas.rect.height / 2, 0) * canvas.localScale.x;
        float angle = Vector3.SignedAngle(Vector3.up, arrowPos - canvasCenter, Vector3.forward) + 90;
        return new Vector3(0, 0, angle);
    }

    private void OnDisable()
    {
        QuestController.Instance.OnQuestCompletion -= DisablePointers;
        QuestController.Instance.OnNextQuestLoad -= StartUpdatingPointer;
    }
}
