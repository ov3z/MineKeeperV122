using UnityEngine;

public class CaveExitDoorTrigger : MonoBehaviour
{
    private bool isFirstAttempt = true;

    private void OnTriggerEnter(Collider other)
    {
        if (isFirstAttempt)
        {
            isFirstAttempt = false;
            CaveGameManager.Instance.ShowWinPanel();
        }
    }
}
