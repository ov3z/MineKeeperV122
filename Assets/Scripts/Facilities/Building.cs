using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildOnPayout buildOnPayout;
    [SerializeField] private VillagerNPC npcPrefab;

    [SerializeField] private List<Transform> npcCells = new List<Transform>();


    private void Awake()
    {
        if (buildOnPayout)
            buildOnPayout.OnPayoutComplete += SpawnNPCWithDelay;
        else
            StartCoroutine(SpawnNPCWithDelayCoroutine());
    }

    private void SpawnNPCWithDelay()
    {
        StartCoroutine(SpawnNPCWithDelayCoroutine());
        buildOnPayout.OnPayoutComplete -= SpawnNPCWithDelay;
    }

    private IEnumerator SpawnNPCWithDelayCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnNpc();
    }

    private void SpawnNpc()
    {
        for (int i = 0; i < 2; i++)
        {
            VillagerNPC villagerNPC = Instantiate(npcPrefab);
            villagerNPC.GetComponent<NavMeshAgent>().Warp(transform.TransformPoint(Vector3.zero));
            villagerNPC.SetHome(npcCells[i]);
            villagerNPC.ReturnHome();

            VillagerNPCMaster.Instance.RegisterNpc(villagerNPC);
        }
    }
}
