using System.Collections;
using System.Configuration;
using UnityEngine;

public class UpgradeInteractor : MonoBehaviour, IInteractable
{
    [SerializeField] private bool canInteractInMotion = false;
    [SerializeField] private bool registerForQuest = false;
    [SerializeField] private BuildingTypes questType;
    [SerializeField] private UpgradePanel upgradePanel;
    [SerializeField] private Camera renderCamera;


    private float interactionTimerMax = 0.3f;
    private float timeBetweenBPs = 0.5f;
    private float BPTimer;

    private bool isInteracting;

    private IEnumerator Start()
    {
        upgradePanel.OnClose += DisableRenderCamera;

        yield return new WaitForSeconds(0.5f);

        if (registerForQuest)
            QuestTargetSystem.Instance.AddUpgrade(questType, transform);
    }

    private void DisableRenderCamera()
    {
        if (renderCamera != null)
            renderCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isInteracting)
        {
            BPTimer += Time.deltaTime;
            if (BPTimer > timeBetweenBPs)
            {
                isInteracting = false;
            }
        }
    }

    public bool CanInteractInMotion() => canInteractInMotion;

    public float GetInteractionTimerMax() => interactionTimerMax;

    public Vector3 GetPosition() => transform.position;

    public void Interact(Transform interactor)
    {
        BPTimer = 0;
        if (!isInteracting)
        {
            isInteracting = true;

            upgradePanel.OpenSettingsPanel();

            if (renderCamera != null)
                renderCamera.gameObject.SetActive(true);
        }
    }

    public void SetActive(bool state)
    {
        throw new System.NotImplementedException();
    }
}
