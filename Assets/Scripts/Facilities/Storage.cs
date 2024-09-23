using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    [SerializeField] private BuildOnPayout buildOnPayout;
    [SerializeField] private FarmerNPC npcPrefab;
    [SerializeField] private ResourceStack baseStack;
    [SerializeField] private GuidComponent guidComponent;
    [SerializeField] private List<Transform> spawnedNPCPositions = new List<Transform>();

    private int spawnedNPCCount;
    private string id => guidComponent.GetGuid().ToString();

    private void Awake()
    {
        buildOnPayout.OnPayoutComplete += SpawnAllNPC;
        spawnedNPCCount = PlayerPrefs.GetInt($"SpawnedNPCs{id}", 2);
    }

    private void SpawnAllNPC()
    {
        for (int i = 0; i < spawnedNPCCount; i++)
        {
            SpawnOneNPC(i);
        }
    }

    private void SpawnOneNPC(int NPCIndex)
    {
        FarmerNPC farmerNPC = Instantiate(npcPrefab);
        farmerNPC.transform.position = spawnedNPCPositions[NPCIndex].position;
        farmerNPC.SetHomeCell(spawnedNPCPositions[NPCIndex]);
        farmerNPC.SetBase(baseStack);
    }

    public void UpgaradeNPCCount()
    {
        spawnedNPCCount++;
        PlayerPrefs.SetInt($"SpawnedNPCs{id}", spawnedNPCCount);
        SpawnOneNPC(spawnedNPCCount - 1);
    }
}
