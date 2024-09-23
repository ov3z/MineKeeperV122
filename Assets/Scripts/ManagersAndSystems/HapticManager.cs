
#if UNITY_ANDROID || PLATFORM_ANDROID
using MoreMountains.NiceVibrations;
#endif
using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void PlayHaptics(HapticIntensity intensity)
    {
        switch (intensity)
        {
            case HapticIntensity.Heavy:
#if UNITY_ANDROID || PLATFORM_ANDROID
                MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
#endif
#if UNITY_IOS || PLATFORM_IOS
                IOSNative.StartHapticFeedback(HapticFeedbackTypes.HEAVY);
#endif
                break;
            case HapticIntensity.Medium:
#if UNITY_ANDROID || PLATFORM_ANDROID
                MMVibrationManager.Haptic(HapticTypes.MediumImpact);
#endif
#if UNITY_IOS || PLATFORM_IOS
                IOSNative.StartHapticFeedback(HapticFeedbackTypes.MEDIUM);
#endif
                break;
            case HapticIntensity.Light:
#if UNITY_ANDROID || PLATFORM_ANDROID
                MMVibrationManager.Haptic(HapticTypes.LightImpact);
#endif
#if UNITY_IOS || PLATFORM_IOS
                IOSNative.StartHapticFeedback(HapticFeedbackTypes.LIGHT);
#endif
                break;
        }
    }

    public void SetHapticsActive(bool isActive)
    {
#if UNITY_ANDROID || PLATFORM_ANDROID
        MMVibrationManager.SetHapticsActive(isActive);
#endif
#if UNITY_IOS || PLATFORM_IOS
        IOSNative.SetHaptics(isActive);
#endif
    }
}

public enum HapticIntensity
{
    Heavy,
    Medium,
    Light
}
