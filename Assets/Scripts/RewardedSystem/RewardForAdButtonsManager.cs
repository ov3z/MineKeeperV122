using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RewardForAdButtonsManager : MonoBehaviour
{
    [SerializeField] private UpgradePanel parentPanel;
    [SerializeField] private Button adButtonPrefab;

    private List<ICoinTradeItem> _buyButtons = new();
    private Action _pendingReward;

    private ICoinTradeItem _selectedTradeable;

    private Dictionary<ICoinTradeItem, CoinTradeUISetup> _tradeUISetupsMap = new();

    private Button _buttonToBeReplaced;

    private float _timeToOfferAppear;
    private float _timeToOfferAppearMax = 5;

    private float _lastUsageTime;
    private bool canAffordUpgarde;

    private List<Button> _adButtonsPool = new();

    private Coroutine _enableAdButtonsRoutine;

    private void Awake()
    {
        //parentPanel.OnOpen += OnParentPanelOpen;
        //parentPanel.OnClose += OnParentPanelClose;
    }

    private void OnParentPanelOpen()
    {
        if (Time.time > _lastUsageTime + _timeToOfferAppearMax)
        {
            if (_enableAdButtonsRoutine == null)
            {
                _enableAdButtonsRoutine = StartCoroutine(EnbableAdButtonsRoutine());
            }
        }
    }

    private IEnumerator EnbableAdButtonsRoutine()
    {
#if !ADS
        yield break;
#endif

        yield return null;

        _buyButtons.Clear();
        _buyButtons.AddRange(GetComponentsInChildren<ICoinTradeItem>().Where(button => button.IsUnlocked));

        canAffordUpgarde = true;
        foreach (var item in _buyButtons)
        {
            canAffordUpgarde &= item.Price <= ResourceStorage.Instance.GetResourceBalance(ResourceTypes.Coins);
        }

        if (!canAffordUpgarde)
        {
            _tradeUISetupsMap.Clear();

            for (int i = 0; i < _buyButtons.Count; i++)
            {
                _selectedTradeable = _buyButtons[i];

                Button adButton;

                if (_adButtonsPool.Count > 0)
                {
                    adButton = _adButtonsPool[0];
                    _adButtonsPool.RemoveAt(0);
                }
                else
                {
                    adButton = Instantiate(adButtonPrefab);
                }

                adButton.gameObject.SetActive(true);
                adButton.onClick.RemoveAllListeners();

                int instantInex = i;
                adButton.onClick.AddListener(() => { ShowRewardedAd(_buyButtons[instantInex]); });

                _buttonToBeReplaced = _selectedTradeable.BuyButton;

                adButton.transform.parent = _buttonToBeReplaced.transform.parent;
                adButton.transform.localPosition = _buttonToBeReplaced.transform.localPosition;
                adButton.transform.localScale = adButtonPrefab.transform.localScale;

                _tradeUISetupsMap.Add(_selectedTradeable, new CoinTradeUISetup
                {
                    Adbutton = adButton,
                    ButtonToBeReplaced = _buttonToBeReplaced,
                    SelectedTradeable = _selectedTradeable
                });

                Debug.Log(_tradeUISetupsMap.Keys.ToArray());

                _buttonToBeReplaced.gameObject.SetActive(false);
            }
        }

        yield return new WaitUntil(() => _tradeUISetupsMap.Keys.Count == 0);

        _enableAdButtonsRoutine = null;
    }

    private void ShowRewardedAd(ICoinTradeItem tradeItem)
    {
        Debug.Log(tradeItem);

        _pendingReward += () => { OnRewardedGetting(tradeItem); };

        //AdsContainer.Instance.ShowRewarded(_pendingReward);
        AdsContainer.Instance.ShowRewardedYso(_pendingReward);//yso rewarded
        _lastUsageTime = Time.time;

        Debug.Log(_pendingReward);
    }

    private void OnRewardedGetting(ICoinTradeItem tradeItem)
    {
        Debug.Log($"get rewarded {tradeItem}");

        _pendingReward = () => { };

        var selectedSetup = _tradeUISetupsMap[tradeItem];

        selectedSetup.Adbutton.gameObject.SetActive(false);
        tradeItem.BuyButton.gameObject.SetActive(true);

        _adButtonsPool.Add(selectedSetup.Adbutton);

        tradeItem.Aquire();
    }

    private void OnParentPanelClose()
    {
        if (!canAffordUpgarde)
        {
            foreach (var item in _buyButtons)
            {
                _tradeUISetupsMap[item].ButtonToBeReplaced.gameObject.SetActive(true);

                var adButton = _tradeUISetupsMap[item].Adbutton;

                adButton.onClick.RemoveAllListeners();
                adButton.gameObject.SetActive(false);

                if (!_adButtonsPool.Contains(adButton))
                    _adButtonsPool.Add(adButton);
            }

            _tradeUISetupsMap.Clear();
        }
    }

    public class CoinTradeUISetup
    {
        public ICoinTradeItem SelectedTradeable;
        public Button Adbutton;
        public Button ButtonToBeReplaced;
    }
}
