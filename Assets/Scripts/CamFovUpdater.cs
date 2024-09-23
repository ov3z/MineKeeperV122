using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFovUpdater : MonoBehaviour
{
    [SerializeField] private float minFov;
    [SerializeField] private float maxFov;

    [SerializeField] private CinemachineVirtualCamera cam;

    float fov;

    private void Start()
    {
        PlayerController.Instance.OnSpeedChangeNormalized += UpdateCameraFov;
        fov = minFov;
    }

    private void UpdateCameraFov(float fovIndex)
    {
        fov = Mathf.Lerp(minFov, maxFov, fovIndex);
    }

    private void Update()
    {
            cam.m_Lens.FieldOfView = Mathf.Lerp(cam.m_Lens.FieldOfView, fov, 1.5f * Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance)
            PlayerController.Instance.OnSpeedChangeNormalized -= UpdateCameraFov;
    }
}
