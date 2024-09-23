using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class LootKey : MonoBehaviour
{
    private bool canFollowPlayer;

    private Transform PlayerTransform => PlayerController.Instance.transform;

    private void Start()
    {
        StartCoroutine(StartFollowPlayer());
    }

    private void Update()
    {
        if (canFollowPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, PlayerTransform.position, 9 * Time.deltaTime);

            var distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

            if (distanceToPlayer < 1f)
            {
                PlayerController.Instance.OnResourceCollect(ResourceTypes.Key, 1);
                gameObject.SetActive(false);
            }

        }
    }


    private IEnumerator StartFollowPlayer()
    {
        yield return new WaitForSeconds(1.5f);

        canFollowPlayer = true;
        transform.GetChild(1).gameObject.SetActive(false);
    }
}
