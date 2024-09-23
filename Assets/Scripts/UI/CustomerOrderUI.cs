using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerOrderUI : MonoBehaviour
{
    [SerializeField] Image orderIcon;
    [SerializeField] Image orderTimerFill;
    [SerializeField] Image noProductAlert;
    [SerializeField] TextMeshProUGUI orderCountText;

    public void EnableOrderUI()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        orderTimerFill.fillAmount = 0;
    }

    public void DisableOrderUI()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void SetOrderSprite(ResourceTypes resourceType)
    {
        orderIcon.sprite = ResourceSpriteStorage.Instance.GetIcon(resourceType);
    }

    public void SetOrderCount(int orderCount)
    {
        orderCountText.text = $"{orderCount}";
    }

    public void SetOrderTimerFill(float progress)
    {
        orderTimerFill.fillAmount = progress;
    }

    public void EnableNoProductAlert()
    {
        noProductAlert.gameObject.SetActive(true);
    }

    public void DisableNoProductAlert()
    {
        noProductAlert.gameObject.SetActive(false);
    }

    public bool IsNoProductAlertEnabled()
    {
        return noProductAlert.gameObject.activeSelf;
    }

    [System.Serializable]
    public class ResourceIcons
    {
        public ResourceTypes Type;
        public Sprite Sprite;
    }
}
