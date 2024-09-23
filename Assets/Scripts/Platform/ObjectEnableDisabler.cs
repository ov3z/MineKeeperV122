using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectEnableDisabler : MonoBehaviour
{
    [SerializeField] private bool webGL;
    [SerializeField] private bool android;
    [SerializeField] private bool iOS;

    private void OnValidate()
    {
#if UNITY_WEBGL
        gameObject.SetActive(webGL);
#endif

#if UNITY_ANDROID
        gameObject.SetActive(android);
#endif

#if UNITY_IOS
        gameObject.SetActive(iOS);
#endif
    }
}
