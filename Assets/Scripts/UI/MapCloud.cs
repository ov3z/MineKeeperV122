using UnityEngine;

public class MapCloud : MonoBehaviour
{
    [SerializeField] private int targetLevel;
    private void Start()
    {
        var level = PlayerPrefs.GetInt("LevelCave", 0);
        if(level >= targetLevel)
        {
            gameObject.SetActive(false);
        }
    }
}
