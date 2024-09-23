using UnityEngine;
using System;

namespace YsoCorp {
    namespace GameUtils {
        [DefaultExecutionOrder(-20)]
        public class YCManager : BaseManager {

            public static YCManager instance;

            #region Static Field
            public static string VERSION = "0.1.0";

            public bool HasGameStarted { get; private set; } = false;
            #endregion

            public YCConfig ycConfig;

            public ABTestingManager abTestingManager { get; set; }
            public AdsManager adsManager { get; set; }
            public AnalyticsManager analyticsManager { get; set; }
            public FBManager fbManager { get; set; }
            public GdprManager gdprManager { get; set; }
            public I18nManager i18nManager { get; set; }
            public InAppManager inAppManager { get; set; }
            public MmpManager mmpManager { get; set; }
            public NoInternetManager noInternetManager { get; set; }
            public RequestManager requestManager { get; set; }
            public SettingManager settingManager { get; set; }
            public YCDataManager dataManager { get; set; }

            private void Awake() {
                if (instance != null) {
                    DestroyImmediate(this.gameObject);
                } else {
                    instance = this;
                    this.ycManager = this;
                    DontDestroyOnLoad(this.gameObject);
                    if (IsTestBuild() == false) {
                        this.ycConfig.activeLogs = 0;
                        this.ycConfig.ABDebugLog = false;
                        this.ycConfig.InappDebug = false;
                    }
                    Debug.Log("[GameUtils] YCManager : Initialize !");
                }
            }

            private void Start() {
                this.ycManager.dataManager.IncrementPlayerLaunchCount();
                this.CheckMmpLaunchCountEvent();
            }

            public static bool IsTestBuild() {
                return Application.installMode == ApplicationInstallMode.Editor || Application.installMode == ApplicationInstallMode.DeveloperBuild;
            }

            public bool IsFirstTimeAppLaunched() {
                return this.ycManager.dataManager.GetPlayerLaunchCount() == 1;
            }

            public int GetPlayerLaunchCount() {
                return this.ycManager.dataManager.GetPlayerLaunchCount();
            }

            private void CheckMmpLaunchCountEvent() {
                if (this.GetPlayerLaunchCount() == 20) {
                    this.mmpManager.OnMmpsInit.AddListener(() => this.mmpManager.SendEvent("20_game_launch"));
                }
            }

            public void OnGameStarted(int level) {
                if (this.HasGameStarted == false) {
                    this.HasGameStarted = true;
                    this.analyticsManager.OnGameStarted(level);
                    this.ycManager.dataManager.IncrementPlayerGameCount();
                }
            }

            public void OnGameFinished(bool levelComplete, float score = 0f) {
                if (this.HasGameStarted == true) {
                    this.HasGameStarted = false;
                    this.analyticsManager.OnGameFinished(levelComplete, score);
                    this.dataManager.IncrementLevelPlayed(levelComplete);
                }
            }

        }
    }
}
