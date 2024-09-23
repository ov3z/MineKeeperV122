using System.Collections;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    private int inventoryCapacityMax;
    private int itemsInInventory;

    public int InventoryCapacityMax => inventoryCapacityMax;
    public int ItemsInInventory => itemsInInventory;

    public bool IsThereSpaceInTheInventory => true;

    private bool hasGameStarted = false;

    private void Awake()
    {
        Instance = this;
        inventoryCapacityMax = (int)PlayerPrefs.GetFloat("Capacity", 100);
        PlayerPrefs.SetFloat("Capacity", inventoryCapacityMax);

        hasGameStarted = false;
    }

    private IEnumerator Start()
    {
        yield return null;
        hasGameStarted = true;
    }

    public void ChangeItemCountInInventory(int increment)
    {
        itemsInInventory += increment;
    }

    public void UpdateCapacityMax()
    {
        inventoryCapacityMax = (int)PlayerPrefs.GetFloat("Capacity", 100);

        if (!GameManager.Instance.IsInCave && InventoryUI.Instance)
            InventoryUI.Instance.UpdateInventoryText();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        float capacityBonus = PlayerPrefs.GetFloat("CapacityBonus", 0);
        if (!hasFocus)
        {
            PlayerPrefs.SetFloat("Capacity", PlayerPrefs.GetFloat("Capacity", 100) - capacityBonus);
        }
        else if (hasGameStarted)
        {
            PlayerPrefs.SetFloat("Capacity", PlayerPrefs.GetFloat("Capacity", 100) + capacityBonus);
            UpdateCapacityMax();
        }
    }
}
