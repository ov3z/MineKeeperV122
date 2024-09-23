using UnityEngine;

public class FallingTrailTrigger : MonoBehaviour
{
    [SerializeField] private bool shouldMakeSound;

    private Rigidbody _parendRigidbody;

    private void Start()
    {
        _parendRigidbody = transform.parent.GetComponent<Rigidbody>();
    }

    private void OnTriggerExit(Collider other)
    {
        _parendRigidbody.isKinematic = false;

        if (shouldMakeSound)
            SoundManager.Instance.Play(SoundTypes.FallingStone);
    }
}
