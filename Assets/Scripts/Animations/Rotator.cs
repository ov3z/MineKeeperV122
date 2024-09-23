using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private int direction = 1;

    private void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * direction * Time.deltaTime, Space.Self);
    }
}
