using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStockSystem : MonoBehaviour
{
    private List<Transform> disablesBonueses = new();

    private List<Transform> enablesBonueses = new();

    private void Start()
    {
#if !ADS
        gameObject.SetActive(false);
        return;
#endif

        foreach (Transform child in transform)
        {
            disablesBonueses.Add(child);
        }

        StartCoroutine(EnableBonus(10));
    }

    private IEnumerator EnableBonus(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (disablesBonueses.Count > 0)
        {
            var enabledPoint = disablesBonueses[Random.Range(0, disablesBonueses.Count)];

            disablesBonueses.Remove(enabledPoint);

            var rarity = Random.Range(0, 4);
            if (rarity == 0)
            {
                enabledPoint.GetChild(0).gameObject.SetActive(true);
                enabledPoint.GetChild(0).GetComponent<ResourceStock>().OnGet += ReturnToDisabledsList;
            }
            else
            {
                enabledPoint.GetChild(1).gameObject.SetActive(true);
                enabledPoint.GetChild(1).GetComponent<ResourceStock>().OnGet += ReturnToDisabledsList;
            }
        }

        StartCoroutine(EnableBonus(45));
    }

    private void ReturnToDisabledsList(Transform transform)
    {
        disablesBonueses.Add(transform);
    }
}
