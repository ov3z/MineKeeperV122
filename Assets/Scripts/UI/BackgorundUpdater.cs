using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgorundUpdater : MonoBehaviour
{
    [SerializeField] private Transform portraitBG;
    [SerializeField] private Transform landscapeBG;


    private bool IsLandscape => Screen.width > Screen.height ? true : false;

    void Update()
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        if(IsLandscape)
        {
            if(!landscapeBG.gameObject.activeSelf)
            {
                landscapeBG.gameObject.SetActive(true);    
                portraitBG.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!portraitBG.gameObject.activeSelf)
            {
                landscapeBG.gameObject.SetActive(false);
                portraitBG.gameObject.SetActive(true);
            }
        }
#endif
    }
}
