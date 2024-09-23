using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CaveSoldierGround : MonoBehaviour
{
    [SerializeField] SoldierType soldierType;
    [SerializeField] Transform soldierGround;
    [SerializeField] SoldierController soldierPrefab;

    private int soldierGrounTier;
    private int soldierCount;

    private void Start()
    {
        bool isSoldierUnlocked = PlayerPrefs.GetInt($"{(int)soldierType}Unlocked", 0) == 1 ? true : false;
        if (isSoldierUnlocked)
        {
            soldierGrounTier = PlayerPrefs.GetInt($"Ground{(int)soldierType}Tier", 0);
            soldierCount = PlayerPrefs.GetInt($"Soldier{(int)soldierType}", 0);

            Transform activeSoldierGround = soldierGround.GetChild(soldierGrounTier);
            activeSoldierGround.gameObject.SetActive(true);

            for (int i = 0; i < soldierCount; i++)
            {
                SoldierController soldierInstance = Instantiate(soldierPrefab, activeSoldierGround.GetChild(i));

                soldierInstance.GetComponent<NavMeshAgent>().Warp(activeSoldierGround.GetChild(i).TransformPoint(Vector3.zero));
                soldierInstance.transform.localEulerAngles = new Vector3(0, 90, -90);
                soldierInstance.transform.parent = null;
            }
        }
    }
}
