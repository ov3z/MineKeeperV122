using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseForAd : MonoBehaviour
{
    [SerializeField] private Transform aquireTarget;

    private ICoinTradeItem aquirable;

    private Button adButton;

    private Action OnRewardedSucceed;
    private Action OnRewardedFail;

    private void Awake()
    {
        aquirable = aquireTarget.GetComponent<ICoinTradeItem>();
        adButton = GetComponent<Button>();

        OnRewardedSucceed = () => { aquirable.Aquire(); };
        OnRewardedFail = () => { };

        adButton.onClick.AddListener(ShowAd);
    }

    private void Start()
    {
#if !ADS
        gameObject.SetActive(false);
#endif
    }

    public void ShowAd()
    {
        Debug.Log("show ad");
        //AdsContainer.Instance.ShowRewarded(OnRewardedSucceed, OnRewardedFail);
        AdsContainer.Instance.ShowRewardedYso(OnRewardedSucceed, OnRewardedFail);//yso rewarded
    }

    private void OnDestroy()
    {
        OnRewardedSucceed = () => { aquirable.Aquire(); };
        if (adButton)
        {
            adButton.onClick.RemoveAllListeners();
        }
    }
}
