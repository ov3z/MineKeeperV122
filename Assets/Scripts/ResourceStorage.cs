using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

public class ResourceStorage : MonoBehaviour
{
    public static ResourceStorage Instance { get; private set; }

    public event Action<ResourceTypes, float> OnResourceAmountChange;

    private Dictionary<ResourceTypes, float> resourceBalanceMap = new Dictionary<ResourceTypes, float>();

    private bool isInventoryFree => InventoryController.Instance.IsThereSpaceInTheInventory;

    private void Awake()
    {
        Instance = this;
        Load();
    }

    private void Load()
    {
        foreach (ResourceTypes type in Enum.GetValues(typeof(ResourceTypes)))
        {

            float balance = PlayerPrefs.GetFloat($"Resource{(int)type}", 0);
            resourceBalanceMap.Add(type, balance);
        }
    }

    public void ChangeResourceAmount(ResourceTypes type, int increment)
    {
        if (type == ResourceTypes.Coins || (increment < 0 || (isInventoryFree || increment > 10)))
        {
            if (resourceBalanceMap.ContainsKey(type))
                resourceBalanceMap[type] += increment;
            else
                resourceBalanceMap.Add(type, increment);
        }

        if (increment > 0)
        {
            QuestEvents.FireOnResourceEarn(type, increment);

            if (type == ResourceTypes.Coins)
            {
                SoundManager.Instance.Play(SoundTypes.Coin);
            }
            else
            {
                if (type != ResourceTypes.Emerald)
                    SoundManager.Instance.Play(SoundTypes.Collect);
            }
        }
        else
        {
            QuestEvents.FireOnResourceSpend(type, Mathf.Abs(increment));
            SoundManager.Instance.Play(SoundTypes.Drop);
        }

        OnResourceAmountChange?.Invoke(type, resourceBalanceMap[type]);
        Save();
    }

    public float GetResourceBalance(ResourceTypes type)
    {
        return resourceBalanceMap[type];
    }

    private void Save()
    {
        foreach (ResourceTypes type in Enum.GetValues(typeof(ResourceTypes)))
        {
            float balance = resourceBalanceMap[type];
            PlayerPrefs.SetFloat($"Resource{(int)type}", balance);
        }
    }
}
