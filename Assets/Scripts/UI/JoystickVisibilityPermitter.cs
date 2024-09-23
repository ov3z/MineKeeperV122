using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickVisibilityPermitter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static JoystickVisibilityPermitter Instance { get; private set; }

    private bool canShowjoystick = false;

    private void Awake()
    {
        Instance = this;
    }

    public bool CanShowJoystick() => canShowjoystick;

    public void OnPointerDown(PointerEventData eventData)
    {
        canShowjoystick = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        canShowjoystick = false;
    }
}
