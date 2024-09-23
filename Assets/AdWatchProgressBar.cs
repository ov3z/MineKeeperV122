using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;

public class AdWatchProgressBar : MonoBehaviour
{
    public static AdWatchProgressBar Instance;
    [SerializeField] private ProgressShadowed _progressShadowed;
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    private int adsWatched;
    private int maxAdsToWatch = 3;

    public Action OnBarFilled;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        OnBarFilled += ResetBar;
        _textMeshProUGUI.text = adsWatched + "/" + maxAdsToWatch;
        _progressShadowed.FillAmountX = 0;
    }

    public void UpdateProgressBar()
    {
        if (_progressShadowed != null)
        {
            _progressShadowed.DoFill2X((float)adsWatched / maxAdsToWatch, 0.3f);
            _textMeshProUGUI.text = adsWatched + "/" + maxAdsToWatch;
        }
    }

    private void UpdateText()
    {
        _textMeshProUGUI.text = adsWatched + "/" + maxAdsToWatch;
    }

    public void AddCount()
    {
        adsWatched++;
        if (adsWatched >= maxAdsToWatch)
        {
            OnBarFilled?.Invoke();
        }

        UpdateText();
        UpdateProgressBar();
    }

    public void ResetBar()
    {
        adsWatched = 0;
        UpdateProgressBar();
    }
}