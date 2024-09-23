
#if USING_GA
using GameAnalyticsSDK;
#endif
using UnityEngine;

#if USING_GA
public class GAInit : MonoBehaviour, IGameAnalyticsATTListener
#else
public class GAInit : MonoBehaviour
#endif
{
    private void Start()
    {
#if USING_GA
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GameAnalytics.RequestTrackingAuthorization(this);
        }
        else
        {
            GameAnalytics.Initialize();
        }
#endif
    }

    public void GameAnalyticsATTListenerNotDetermined()
    {
#if USING_GA
        GameAnalytics.Initialize();
#endif
    }
    public void GameAnalyticsATTListenerRestricted()
    {
#if USING_GA
        GameAnalytics.Initialize();
#endif
    }
    public void GameAnalyticsATTListenerDenied()
    {
#if USING_GA
        GameAnalytics.Initialize();
#endif
    }
    public void GameAnalyticsATTListenerAuthorized()
    {
#if USING_GA
        GameAnalytics.Initialize();
#endif
    }
}
