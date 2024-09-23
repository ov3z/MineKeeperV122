using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourcesUI : MonoBehaviour
{
    public static ResourcesUI Instance { get; private set; }

    [SerializeField] private List<ResourceUIUnit> resourceUnits = new List<ResourceUIUnit>();

    private Dictionary<ResourceTypes, ResourceUIUnit> resourceUIMap = new Dictionary<ResourceTypes, ResourceUIUnit>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeResourceUI();
        ResourceStorage.Instance.OnResourceAmountChange += UpdateResourceAmountUI;
    }

    private void InitializeResourceUI()
    {
        foreach (var unit in resourceUnits)
        {
            resourceUIMap.Add(unit.Type, unit);
            resourceUIMap[unit.Type].Text.text = $"{ResourceStorage.Instance.GetResourceBalance(unit.Type)}";
            if (ResourceStorage.Instance.GetResourceBalance(unit.Type) == 0 && unit.Type != ResourceTypes.Coins)
                unit.Parent.gameObject.SetActive(false);
        }

        if (transform.GetChild(0).GetActiveChildrenCount() == 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void UpdateResourceAmountUI(ResourceTypes type, float amount)
    {
        if (amount <= 0 && type != ResourceTypes.Coins)
            resourceUIMap[type].Parent.gameObject.SetActive(false);
        else if (!resourceUIMap[type].Parent.gameObject.activeSelf)
            resourceUIMap[type].Parent.gameObject.SetActive(true);

        if (transform.GetChild(0).GetActiveChildrenCount() == 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        else if (!transform.GetChild(0).gameObject.activeSelf)
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }

        resourceUIMap[type].Text.text = $"{Mathf.RoundToInt(amount)}";
    }

    [System.Serializable]
    public class ResourceUIUnit
    {
        public ResourceTypes Type;
        public Transform Parent;
        public TextMeshProUGUI Text;
    }
}
