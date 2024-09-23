using UnityEngine;

public class RotationSyncer : MonoBehaviour
{
    private void Start()
    {
        float rotationZ = PlayerPrefs.GetFloat("RotationAngle", 0);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, rotationZ);
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat("RotationAngle", transform.localEulerAngles.z);
    }
}
