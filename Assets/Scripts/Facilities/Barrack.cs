using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrack : MonoBehaviour
{
    [SerializeField] Transform soldierGround;
    [SerializeField] Transform spawnPoint;
    [SerializeField] SoldierController soldierPrefab;
    [SerializeField] BuildOnPayout payoutValidation;
    [SerializeField] SoldierType soldierType;

    private int soldierGroundTier;
    private int spawnedSoldiers;
    private float spawnDelay = 4f;
    private Transform activeSoldierGround;
    private Coroutine spawnCoroutine;
    private List<SoldierController> spawnedSoldiersList = new List<SoldierController>();

    private bool IsFirstSpawn
    {
        get
        {
            return PlayerPrefs.GetInt($"Barrack{(int)soldierType}", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt($"Barrack{(int)soldierType}", value ? 1 : 0);
        }
    }

    private void Awake()
    {
        soldierGroundTier = PlayerPrefs.GetInt($"Ground{(int)soldierType}Tier", 0);
    }

    private void Start()
    {
        payoutValidation.OnPayoutComplete += OnPayout;
        SoldierUpgradeManager.Instance.OnSoldierUpgrade += OnSoldierPlaceUpgrade;
    }

    private void OnSoldierPlaceUpgrade(SoldierType soldier)
    {
        if (soldier == soldierType)
        {
            foreach (var soldierInsance in spawnedSoldiersList)
            {
                Destroy(soldierInsance.gameObject);
            }

            spawnedSoldiersList.Clear();
            spawnedSoldiers = 0;

            activeSoldierGround.gameObject.SetActive(false);
            soldierGroundTier = PlayerPrefs.GetInt($"Ground{(int)soldierType}Tier", 0);

            EnableSoldierGround();

            StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnRoutineFirstTime());
        }
    }

    private void OnPayout()
    {
        EnableSoldierGround();

        if (IsFirstSpawn)

        {
            spawnCoroutine = StartCoroutine(SpawnRoutineFirstTime());
            IsFirstSpawn = false;
        }
        else
        {
            spawnCoroutine = StartCoroutine(SpawnNotFirstTime());
        }

        PlayerPrefs.SetInt($"Ground{(int)soldierType}Tier", soldierGroundTier);
        PlayerPrefs.SetInt($"{(int)soldierType}Unlocked", 1);
    }

    private void EnableSoldierGround()
    {
        activeSoldierGround = soldierGround.GetChild(soldierGroundTier);
        activeSoldierGround.gameObject.SetActive(true);
    }

    private IEnumerator SpawnRoutineFirstTime()
    {
        yield return new WaitForSeconds(spawnDelay);
        for (int i = spawnedSoldiers; i < activeSoldierGround.childCount; i++)
        {
            yield return null;
            SpawnSoldier(i);
        }
    }

    private IEnumerator SpawnNotFirstTime()
    {
        for (int i = spawnedSoldiers; i < activeSoldierGround.childCount; i++)
        {
            SpawnSoldier(i);

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnSoldier(int i)
    {
        spawnedSoldiers++;

        SoldierController soldierInstance = Instantiate(soldierPrefab);
        soldierInstance.transform.position = spawnPoint.position;
        soldierInstance.transform.localScale = Vector3.one * 0.75f;
        soldierInstance.HomeCell = activeSoldierGround.GetChild(i);

        spawnedSoldiersList.Add(soldierInstance);
    }

    private void OnDisable()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        PlayerPrefs.SetInt($"Soldier{(int)soldierType}", spawnedSoldiers);
        SoldierUpgradeManager.Instance.OnSoldierUpgrade -= OnSoldierPlaceUpgrade;
    }
}

public enum SoldierType
{
    Swordsmen,
    Spearmen
}