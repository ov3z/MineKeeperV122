using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextIntroPart : MonoBehaviour
{
    [SerializeField] private int introIndex;
    [SerializeField] private bool tickIsFromCave;
    [SerializeField] private bool loadCloudScene;

    private bool hasntLoadYet = true;

    private void OnTriggerEnter(Collider other)
    {
        if (hasntLoadYet)
        {
            hasntLoadYet = false;

            PlayerPrefs.SetInt("IsFromCave", tickIsFromCave ? 1 : 0);
            PlayerPrefs.SetInt("IsFromInitialLoadScene", tickIsFromCave ? 1 : 0);
            PlayerPrefs.SetInt("IntroductionPart", introIndex);

            if (loadCloudScene)
                CaveTraveller.Instance.LoadCloudScene();
            else
                CaveTraveller.Instance.LoadCaveLoadingScene();

            if (introIndex == 0)
            {
                QuestController.Instance.SkipCurrentQuest();
                Debug.Log($"intro scene started {introIndex}");
            }
            if (introIndex == 3 || introIndex == 2)
            {
                QuestController.Instance.SkipCurrentQuest();
                Debug.Log($"intro scene is continuing {introIndex}");
            }
        }
    }
}
