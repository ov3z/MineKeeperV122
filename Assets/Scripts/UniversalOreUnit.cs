using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalOreUnit : MonoBehaviour
{
    private void Awake()
    {
        int stoneSize = Random.Range(0, 3);
        int stoneType = /*Random.Range(0, 3)*/0;

        Transform stoneSizeParent = transform.GetChild(stoneSize);
        stoneSizeParent.gameObject.SetActive(true);
        Transform stoneTypeParent = stoneSizeParent.GetChild(stoneType);
        stoneTypeParent.gameObject.SetActive(true);
    }
}
