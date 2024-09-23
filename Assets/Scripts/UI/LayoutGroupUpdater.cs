using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayoutGroupUpdater : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup baseLayoutGroup;
    [SerializeField] private VerticalLayoutGroup portraitLayoutGroup;
    [SerializeField] private VerticalLayoutGroup landscapeLayoutGroup;

    private void Start()
    {
        ScreenManager.Instance.OnScreenChange += UpdateLayoutGroup;
    }

    private void UpdateLayoutGroup(ScreenOrieantation orientation)
    {
        var isLandscape = (Screen.width > Screen.height) ? true : false;

        if(isLandscape)
        {
            baseLayoutGroup.padding.left = landscapeLayoutGroup.padding.left;
            baseLayoutGroup.padding.right = landscapeLayoutGroup.padding.right;
            baseLayoutGroup.padding.top = landscapeLayoutGroup.padding.top;
            baseLayoutGroup.padding.bottom = landscapeLayoutGroup.padding.bottom;
        }
        else
        {
            baseLayoutGroup.padding.left = portraitLayoutGroup.padding.left;
            baseLayoutGroup.padding.right = portraitLayoutGroup.padding.right;
            baseLayoutGroup.padding.top = portraitLayoutGroup.padding.top;
            baseLayoutGroup.padding.bottom = portraitLayoutGroup.padding.bottom;
        }
    }
}
