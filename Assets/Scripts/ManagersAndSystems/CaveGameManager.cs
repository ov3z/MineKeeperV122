using System;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
#if USING_GA
using GameAnalyticsSDK;
#endif   

public class CaveGameManager : MonoBehaviour
{
    public static CaveGameManager Instance { get; private set; }

    public event Action OnAttackStart;
    public event Action OnAnyEnemyDeath;
    public event Action OnLevelCleared;
    public event Action OnPlayerLose;

    [SerializeField] private Transform caves;
    [SerializeField] private NavMeshSurface navMesh;
    [SerializeField] private CinemachineConfiner confiner;
    [SerializeField] private List<Collider> cinemachineCollidersForCave;
    [SerializeField] private List<Vector3> levelPositions;
    [SerializeField] private Transform winPanel;
    [SerializeField] private Transform loosePanel;
    [SerializeField] private Transform tutorialText;
    [SerializeField] private Transform levels;
    [SerializeField] private Transform stars;
    [SerializeField] private Transform[] doors;

    public int activeCaveIndex;
    private int levelCave;
    private int level;
    private Transform activeCave;
    private Vector3 playerInitialPosition;
    private Vector3 playerInitialEulerAngles;

    private List<IDamageable> enemies = new List<IDamageable>();
    private List<Ore> ores = new List<Ore>();
    private List<SoldierController> soldiers = new List<SoldierController>();
    private Dictionary<ResourceTypes, float> resourceInitialAmount = new Dictionary<ResourceTypes, float>();

    private bool isAttacking;

    private int xpRewardForThisLevel => 30 + 5 * (level / 4);
    private int coinRewardForThisLevel => 20 + 5 * level;

    private NavMeshData navMeshData;
    private bool isReplayLevel;


    private void Awake()
    {
        Instance = this;

        var replayLevel = PlayerPrefs.GetInt("ReplayedLevel", -1);
        if (replayLevel >= 0)
        {
            levelCave = replayLevel;
            isReplayLevel = true;
        }
        else
        {
            levelCave = PlayerPrefs.GetInt("LevelCave", 0);
        }

        level = PlayerPrefs.GetInt("Level", 1);
        
    }

    public void RespawnPlayer()
    {
        PlayerController.Instance.transform.SetPositionAndRotation(playerInitialPosition, Quaternion.Euler(playerInitialEulerAngles));
        PlayerController.Instance.gameObject.SetActive(true);
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        playerInitialPosition = PlayerController.Instance.transform.position;
        playerInitialEulerAngles = PlayerController.Instance.transform.eulerAngles;

        foreach (ResourceTypes type in Enum.GetValues(typeof(ResourceTypes)))
        {
            resourceInitialAmount.Add(type, ResourceStorage.Instance.GetResourceBalance(type));
        }

        CaveLevelUI.Instance.SetLevelText(levelCave + 1);
        

        ActivateCave();

#if USING_GA
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, $"Cave Level {levelCave}");
#endif

        navMesh.BuildNavMesh();
        navMeshData = navMesh.navMeshData;
    }

    private void ActivateCave()
    {
        activeCaveIndex = levelCave % 6;
        activeCave = caves.GetChild(activeCaveIndex);
        activeCave.gameObject.SetActive(true);

        navMesh.BuildNavMesh();

        levels.SetParent(activeCave);
        levels.localPosition = Vector3.zero;

        var currentCaveLevel = levels.GetChild(activeCaveIndex);
        currentCaveLevel.gameObject.SetActive(true);

        int layoutIndex = Mathf.Clamp(levelCave / 6, 0, 1);
        var activeLayout = currentCaveLevel.GetChild(layoutIndex);
        activeLayout.gameObject.SetActive(true);

        int currentLevel = levelCave;

        if (currentLevel >= 10)
            currentLevel = Random.Range(0, 10);

        confiner.m_BoundingVolume = cinemachineCollidersForCave[activeCaveIndex];
    }

    public void RegisterEnemy(IDamageable enemy)
    {
        enemies.Add(enemy);

        (enemy as EnemyController).OnDeath += ((IDamageable damageable) =>
        {
            OnAnyEnemyDeath?.Invoke();
        });
    }

    public void DiscardEnemy(IDamageable enemy)
    {
        enemies.Remove(enemy);

        if (enemies.Count == 0)
            CaveLevelUI.Instance.DisableCountdown();
    }

    public IDamageable GetClosestEnemyToThePoint(Vector3 pointPos)
    {
        IDamageable closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(pointPos, enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    public void AddOre(Ore ore)
    {
        ores.Add(ore);
    }

    public void RemoveOre(Ore ore)
    {
        ores.Remove(ore);
    }

    public Ore GetClosestOreToPoint(Vector3 pointPos)
    {
        Ore closestOre = null;
        float closestDistance = float.MaxValue;

        foreach (var ore in ores)
        {
            float distance = Vector3.Distance(pointPos, ore.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestOre = ore;
            }
        }

        return closestOre;
    }

    public void FireOnAttackStart()
    {
        if (!isAttacking)
        {
            OnAttackStart?.Invoke();
            isAttacking = true;
        }
    }

    public float GetResourceChange(ResourceTypes type)
    {
        return ResourceStorage.Instance.GetResourceBalance(type) - resourceInitialAmount[type];
    }

    public void ShowWinPanel()
    {
        if (!winPanel.gameObject.activeSelf)
        {
            stars.gameObject.SetActive(false);
            int recievedStars = PlayerPrefs.GetInt("Stars", 0);
            float starsForCurrentLevelNorm = (float)recievedStars / 3;
            if (starsForCurrentLevelNorm > 0)
            {
#if UNITY_WEBGL || PLATFORM_WEBGL
                CrazyGames.CrazyEvents.Instance.HappyTime();
#endif

                winPanel.gameObject.SetActive(true);
                int xpReward = Mathf.RoundToInt(xpRewardForThisLevel * starsForCurrentLevelNorm);
                PlayerLevelManager.Instance.AddXP(xpReward);

#if USING_GA
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, $"Cave Level {levelCave}", recievedStars);
#endif
            }
            else
            {
                loosePanel.gameObject.SetActive(true);
                Debug.Log("lose panel");
#if USING_GA
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, $"Cave Level {levelCave}");
#endif
            }
            ResourceStorage.Instance.ChangeResourceAmount(ResourceTypes.Coins, coinRewardForThisLevel);

            if (!isReplayLevel)
                PlayerPrefs.SetInt("LevelCave", ++levelCave);
        }
    }

    public void AddSoldier(SoldierController soldier)
    {
        soldiers.Add(soldier);
    }

    public void RemoveSoldier(SoldierController soldier)
    {
        soldiers.Remove(soldier);
        if (soldiers.Count == 0)
        {
            //CaveLevelUI.Instance.EnableCountdown();
        }
    }

    public void RebakeNavmesh()
    {
        navMeshData = navMesh.navMeshData;
        navMesh.UpdateNavMesh(navMeshData);
    }

    public int GetEnemyCount() => enemies.Count;

    public void FireOnLevelCleared()
    {
        OnLevelCleared?.Invoke();
    }

    public void FireOnPlayerLose()
    {
        OnPlayerLose?.Invoke();
    }
}
