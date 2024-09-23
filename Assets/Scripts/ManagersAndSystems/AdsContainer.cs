using System;
using UnityEngine;

public class AdsContainer : MonoBehaviour
{
    public static AdsContainer Instance;

    private Action OnRewardedSucced;
    private Action OnRewardedFail;
    private Action OnInterstitalSucceed;
    private Action OnInterstitalFail;

#if (UNITY_ANDROID || UNITY_IOS) && MAX_SDK
    private const string SDK_KEY = "PyEhseMunI3DtCpKH3imtL0prh8Qy8oNsTcsbqOcL6GyT5mFGxpXymvJMQb9sMrhTcLSbXsq1GOMZJhzkaIAmn";
    private const string BANNER_ID = "c2598461bf3b6520";
    private const string INTERSTITIAL_ID = "b95eda21a8d12bde";
    private const string REWARDED_ID = "7f5d4f9e91f956e9";

    private int retryAttemptInterstitial;
    private int retryAttemptRewarded;

    private void InitializeBanner()
    {
        MaxSdk.CreateBanner(BANNER_ID, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerBackgroundColor(BANNER_ID, Color.black);
        MaxSdk.ShowBanner(BANNER_ID);
    }

    #region Interstitials
    public void InitializeInterstitialAds()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(INTERSTITIAL_ID);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        retryAttemptInterstitial = 0;

        Debug.Log($"timescale on interstitial load event {Time.timeScale}");
        Debug.Log(adInfo.Placement);
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        retryAttemptInterstitial++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttemptInterstitial));

        Invoke("LoadInterstitial", (float)retryDelay);

        Debug.Log(errorInfo.Message);
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {

    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        OnInterstitalFail?.Invoke();

        OnInterstitalFail = () => { };
        OnInterstitalSucceed = () => { };

        LoadInterstitial();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        OnInterstitalSucceed?.Invoke();
        LoadInterstitial();
    }
    #endregion

    #region Rewarded

    public void InitializeRewardedAds()
    {
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

        // Load the first rewarded ad
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(REWARDED_ID);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        retryAttemptRewarded = 0;

        Debug.Log("rewarded ad loaded");
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        retryAttemptRewarded++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttemptRewarded));

        Debug.Log("rewardeed load failed");

        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        OnRewardedFail?.Invoke();

        OnRewardedFail = () => { };
        OnRewardedSucced = () => { };

        LoadRewardedAd();
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        OnRewardedSucced?.Invoke();
        OnRewardedSucced = () => { };
        OnRewardedFail = () => { };
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }
    #endregion

#endif

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        DontDestroyOnLoad(gameObject);

    }
    private void Start()
    {
#if (UNITY_ANDROID || UNITY_IOS) && MAX_SDK
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            Debug.Log("have found the ads intitializer");
            //InitializeBanner();
            InitializeInterstitialAds();
            InitializeRewardedAds();
        };

        MaxSdk.SetSdkKey(SDK_KEY);
        MaxSdk.InitializeSdk();
#endif
    }

    public void ShowInsterstitial(Action OnInterstialEnd)
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        void StarAction() => OnInterstialEnd?.Invoke();
        CrazyGames.CrazyAds.Instance.beginAdBreak(StarAction, StarAction);
#elif (UNITY_ANDROID || UNITY_IOS) && MAX_SDK
        OnInterstitalSucceed = OnInterstialEnd;
        OnInterstitalFail = OnInterstialEnd;

        if (MaxSdk.IsInterstitialReady(INTERSTITIAL_ID))
        {
            MaxSdk.ShowInterstitial(INTERSTITIAL_ID);
        }
        else
        {
            Debug.Log("interstitial inst ready");

            OnInterstitalFail?.Invoke();

            OnInterstitalFail = () => { };
            OnInterstitalSucceed = () => { };
        }
#else
        OnInterstialEnd?.Invoke();
#endif
    }
    
    

    public void ShowRewarded(Action OnCompletion, Action OnFail = null)
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        void Success()
        {
            OnCompletion?.Invoke();
            OnCompletion = () => { };
        }
        void Failed()
        {
            OnCompletion = () => { };
            OnFail?.Invoke();
        }
        CrazyGames.CrazyAds.Instance.beginAdBreakRewarded(Success, Failed);
#elif (UNITY_ANDROID || UNITY_IOS) && MAX_SDK
        OnRewardedSucced = OnCompletion;
        OnRewardedFail = OnFail;

        if (MaxSdk.IsRewardedAdReady(REWARDED_ID))
        {
            MaxSdk.ShowRewardedAd(REWARDED_ID);
        }
        else
        {

            Debug.Log("rewarded inst ready");

            OnRewardedFail?.Invoke();

            OnRewardedFail = () => { };
            OnRewardedSucced = () => { };
        }
#else
        OnCompletion?.Invoke();
#endif
    }
    

    
    public void ShowInsterstitialYso(Action action)
    {
#if YSO_ENABLED
          YsoCorp.GameUtils.YCManager.instance.adsManager.ShowInterstitial(() => { action?.Invoke(); });
#endif
    }

    public void ShowRewardedYso(Action OnCompletion, Action OnFail = null)
    {
#if YSO_ENABLED
       YsoCorp.GameUtils.YCManager.instance.adsManager.ShowRewarded((bool ok) =>
       {
          if (ok)
          {
             OnCompletion?.Invoke();
          }
          else
          {
             OnFail?.Invoke();
          }
       });
#endif
    }
    
    
}

