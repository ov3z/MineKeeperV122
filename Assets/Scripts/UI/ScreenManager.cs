using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.UI;
public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;
    private ScreenOrieantation _currentOrientation = ScreenOrieantation.None;
    public Action<ScreenOrieantation> OnScreenChange;
    public Transform baseCanvas;
    public Transform LandscapeCanvas;
    public Transform PortraitCanvas;

    public CinemachineVirtualCamera mainVCam;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        OnScreenChange += ChangeCameraFOV;
#endif
    }

    void Update()
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        CheckAndUpdateOrientation();
#endif
    }

/*    private void OnValidate()
    {
        CheckAndUpdateOrientation();
        Debug.Log("valide");
    }
*/
    private void CheckAndUpdateOrientation()
    {
        if (Screen.width > Screen.height)
        {
            if (_currentOrientation != ScreenOrieantation.Landscape)
            {
                _currentOrientation = ScreenOrieantation.Landscape;
                OnScreenChange?.Invoke(_currentOrientation);
                Extensions.CopyRectTransform(baseCanvas, LandscapeCanvas);
            }
        }
        else
        {
            if (_currentOrientation != ScreenOrieantation.Portrait)
            {
                _currentOrientation = ScreenOrieantation.Portrait;
                OnScreenChange?.Invoke(_currentOrientation);
                Extensions.CopyRectTransform(baseCanvas, PortraitCanvas);
            }
        }
    }

    private void ChangeCameraFOV(ScreenOrieantation screenOrintation)
    {
        if (screenOrintation == ScreenOrieantation.Portrait)
            mainVCam.m_Lens.FieldOfView = 40;
        else
            mainVCam.m_Lens.FieldOfView = 30;
    }
}
public enum ScreenOrieantation
{
    None,
    Landscape,
    Portrait
}