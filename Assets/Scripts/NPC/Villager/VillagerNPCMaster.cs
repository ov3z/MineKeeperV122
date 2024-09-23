using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerNPCMaster : MonoBehaviour
{
    public static VillagerNPCMaster Instance { get; private set; }

    private List<VillagerNPC> idleNPCs = new List<VillagerNPC>();
    private List<VillagerNPC> NPCsInQueue = new List<VillagerNPC>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterNpc(VillagerNPC npc)
    {
        idleNPCs.Add(npc);
    }

    public bool isThereAnyIdleNPC() => idleNPCs.Count > 0;

    public VillagerNPC GetIdleNPC()
    {
        if(idleNPCs.Count > 0)
        {
            float closestDistance = Mathf.Infinity;
            VillagerNPC closestNPC = null;

            foreach(var npc in idleNPCs)
            {
                float distance = Vector3.Distance(TradeStation.Instance.transform.position,npc.transform.position);
                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNPC = npc;
                }
            }
            idleNPCs.Remove(closestNPC);
            return closestNPC;
        }

        return null;
    }
}
