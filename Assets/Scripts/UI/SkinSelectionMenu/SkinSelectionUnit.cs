using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerController;

public class SkinSelectionUnit : MonoBehaviour
{
    [SerializeField] private PlayerGender gender;
    [SerializeField] private int price;

    [SerializeField] private Button equipButton;
    [SerializeField] private Button buyButton; //shutayda buttonlary alya , her skinin ozunde bar shu script 
    [SerializeField] private Button adWatch;
    [SerializeField] private TextMeshProUGUI priceText_1;
    [SerializeField] private TextMeshProUGUI priceText_2;

    [SerializeField] private Transform equippedButton;
    [SerializeField] private Transform specialText;

    [SerializeField] private bool isUnlocked;

    public int adsWatched;
    public int maxAdsToWatch = 3;
    [SerializeField] private Image image;
    [SerializeField] private Sprite watchButton;
    [SerializeField] private Sprite claimButton;
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;

    [SerializeField] private GuidComponent guid;
    private int SkinIndex => transform.GetSiblingIndex();
    private string ID => guid.GetGuid().ToString();

    public Action OnBarFilled;

    private bool _isWatchingAd = false;

    private void Start()
    {
        ResetCount();
    }

    private bool IsEnabled
    {
        get { return PlayerPrefs.GetInt($"IsUnlocked{ID}", (isUnlocked) ? 1 : 0) == 1; }
        set
        {
            var intValue = (value) ? 1 : 0;
            PlayerPrefs.SetInt($"IsUnlocked{ID}", intValue);
        }
    }

    private int SelectedSkinData
    {
        get { return PlayerPrefs.GetInt("SelectedSkin", PlayerPrefs.GetInt("Gender", 0) * 10); }
        set { PlayerPrefs.SetInt("SelectedSkin", value); }
    }

    private bool IsEquipped =>
        SelectedSkinData / 10 == (int)gender && SelectedSkinData % 10 == transform.GetSiblingIndex();

    public void Equip()
    {
        PlayerController.Instance.ChangeSkin(SkinIndex, gender);

        int firstDigit = ((int)gender) * 10;
        int secondDigit = SkinIndex;

        SelectedSkinData = firstDigit + secondDigit;
        equipButton.gameObject.SetActive(true);
    }


    public void EnableButtons()
    {
        equipButton.onClick.RemoveAllListeners();
        buyButton.onClick.RemoveAllListeners();
        adWatch.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(Equip);
        buyButton.onClick.AddListener(Buy);
        adWatch.onClick
            .AddListener(WatchAd); //bardede hemme listener ayrylya remove etya sonam name goshmaly bolsa shony goshya 
        UpdateImage();
        UpdateText();
        if (IsEquipped)
        {
            equippedButton.gameObject.SetActive(true);
            equipButton.gameObject.SetActive(false);
            buyButton.gameObject.SetActive(false);
            specialText.gameObject.SetActive(false);
            
            adWatch.gameObject.SetActive(false);
        }
        else if (IsEnabled)
        {
            equipButton.gameObject.SetActive(true);
            buyButton.gameObject.SetActive(false);
            equippedButton.gameObject.SetActive(false);
            specialText.gameObject.SetActive(false);
            
            adWatch.gameObject.SetActive(false);
        }
        else if (price < 0)
        {
            buyButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(false);
            equippedButton.gameObject.SetActive(false);
            specialText.gameObject.SetActive(true);
            
            adWatch.gameObject.SetActive(false);
        }
        else
        {
            buyButton.gameObject.SetActive(true);
            equipButton.gameObject.SetActive(false);
            equippedButton.gameObject.SetActive(false);
            specialText.gameObject.SetActive(false);
            
            adWatch.gameObject.SetActive(true);
        }

        priceText_1.text = $"{price}";
        priceText_2.text = $"{price}";
    }

    private void Buy()
    {
        if (price <= ResourceStorage.Instance.GetResourceBalance(ResourceTypes.Coins))
        {
            ResourceStorage.Instance.ChangeResourceAmount(ResourceTypes.Coins, -price);
            Unlock();
        }
    }

    public void Unlock()
    {
        IsEnabled = true;
        equipButton.gameObject.SetActive(true);
        buyButton.gameObject.SetActive(false);
        adWatch.gameObject.SetActive(false);
    }

    #region AdWatch

    public void WatchAd()
    {
        if (adsWatched < maxAdsToWatch)
        {
            Debug.Log("Ad has been watched");
            _isWatchingAd = true;
            AdsContainer.Instance.ShowRewardedYso(AddCount, AddCount); //yso rewarded
        }
        else
        {
            Unlock();
        }
    }

    private void UpdateImage()
    {
        if (adsWatched < maxAdsToWatch)
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
        _textMeshProUGUI.enabled = adsWatched < maxAdsToWatch;
    }

    private void AddCount()
    {
        if (!_isWatchingAd) return;
        _isWatchingAd = false;
        adsWatched++;
        Debug.Log($"Adding {name}, {adsWatched} >= {maxAdsToWatch}");
        if (adsWatched >= maxAdsToWatch)
        {
            OnBarFilled?.Invoke();
        }

        UpdateImage();
        UpdateText();
    }

    public void ResetCount()
    {
        adsWatched = 0;
        UpdateImage();
        UpdateText();
    }

    private void Reset()
    {
        UpdateImage();
        _textMeshProUGUI.text = adsWatched + "/" + maxAdsToWatch;
    }

    #endregion
}