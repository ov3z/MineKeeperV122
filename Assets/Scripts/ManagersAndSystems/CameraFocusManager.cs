using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocusManager : MonoBehaviour
{
    public static CameraFocusManager Instance { get; private set; }

    public event Action OnFocusChainRegister;
    public event Action OnFocusEnd;

    [SerializeField] private CinemachineVirtualCamera cam_1;
    [SerializeField] private CinemachineVirtualCamera cam_2;

    [SerializeField] private CinemachineBrain brain;

    [SerializeField] private Transform motionInBlendRestrictionPanel;

    [SerializeField] private bool playerWaitsWhenBlending;

    private List<Transform> focusTargets = new List<Transform>();

    private CinemachineVirtualCamera currentCameraInUse;
    private CinemachineVirtualCamera currentIdleCam;

    private float timeNotAddingTargets;
    private float timeNotAddingTargetsMax = 0.5f;

    private float blendDuration;

    private bool isBlendingBetweenCameras;

    public bool IsBlendingBetweenCameras => isBlendingBetweenCameras;

    private void Awake()
    {
        Instance = this;
        blendDuration = brain.m_DefaultBlend.BlendTime;
    }

    public float AddFocusTarget(Transform target)
    {
        focusTargets.Add(target);
        timeNotAddingTargets = 0;
        float delay = (blendDuration + 0.3f) * focusTargets.Count - 0.3f;
        return delay;
    }

    private void Start()
    {
        currentCameraInUse = cam_1;
        currentIdleCam = cam_2;
    }

    private void Update()
    {
        if (!isBlendingBetweenCameras && GetQueueLenght() > 0)
        {
            timeNotAddingTargets += Time.deltaTime;
            if (timeNotAddingTargets >= timeNotAddingTargetsMax)
            {
                StartFocusSequence();
                timeNotAddingTargets = 0;
            }
        }
    }

    private void StartFocusSequence()
    {
        StartCoroutine(FocusSequenceCoroutine());
    }

    private IEnumerator FocusSequenceCoroutine()
    {
        isBlendingBetweenCameras = true;
        motionInBlendRestrictionPanel.gameObject.SetActive(true);

        OnFocusChainRegister?.Invoke();

        while (focusTargets.Count > 0)
        {
            currentCameraInUse.gameObject.SetActive(false);
            SwitchCameras();
            currentCameraInUse.Follow = focusTargets[0];
            currentCameraInUse.gameObject.SetActive(true);
            yield return new WaitForSeconds(blendDuration + 0.3f);

            focusTargets.RemoveAt(0);
        }
        isBlendingBetweenCameras = false;

        cam_1.gameObject.SetActive(false);
        cam_2.gameObject.SetActive(false);

        if (!playerWaitsWhenBlending)
            StartCoroutine(WaitPlayerbecomeVisible());

        yield return new WaitForSeconds(blendDuration);

        OnFocusEnd?.Invoke();

        motionInBlendRestrictionPanel.gameObject.SetActive(false);
    }

    private IEnumerator WaitPlayerbecomeVisible()
    {
        yield return new WaitUntil(() => PlayerController.Instance.isVisible == true);
        motionInBlendRestrictionPanel.gameObject.SetActive(false);
    }

    private void SwitchCameras()
    {
        CinemachineVirtualCamera inUse = currentCameraInUse;
        currentCameraInUse = currentIdleCam;
        currentIdleCam = inUse;
    }

    public int GetQueueLenght() => focusTargets.Count;

    public void FocusCamOnQuestTarget()
    {
        if (!isBlendingBetweenCameras)
        {
            Transform questTarget = QuestController.Instance.GetCurrentQuestTarget();
            if (questTarget != null)
            {
                OnFocusChainRegister?.Invoke();
                AddFocusTarget(questTarget);
            }
        }
    }
}
