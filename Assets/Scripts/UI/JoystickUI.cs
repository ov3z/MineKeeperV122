using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickUI : MonoBehaviour
{
    public static JoystickUI Instance;

    [SerializeField] RectTransform handle;
    [SerializeField] RectTransform joystickBackground;

    private Vector3 previousMousePosition;
    private float joystickDisplacementMax = 75;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Hide();
    }

    private void Hide()
    {
        joystickBackground.gameObject.SetActive(false);
    }

    private void Show()
    {
        if (JoystickVisibilityPermitter.Instance.CanShowJoystick())
        {
            joystickBackground.gameObject.SetActive(true);
            handle.anchoredPosition = Vector2.zero;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousMousePosition = Input.mousePosition;
            joystickBackground.position = previousMousePosition;
            Show();
        }
        if (Input.GetMouseButton(0))
        {
            if (previousMousePosition != Input.mousePosition)
            {
                Vector3 dir = (Input.mousePosition - previousMousePosition).normalized;
                float swipeDistance = (Input.mousePosition - previousMousePosition).magnitude;
                swipeDistance = Mathf.Clamp(swipeDistance, 0, joystickDisplacementMax);

                handle.anchoredPosition = dir * swipeDistance;
            }
        }
        if (Input.GetMouseButtonUp(0) || (CameraFocusManager.Instance != null && CameraFocusManager.Instance.IsBlendingBetweenCameras))
        {
            previousMousePosition = Input.mousePosition;
            Hide();
        }
    }

    public float GetJoystickDisplacementNormalized() => handle.anchoredPosition.magnitude / joystickDisplacementMax;
    public Vector3 GetJoystickDisplacementDirectionWorld()
    {
        Vector3 joystickDispDir = handle.anchoredPosition.normalized;
        return new Vector3(joystickDispDir.x, 0, joystickDispDir.y);
    }
}
