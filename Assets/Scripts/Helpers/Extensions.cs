using UnityEngine;

public static class Extensions
{
    public static bool HasChildren(this Transform parent) => parent.childCount > 0;
    public static bool ActiveSelf(this Transform transform) => transform.gameObject.activeSelf;
    public static Transform GetLastChild(this Transform transform) => transform.GetChild(transform.childCount - 1);
    public static void ResetRelativeParent(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;
    }
    public static int GetActiveChildrenCount(this Transform transform)
    {
        int activeChildrenCount = 0;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                activeChildrenCount++;
        }
        return activeChildrenCount;
    }

    public static void CopyRectTransform(Transform target, Transform source)
    {
        try
        {
            if (target.TryGetComponent<RectTransform>(out var targetRect) && !target.CompareTag("DontUpdateScale"))
            {
                source.TryGetComponent<RectTransform>(out var sourceRect);
                targetRect.localPosition = sourceRect.localPosition;
                targetRect.localRotation = sourceRect.localRotation;
                targetRect.localScale = sourceRect.localScale;
                targetRect.anchorMin = sourceRect.anchorMin;
                targetRect.anchorMax = sourceRect.anchorMax;
                targetRect.anchoredPosition = sourceRect.anchoredPosition;
                targetRect.sizeDelta = sourceRect.sizeDelta;
                targetRect.pivot = sourceRect.pivot;
            }
            for (int i = 0; i < target.childCount; i++)
            {
                CopyRectTransform(target.GetChild(i), source.GetChild(i));
            }
        }
        catch
        {
            Debug.LogError($"target {target + " " + source}", target);
            Debug.LogError($"source {target + " " + source}", source);
        }
    }
}
