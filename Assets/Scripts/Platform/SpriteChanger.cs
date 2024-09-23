using UnityEngine;
using UnityEngine.UI;

public class SpriteChanger : MonoBehaviour
{
    [SerializeField] private Sprite webGlSprite;
    [SerializeField] private Sprite androidSprite;
    [SerializeField] private Sprite iOSSprite;

    private void OnValidate()
    {
        var image = GetComponent<Image>();

#if UNITY_WEBGL
        image.sprite = webGlSprite;
#endif

#if UNITY_ANDROID
        image.sprite = androidSprite;
#endif

#if UNITY_IOS
        image.sprite = iOSSprite;
#endif
    }
}
