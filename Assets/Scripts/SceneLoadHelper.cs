using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadHelper : MonoBehaviour
{
    public static void LoadSceneWithVisuals(int sceneNumber)
    {
        PlayerPrefs.SetInt("SceneToLoad", sceneNumber);
        SceneManager.LoadScene(2);
    }
}
