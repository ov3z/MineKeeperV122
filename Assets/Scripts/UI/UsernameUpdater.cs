using TMPro;
using UnityEngine;

public class UsernameUpdater : MonoBehaviour
{
    private TextMeshProUGUI username;

    private void Start()
    {
        username = transform.GetComponent<TextMeshProUGUI>();
        UpdateUsername();
    }

    public void UpdateUsername()
    {
        username.text = PlayerPrefs.GetString("UserName", "Miner");
    }

}
