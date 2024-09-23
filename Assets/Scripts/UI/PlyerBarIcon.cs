using UnityEngine;
using UnityEngine.UI;

public class PlyerBarIcon : MonoBehaviour
{
    [SerializeField] private Image playerIcon;
    [SerializeField] private Sprite girlMiner;

    private void Start()
    {
        if (PlayerPrefs.GetInt("Gender", 0) == 1)
        {
            playerIcon.sprite = girlMiner;
        }
    }
}
