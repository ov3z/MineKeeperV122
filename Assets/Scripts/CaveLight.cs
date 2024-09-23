using UnityEngine;

public class CaveLight : MonoBehaviour
{
    [SerializeField] private Transform otherCavesLight;
    private void Start()
    {
        if (gameObject.activeInHierarchy)
            otherCavesLight.gameObject.SetActive(false);
    }
}
