using UnityEngine;

public static class ExtensionMethods 
{
    public static float ClampNormalized(this float value) => Mathf.Clamp01(value);
}
