using UnityEngine;

public class RewardedItem : MonoBehaviour
{
    private void Start()
    {
#if !ADS
        gameObject.SetActive(false);
#endif
    }
}
