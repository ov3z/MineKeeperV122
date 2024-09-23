using System.Collections;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public static ExitDoor Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);
        CaveGameManager.Instance.OnLevelCleared += ActivateExit;

        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
    }

    private void ActivateExit()
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(true);
    }
}
