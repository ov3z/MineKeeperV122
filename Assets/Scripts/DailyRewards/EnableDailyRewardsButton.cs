using UnityEngine;

public class EnableDailyRewardsButton : MonoBehaviour
{
    private void Awake()
    {
        if (PlayerPrefs.GetInt("AreDailyRewardsEnabled", 0) == 1)
            gameObject.SetActive(true);
    }

    public void EnabledailyRewardButton()
    {
        PlayerPrefs.SetInt("AreDailyRewardsEnabled", 1);
        gameObject.SetActive(true);
    }

    public void DisableDailyRewards()
    {
        PlayerPrefs.SetInt("AreDailyRewardsEnabled", 0);
        gameObject.SetActive(false);
    }
}
