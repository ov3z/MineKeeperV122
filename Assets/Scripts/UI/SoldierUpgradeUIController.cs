using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SoldierUpgradeUIController : MonoBehaviour, ICoinTradeItem
{
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private int soldierType;

    public float Price => float.Parse(priceText.text);
    public Button BuyButton => buyButton;

    public void Aquire()
    {
        SoldierUpgradeManager.Instance.AquireSoldier(soldierType);
    }
}
