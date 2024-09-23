using System.Collections;
using UnityEngine;

public class PlayerArrow : MonoBehaviour
{
    private bool isArrowEnabled;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);
        CaveGameManager.Instance.OnLevelCleared += EnableArrow;
    }

    private void EnableArrow()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        isArrowEnabled = true;
    }

    private void Update()
    {
        if (isArrowEnabled)
        {
            var dir = (ExitDoor.Instance.transform.position - PlayerController.Instance.transform.position).normalized;
            dir -= Vector3.up * dir.y;
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, angle, transform.eulerAngles.z);
        }
    }
}
