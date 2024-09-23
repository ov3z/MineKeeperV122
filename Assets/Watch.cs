using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Watch : MonoBehaviour
{
    public static Watch Instance;
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    [SerializeField] private Image image;
    [SerializeField] private Sprite watchButton;
    [SerializeField] private Sprite claimButton;
    private int adsWatched;
    private int maxAdsToWatch = 3;
    
    public Action OnBarFilled;
  
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        UpdateImage();
        OnBarFilled += ResetCount;
        _textMeshProUGUI.text = adsWatched + "/" + maxAdsToWatch;
    }
    public void WatchAd()
    {
        Debug.Log("Ad has been watched");
        AdsContainer.Instance.ShowRewardedYso(AddCount, AddCount); //yso rewarded
    }

    public void AddCount()
    {
        adsWatched++;
        if (adsWatched >= maxAdsToWatch)
        {
            OnBarFilled?.Invoke();
        }

        UpdateText();
        UpdateImage();
    }

    public void ResetCount()
    {
        adsWatched = 0;
        UpdateImage();
        UpdateText();
    }

    private void UpdateImage()
    {
        if (adsWatched <= maxAdsToWatch)
        {
            image.sprite = watchButton;
        }
        else
        {
            image.sprite = claimButton;
        }
    }
    private void UpdateText()
    {
        _textMeshProUGUI.text = adsWatched + "/" + maxAdsToWatch;
    }
}