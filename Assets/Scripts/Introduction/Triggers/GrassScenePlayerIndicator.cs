using System.Collections;
using UnityEngine;

public class GrassScenePlayerIndicator : MonoBehaviour
{
    [SerializeField] RabbitController rabbit;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        CameraFocusManager.Instance.FocusCamOnQuestTarget();
    }

    private void OnTriggerEnter(Collider other)
    {
        rabbit.SwitchState(RabbitStates.RunAway);
    }
}
