using UnityEngine;

public class EnablePayingInteractor : MonoBehaviour
{
    [SerializeField] private PayoutValidation payingInteractor;

    private bool isOpened;

    private void OnTriggerEnter(Collider other)
    {
        if (!isOpened)
        {
            payingInteractor.OnPayout();
            isOpened = true;
        }
    }
}
