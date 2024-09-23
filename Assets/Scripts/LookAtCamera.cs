using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Mode mode;

    private enum Mode
    {
        LookAt,
        LookAtIverted,
        CameraForward,
        CameraForwardInverted,
    }

    private void LateUpdate()
    {
        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(Camera.main.transform.position);
                break;
            case Mode.LookAtIverted:
                Vector3 deltaDist = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + deltaDist);
                break;
            case Mode.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            case Mode.CameraForwardInverted:
                transform.forward = -Camera.main.transform.forward;
                break;
        }
    }
}
