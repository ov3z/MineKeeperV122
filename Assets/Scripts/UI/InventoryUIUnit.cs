using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIUnit : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }

    public void SetAmount(int amount)
    {
        amountText.text = $"{amount}";
    }

    public int GetIncrement(int newAmount)
    {
        return newAmount - int.Parse(amountText.text);
    }

    public void SetIconActive(bool isActive)
    {
        icon.gameObject.SetActive(isActive);

        if (isActive)
        {
            transform.SetAsFirstSibling();
            return;
        }
        transform.SetAsLastSibling();
    }
}
