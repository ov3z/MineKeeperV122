using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using YsoCorp.GameUtils;
#if UNITY_WEBGL || PLATFORM_WEBGL
using CrazyGames;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<DamageEffect> OnFireSwordPick;

    [SerializeField] Transform caveEntrance;
    [SerializeField] ParticleSystem unlockSmoke;
    [SerializeField] RectTransform initiaPanel;
    [SerializeField] DialogueController dialogueController;

    [SerializeField] private UpgradePanel townHallPanel;
    [SerializeField] private UpgradePanel playerUpgradePanel;
    [SerializeField] private UpgradePanel petPanel;

    [SerializeField] private bool shouldInitializePlayerPos = true;

    private Dictionary<ResourceTypes, List<Plant>> plantMap = new Dictionary<ResourceTypes, List<Plant>>();
    private Dictionary<ResourceTypes, List<Ore>> oreMap = new Dictionary<ResourceTypes, List<Ore>>();

    private bool isPlayerUpgradePanelOpened;
    private bool isTownHallPanelOpened;
    private bool isPetPanelOpnened;
    private bool isFireSwordPicked;

    public event Action OnTownHallPanelOpen;
    public event Action OnTownHallPanelClose;
    public event Action OnPlayerUpgradePanleOpen;
    public event Action OnPlayerUpgradePanleClose;
    public event Action OnPetPanelOpen;
    public event Action OnPetPanelClose;

    public bool IsInCave => CaveGameManager.Instance != null;
    public bool IsPlayerUpgradePanelOpened => isPlayerUpgradePanelOpened;
    public bool IsTownHallPanelOpened => isTownHallPanelOpened;
    public bool IsPetPanelOpnened => isPetPanelOpnened;
    public bool IsFireSwordPicked => isFireSwordPicked;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        CrazySDK.Instance.GameplayStart();
#endif

        Application.targetFrameRate = 60;
        PlayerPrefs.SetInt("IsFromCave", IsInCave ? 1 : 0);

        bool isFirstLoad = PlayerPrefs.GetInt("IsFromInitialLoadScene", 1) == 1 ? true : false;
        bool isRightAfterIntro = PlayerPrefs.GetInt("IsFirstLoad", 0) == 1 ? true : false;

        if (!isRightAfterIntro && !IsInCave && shouldInitializePlayerPos)
        {
            PlayerController.Instance.transform.SetParent(caveEntrance);
            PlayerController.Instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
        }
        else if (initiaPanel != null && isRightAfterIntro)
        {
            StartCoroutine(EnableBlurPanel());
            PlayerController.Instance.LieDown();
            PlayerPrefs.SetInt("IsFirstLoad", 0);
        }

        if (townHallPanel)
        {
            townHallPanel.OnOpen += SetTowneHallPanelOpened;
            townHallPanel.OnClose += SetTowneHallPanelClosed;
        }
        if (playerUpgradePanel)
        {
            playerUpgradePanel.OnOpen += SetPlayerUpgradePanelOpened;
            playerUpgradePanel.OnClose += SetPlayerUpgradePanelClosed;
        }
        if (petPanel)
        {
            petPanel.OnOpen += SetPetPanelOpned;
            petPanel.OnClose += SetPetPanelClosed;
        }

        if (IsInCave)
        {
            SoundManager.Instance.PlayAmbientMusic(SoundTypes.Fight);
        }
        else
        {
            SoundManager.Instance.PlayAmbientMusic(SoundTypes.Village);
        }

        PlayerPrefs.SetInt("IsFromInitialLoadScene", 0);
    }

    private IEnumerator EnableBlurPanel()
    {
        initiaPanel.gameObject.SetActive(true);
        yield return null;

        initiaPanel.sizeDelta = Vector2.zero;
        initiaPanel.DOSizeDelta(Vector2.one * 30, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            initiaPanel.gameObject.SetActive(false);
            dialogueController.EnableBlurPanel();
        });
    }

    public void AddPlant(ResourceTypes type, Plant plant)
    {
        if (!plantMap.ContainsKey(type))
        {
            plantMap.Add(type, new List<Plant>());
        }
        plantMap[type].Add(plant);
    }

    public void RemovePlant(ResourceTypes type, Plant plant)
    {
        if (plantMap.ContainsKey(type))
            plantMap[type].Remove(plant);
    }

    public Plant GetClosestPlant(Vector3 requestPosition, ResourceTypes type)
    {
        float minDistance = Mathf.Infinity;
        Plant closestPlant = null;
        foreach (var plant in plantMap[type])
        {
            float currentDistance = Vector3.Distance(requestPosition, plant.transform.position);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                closestPlant = plant;
            }
        }
        return closestPlant;
    }

    public ParticleSystem GetUnlockSmoke()
    {
        return unlockSmoke;
    }

    public void SetTowneHallPanelOpened()
    {
        isTownHallPanelOpened = true;
        OnTownHallPanelOpen?.Invoke();
    }
    public void SetTowneHallPanelClosed()
    {
        isTownHallPanelOpened = false;
        OnTownHallPanelClose?.Invoke();
    }
    public void SetPlayerUpgradePanelOpened()
    {
        isPlayerUpgradePanelOpened = true;
        OnPlayerUpgradePanleOpen?.Invoke();
    }
    public void SetPlayerUpgradePanelClosed()
    {
        isPlayerUpgradePanelOpened = false;
        OnPlayerUpgradePanleClose?.Invoke();
    }
    public void SetPetPanelOpned()
    {
        isPetPanelOpnened = true;
        OnPetPanelOpen?.Invoke();
    }

    public void SetPetPanelClosed()
    {
        isPetPanelOpnened = false;
        OnPetPanelClose?.Invoke();
    }

    public void GetFireSword()
    {
        isFireSwordPicked = true;
        OnFireSwordPick?.Invoke(DamageEffect.Fire);
    }

    private void OnDestroy()
    {

    }
}