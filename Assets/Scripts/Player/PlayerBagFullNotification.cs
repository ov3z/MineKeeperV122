using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBagFullNotification : MonoBehaviour
{
    [SerializeField] private Transform notification;

    private IEnumerator Start()
    {
        ResourceStorage.Instance.OnResourceAmountChange += OnResourceAmountChange;

        yield return null;

        CheckSpaceAndUpdateUI();
    }

    private void OnResourceAmountChange(ResourceTypes type, float amount)
    {
        CheckSpaceAndUpdateUI();
    }

    private void CheckSpaceAndUpdateUI()
    {
        var isInventoryFull = InventoryController.Instance.IsThereSpaceInTheInventory == false;

        if (isInventoryFull && !notification.gameObject.activeSelf)
        {
            notification.gameObject.SetActive(true);
        }
        else if (!isInventoryFull && notification.gameObject.activeSelf)
        {
            notification.gameObject.SetActive(false);
        }
    }
}
