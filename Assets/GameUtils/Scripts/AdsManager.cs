using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
    
namespace YsoCorp {
    namespace GameUtils {

        [DefaultExecutionOrder(-10)]
        public class AdsManager : BaseManager {

            public enum BooleanOverride {
                NoOverride,
                True,
                False
            }

            public static string DATA_PRIVACY_URL = "https://www.ysocorp.com/privacy-policy";
            private static string SDK_KEY = "Wbl6_lWhN3Hoy5kL_UNgTb6Ed7o69yD8-mFOzFG68AmdgawolWGk0K2W_GHiai_D5N5pE_DA7MSfJoJZ_oOm1G";

            public bool isConsentFlowShown;
            public int consentFlowStatus;

            public Image iInterstitial;
            public Image iLoader;
            public Image iBanner;
            public Button bRemoveAds;

            public bool rewardedVisible {
                get; set;
            }
            public bool interstitialVisible {
                get; set;
            }

            private Action<bool> aRewarded = null;
            private Action aInterstitial = null;
            private bool _finishRewarded = false;
            private float _delayInterstitial = 0f;
            public float delayInterstitial { get { return this._delayInterstitial; } set { } }
            [HideInInspector] public float delayInterstitialOverride = -1f;
            [HideInInspector] public BooleanOverride rewardedResetDelayOverride = BooleanOverride.NoOverride;
            private BooleanOverride _bannerTransparencyOverride = BooleanOverride.NoOverride;
            [HideInInspector] public BooleanOverride bannerTransparencyOverride {
                get { return this._bannerTransparencyOverride; }
                set {
                    this._bannerTransparencyOverride = value;
                    this.SetBannerTransparency();
                }
            }

#if !UNITY_EDITOR
            private bool _isEnableRewarded = false;
#endif
            private bool _isEnableInterstitial = false;
            private bool _isEnableBanner = false;

            private bool _bannerHide = true;
            private bool _canDisplayInterstitial = false;

            private MaxSdkBase.SdkConfiguration _sdkConfiguration;

            private float _onDisplayTimeScale;

            private Dictionary<string, double> _tooSmallRevenuesRR;

#if AMAZON_APS
            private bool isInterstitialFirstLoad = true;
            private bool isRewardedFirstLoad = true;
            private AmazonAds.APSVideoAdRequest interstitialAdRequest;
            private AmazonAds.APSVideoAdRequest rewardedAdRequest;
            private AmazonAds.APSBannerAdRequest bannerAdRequest;
#endif

            private void Awake() {
                this.ycManager.adsManager = this;
                this.bRemoveAds.onClick.AddListener(() => {
                    this.ycManager.inAppManager.BuyProductIDAdsRemove();
                });
                this.iBanner.gameObject.SetActive(false);
                this.iInterstitial.gameObject.SetActive(false);
                if (this.GetInterstitialKey() != "" || this.GetRewardedKey() != "" || this.GetBannerKey() != "") {
                    this.InitMax();
                } else {
                    this.ycManager.ycConfig.LogWarning("[Ads] not init");
                }
            }

            private void InitMax() {
#if AMAZON_APS && !UNITY_EDITOR
                this.InitAmazon();
#endif
                MaxSdkCallbacks.OnSdkInitializedEvent += this.OnSdkInitializedEvent;
                MaxSdk.SetSdkKey(SDK_KEY);
                MaxSdk.SetUserId(this.ycManager.ycConfig.deviceKey);
                MaxSdk.SetVerboseLogging(this.ycManager.ycConfig.activeLogs.HasFlag(YCConfig.YCLogCategories.ApplovinMax));
                MaxSdk.InitializeSdk();
            }

            public bool IsInterstitialOrRewardedVisible() {
                return this.rewardedVisible || this.interstitialVisible;
            }

            public string GetAppTrackingStatus() {
#if UNITY_IOS
                if (this._sdkConfiguration != null) {
                    return this._sdkConfiguration.AppTrackingStatus.ToString();
                }
#endif
                return "";
            }

            public AnalyticsManager.AdData OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
                AnalyticsManager.AdData adData = new AnalyticsManager.AdData();
                adData.revenue = (float)adInfo.Revenue;
                adData.country_code = MaxSdk.GetSdkConfiguration().CountryCode;
                adData.network_name = adInfo.NetworkName;
                adData.network_placement = adInfo.NetworkPlacement;
                adData.placement = adInfo.Placement;
                adData.ad_unit_id = adInfo.AdUnitIdentifier;
                adData.creative_id = adInfo.CreativeIdentifier;
                return adData;
            }

            void OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration sdkConfiguration) {
                this._sdkConfiguration = sdkConfiguration;
                this.ycManager.mmpManager.Init();
                this.InitConsentAndAds();
            }

            void LoaderRotate() {
                Vector3 angles = this.iLoader.transform.localEulerAngles;
                angles.z -= Time.deltaTime * 270f;
                this.iLoader.transform.localEulerAngles = angles;
            }

            void InitAmazon() {
#if AMAZON_APS
                if (this.GetAmazonAppID() != "") {
                    AmazonAds.Amazon.Initialize(this.GetAmazonAppID());
                    AmazonAds.Amazon.SetAdNetworkInfo(new AmazonAds.AdNetworkInfo(AmazonAds.DTBAdNetwork.MAX));
                    AmazonAds.Amazon.UseGeoLocation(true);
                    //AmazonAds.Amazon.EnableLogging(true);
                    //AmazonAds.Amazon.EnableTesting(true);
#if UNITY_IOS
                    AmazonAds.Amazon.SetAPSPublisherExtendedIdFeatureEnabled(true);
#endif
                }
#endif
            }

            void InitConsentAndAds() {
                this.InitConsent();
                this.InitAds();
            }

            void InitConsent() {
                //MaxSdk.SetIsAgeRestrictedUser(false);
            }

#if IN_APP_PURCHASING
            private void Start() {
                this.ycManager.inAppManager.AddListener(this.ycManager.ycConfig.InAppRemoveAds, () => this.BuyAdsShow());
            }
#endif

            private void Update() {
                this.bRemoveAds.gameObject.SetActive(
                    this.ycManager.ycConfig.InAppRemoveAdsCanRemoveInBanner &&
                    this.ycManager.ycConfig.InAppRemoveAds != "" &&
                    this.ycManager.dataManager.GetAdsShow()
                );
                this.LoaderRotate();
                if (this._delayInterstitial > 0f) {
                    this._delayInterstitial -= Time.unscaledDeltaTime;
                }
            }

            private void InitAds() {
                this._tooSmallRevenuesRR = this.ycManager.dataManager.GetRedRockRevenue();
                if (this._tooSmallRevenuesRR == null) this._tooSmallRevenuesRR = new Dictionary<string, double>();

                if (this.ycManager.ycConfig.UseBanners) {
#if UNITY_EDITOR
                    if (this.ycManager.ycConfig.UseBannersInEditor) {
                        this.InitializeBannerAds();
                    }
#else
                    this.InitializeBannerAds();
#endif
                }
                this.InitializeInterstitialAds();
                this.InitializeRewardedAds();
            }

            public void TimeScaleOff() {
                this._onDisplayTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                
                Debug.Log($"ondisplaytimescale time scale in TimeScaleOff {_onDisplayTimeScale} , actual time scale {Time.timeScale}");
            }

            public void TimeScaleOn() {
                Time.timeScale = this._onDisplayTimeScale;

                if (Time.timeScale == 0)
                {
                    Time.timeScale = 1;
                }
                Debug.Log($"ondisplaytimescale time scale in TimeScaleOn {_onDisplayTimeScale} , actual time scale {Time.timeScale}");
            }

            public void DisplayGDPR() {
                if (MaxSdk.GetSdkConfiguration().ConsentFlowUserGeography == MaxSdkBase.ConsentFlowUserGeography.Gdpr) {
                    MaxSdk.CmpService.ShowCmpForExistingUser(error => { });
                } else {
                    Application.OpenURL(DATA_PRIVACY_URL);
                }
            }

            private void OnAdRevenuePaidEventFirebase(string adUnitId, MaxSdkBase.AdInfo adInfo) {
#if FIREBASE
                string adUnitIdentifier = adInfo.AdUnitIdentifier;
                string adFormat = adInfo.AdFormat;
                string networkName = adInfo.NetworkName;
                string countryCode = MaxSdk.GetSdkConfiguration().CountryCode;
                double revenue = adInfo.Revenue;

                string dictionaryKey = adFormat + networkName + countryCode;
                bool hasKey = this._tooSmallRevenuesRR.ContainsKey(dictionaryKey);
                if (hasKey) {
                    revenue += this._tooSmallRevenuesRR[dictionaryKey];
                }

                if (revenue >= 0.0001) {
                    if (hasKey) {
                        this._tooSmallRevenuesRR.Remove(dictionaryKey);
                    }

                    var impressionParameters = new[] {
                        new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
                        new Firebase.Analytics.Parameter("ad_source", networkName),
                        new Firebase.Analytics.Parameter("ad_unit_name",adUnitIdentifier),
                        new Firebase.Analytics.Parameter("ad_format", adFormat),
                        new Firebase.Analytics.Parameter("value", revenue),
                        new Firebase.Analytics.Parameter("currency", "USD")
                    };
                    Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
                } else {
                    this._tooSmallRevenuesRR[dictionaryKey] = revenue;
                }
                this.ycManager.dataManager.SetRedRockRevenue(this._tooSmallRevenuesRR);
#endif
            }

            // REWARDED

            public void InitializeRewardedAds() {
                if (this.GetRewardedKey() != "") {
#if !UNITY_EDITOR
                    this._isEnableRewarded = true;
#endif

                    MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => { };
                    MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo) => {
                        this._finishRewarded = true;
                    };
                    MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.rewardedVisible = false;
                        this.TimeScaleOn();
                        this.LoadRewardedAd();
                        this.aRewarded(this._finishRewarded);
                        if ((this.ycManager.ycConfig.rewardedResetDelay && this.rewardedResetDelayOverride != BooleanOverride.False) || this.rewardedResetDelayOverride == BooleanOverride.True) {
                            this._delayInterstitial = this.GetInterstitialDelay();
                        }
                    };
                    MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += (string adUnitId, MaxSdkBase.ErrorInfo errorInfo) => {
                        this.Invoke("LoadRewardedAd", 2f);
                    };
                    MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) => {
                        this.LoadRewardedAd();
                    };
                    MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.rewardedVisible = true;
                        this.TimeScaleOff();
                        this.ycManager.analyticsManager.RewardedShow();
                        this.ycManager.dataManager.IncrementRewardedsNb();
                    };
                    MaxSdkCallbacks.Rewarded.OnAdClickedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.ycManager.analyticsManager.RewardedClick();
                    };
                    MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.ycManager.analyticsManager.GetSession().ads_rewarded.Add(this.OnAdRevenuePaidEvent(adUnitId, adInfo));
                    };
                    MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += this.OnAdRevenuePaidEventFirebase;
                    this.LoadRewardedAd();
                }
            }

            private void LoadRewardedAd() {
#if AMAZON_APS
                if (this.GetAmazonRewardedKey() != "" && isRewardedFirstLoad) {
                    isRewardedFirstLoad = false;
#if !UNITY_EDITOR
                    this.rewardedAdRequest = new AmazonAds.APSVideoAdRequest(320, 480, this.GetAmazonRewardedKey());
#endif
                    if (this.rewardedAdRequest != null) {
                        this.rewardedAdRequest.onSuccess += (adResponse) => {
                            MaxSdk.SetRewardedAdLocalExtraParameter(this.GetRewardedKey(), "amazon_ad_response", adResponse.GetResponse());
                            MaxSdk.LoadRewardedAd(this.GetRewardedKey());
                        };
                        this.rewardedAdRequest.onFailedWithError += (adError) => {
                            MaxSdk.SetRewardedAdLocalExtraParameter(this.GetRewardedKey(), "amazon_ad_error", adError.GetAdError());
                            MaxSdk.LoadRewardedAd(this.GetRewardedKey());
                        };
                        this.rewardedAdRequest.LoadAd();
                    } else {
                        MaxSdk.LoadRewardedAd(this.GetRewardedKey());
                    }
                } else {
                    MaxSdk.LoadRewardedAd(this.GetRewardedKey());
                }
#else
                MaxSdk.LoadRewardedAd(this.GetRewardedKey());
#endif
            }

            public bool IsRewardBasedVideo() {
#if UNITY_EDITOR
                return true;
#else
                return this._isEnableRewarded == true && MaxSdk.IsRewardedAdReady(this.GetRewardedKey());
#endif
            }

            public bool ShowRewarded(Action<bool> action) {
                this.aRewarded = action;
                if (this.IsRewardBasedVideo()) {
                    this._finishRewarded = false;
                    MaxSdk.ShowRewardedAd(this.GetRewardedKey());
                    return true;
                }
                action(false);
                return false;
            }

            float GetInterstitialDelay() {
                if (this.delayInterstitialOverride >= 0) {
                    return this.delayInterstitialOverride;
                } else {
                    return this.ycManager.ycConfig.InterstitialInterval;
                }
            }


            // INTERSTITIAL

            public void InitializeInterstitialAds() {
                if (this.GetInterstitialKey() != "") {
                    this._isEnableInterstitial = true;
                    this._delayInterstitial = this.GetInterstitialDelay();
                    MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => { };
                    MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (string adUnitId, MaxSdkBase.ErrorInfo errorInfo) => {
                        this.Invoke("LoadInterstitial", 2f);
                    };
                    MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) => {
                        this.LoadInterstitial();
                    };
                    MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.interstitialVisible = false;
                        this.TimeScaleOn();
                        this.LoadInterstitial();
                        this.iInterstitial.gameObject.SetActive(false);
                        this.aInterstitial?.Invoke();
                        this._delayInterstitial = this.GetInterstitialDelay();
                    };
                    MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.interstitialVisible = true;
                        this.TimeScaleOff();
                        this.ycManager.analyticsManager.InterstitialShow();
                        this.ycManager.dataManager.IncrementInterstitialsNb();
                    };
                    MaxSdkCallbacks.Interstitial.OnAdClickedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.ycManager.analyticsManager.InterstitialClick();
                    };
                    MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.ycManager.analyticsManager.GetSession().ads_interstitial.Add(this.OnAdRevenuePaidEvent(adUnitId, adInfo));
                    };
                    MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += this.OnAdRevenuePaidEventFirebase;
                    this.LoadInterstitial();
                }
            }

            private void LoadInterstitial() {
#if AMAZON_APS
                if (this.isInterstitialFirstLoad && this.GetAmazonInterstitialKey() != "") {
                    this.isInterstitialFirstLoad = false;
#if !UNITY_EDITOR
                    this.interstitialAdRequest = new AmazonAds.APSVideoAdRequest(320, 480, this.GetAmazonInterstitialKey());
#endif
                    if (this.interstitialAdRequest != null) {
                        this.interstitialAdRequest.onSuccess += (adResponse) => {
                            MaxSdk.SetInterstitialLocalExtraParameter(this.GetInterstitialKey(), "amazon_ad_response", adResponse.GetResponse());
                            MaxSdk.LoadInterstitial(this.GetInterstitialKey());
                        };
                        this.interstitialAdRequest.onFailedWithError += (adError) => {
                            MaxSdk.SetInterstitialLocalExtraParameter(this.GetInterstitialKey(), "amazon_ad_error", adError.GetAdError());
                            MaxSdk.LoadInterstitial(this.GetInterstitialKey());
                        };
                        this.interstitialAdRequest.LoadAd();
                    } else {
                        MaxSdk.LoadInterstitial(this.GetInterstitialKey());
                    }
                } else {
                    MaxSdk.LoadInterstitial(this.GetInterstitialKey());
                }
#else
                MaxSdk.LoadInterstitial(this.GetInterstitialKey());
#endif
            }

            public bool IsInterstitial(bool force = false) {
                bool cond = this._canDisplayInterstitial && this.IsAdsShow() && this._isEnableInterstitial && (this._delayInterstitial <= 0f || force == true);
#if !UNITY_EDITOR
                if (cond) {
                    return MaxSdk.IsInterstitialReady(this.GetInterstitialKey());
                }
#endif
                return cond;
            }

            void _ShowInterstitial() {
                MaxSdk.ShowInterstitial(this.GetInterstitialKey());
            }

            public bool ShowInterstitial(Action action = null, float delai = -1f, bool force = false) {
                if (this.IsInterstitial(force)) {
                    this.iInterstitial.gameObject.SetActive(delai >= 0f);
                    this.aInterstitial = action;
                    if (delai <= 0f) {
                        this._ShowInterstitial();
                    } else {
                        this.Invoke("_ShowInterstitial", delai);
                    }
                    return true;
                }
                this._canDisplayInterstitial = true;
                action?.Invoke();
                return false;
            }

            // BANNER

            private void SetBannerTransparency() {
                bool baseValue = this.ycManager.ycConfig.TransparentBannerBackground;
                bool setClear = (baseValue && this.bannerTransparencyOverride != BooleanOverride.False) || this.bannerTransparencyOverride == BooleanOverride.True;
                this.iBanner.color = setClear ? Color.clear : Color.white;
            }

            private void InitializeBannerAds() {
                this.SetBannerTransparency();
                this.iBanner.color = this.ycManager.ycConfig.TransparentBannerBackground ? Color.clear : Color.white;
                if (this.GetBannerKey() != "") {
                    this._isEnableBanner = true;
#if AMAZON_APS
                    this.RequestAmazonBanner();
#else
                    this.CreateMaxBanner();
#endif
                    MaxSdkCallbacks.Banner.OnAdClickedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.ycManager.analyticsManager.BannerClick();
                    };
                    MaxSdkCallbacks.Banner.OnAdLoadedEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.ycManager.analyticsManager.BannerShow();
                    };
                    MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (string adUnitId, MaxSdkBase.AdInfo adInfo) => {
                        this.ycManager.analyticsManager.GetSession().ads_banner.Add(this.OnAdRevenuePaidEvent(adUnitId, adInfo));
                    };
                    if (this.ycManager.ycConfig.UseBanners) {
#if UNITY_EDITOR
                        if (this.ycManager.ycConfig.UseBannersInEditor) {
                            this.HideBanner(false);
                        }
#else
                        this.HideBanner(false);
#endif
                    }
                    MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += this.OnAdRevenuePaidEventFirebase;
                }
            }

            private void RequestAmazonBanner() {
#if AMAZON_APS
                if (this.GetAmazonBannerKey() != "") {
                    if (this.bannerAdRequest != null) {
                        this.bannerAdRequest.DestroyFetchManager();
                    }
#if !UNITY_EDITOR
                    this.bannerAdRequest = new AmazonAds.APSBannerAdRequest(320, 50, this.GetAmazonBannerKey());
#endif
                    if (this.bannerAdRequest != null) {
                        this.bannerAdRequest.onFailedWithError += (adError) => {
                            MaxSdk.SetBannerLocalExtraParameter(this.GetBannerKey(), "amazon_ad_error", adError.GetAdError());
                            this.CreateMaxBanner();
                        };
                        this.bannerAdRequest.onSuccess += (adResponse) => {
                            MaxSdk.SetBannerLocalExtraParameter(this.GetBannerKey(), "amazon_ad_response", adResponse.GetResponse());
                            this.CreateMaxBanner();
                        };
                        this.bannerAdRequest.LoadAd();
                    } else {
                        this.CreateMaxBanner();
                    }
                } else {
                    this.CreateMaxBanner();
                }
#endif
            }

            private void CreateMaxBanner() {
                MaxSdk.CreateBanner(this.GetBannerKey(), MaxSdkBase.BannerPosition.BottomCenter);
                MaxSdk.SetBannerPlacement(this.GetBannerKey(), "MY_BANNER_PLACEMENT");
            }

            public void HideBanner(bool hide) {
                if (this._isEnableBanner == true) {
                    if (this.IsAdsShow()) {
                        this._bannerHide = hide;
                        if (hide == true) {
                            MaxSdk.HideBanner(this.GetBannerKey());
                        } else {
                            MaxSdk.ShowBanner(this.GetBannerKey());
                        }
                        this.iBanner.gameObject.SetActive(!hide);
                    } else {
                        MaxSdk.HideBanner(this.GetBannerKey());
                        this.iBanner.gameObject.SetActive(false);
                    }
                } else {
                    this.ycManager.ycConfig.LogWarning("[Ads] not init");
                }
            }

            // ADS
            public bool IsAdsShow() {
                return this.ycManager.dataManager.GetAdsShow();
            }

            public void BuyAdsShow() {
                this.ycManager.dataManager.BuyAdsShow();
                this.HideBanner(true);
            }

            string GetRewardedKey() {
#if UNITY_ANDROID
                return this.ycManager.ycConfig.AndroidRewarded;
#elif UNITY_IOS || UNITY_IPHONE
                return this.ycManager.ycConfig.IosRewarded;
#else
                return "";
#endif
            }

            string GetInterstitialKey() {
#if UNITY_ANDROID
                return this.ycManager.ycConfig.AndroidInterstitial;
#elif UNITY_IOS || UNITY_IPHONE
                return this.ycManager.ycConfig.IosInterstitial;
#else
                return "";
#endif
            }

            string GetBannerKey() {
#if UNITY_ANDROID
                return this.ycManager.ycConfig.AndroidBanner;
#elif UNITY_IOS || UNITY_IPHONE
                return this.ycManager.ycConfig.IosBanner;
#else
                return "";
#endif
            }

            string GetAmazonAppID() {
#if UNITY_ANDROID
                return this.ycManager.ycConfig.AmazonAndroidAppID;
#elif UNITY_IOS || UNITY_IPHONE
                return this.ycManager.ycConfig.AmazonIosAppID;
#else
                return "";
#endif
            }

            string GetAmazonRewardedKey() {
#if UNITY_ANDROID
                return this.ycManager.ycConfig.AmazonAndroidRewarded;
#elif UNITY_IOS || UNITY_IPHONE
                return this.ycManager.ycConfig.AmazonIosRewarded;
#else
                return "";
#endif
            }

            string GetAmazonInterstitialKey() {
#if UNITY_ANDROID
                return this.ycManager.ycConfig.AmazonAndroidInterstitial;
#elif UNITY_IOS || UNITY_IPHONE
                return this.ycManager.ycConfig.AmazonIosInterstitial;
#else
                return "";
#endif
            }

            string GetAmazonBannerKey() {
#if UNITY_ANDROID
                return this.ycManager.ycConfig.AmazonAndroidBanner;
#elif UNITY_IOS || UNITY_IPHONE
                return this.ycManager.ycConfig.AmazonIosBanner;
#else
                return "";
#endif
            }


        }
    }
}