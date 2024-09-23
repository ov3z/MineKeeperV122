using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [SerializeField] private int disappearLevel = 5;
    [SerializeField] private bool shouldSupportReplay;

    private void Start()
    {
        var currentCaveIndex = PlayerPrefs.GetInt("LevelCave", 0);
        var replayLevel = PlayerPrefs.GetInt("ReplayedLevel", -1);
        if (currentCaveIndex >= disappearLevel)
        {
            gameObject.SetActive(false);

            if ((replayLevel >= 0 && shouldSupportReplay))
            {
                gameObject.SetActive(true);
            }
        }
    }
}
