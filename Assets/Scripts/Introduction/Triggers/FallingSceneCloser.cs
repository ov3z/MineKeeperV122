using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class FallingSceneCloser : MonoBehaviour
{
    [SerializeField] private RectTransform cutoutMask;

    private AsyncOperation opeartion;
    private float progress;

    private void OnTriggerEnter(Collider other)
    {
        HidePlayer();
    }

    private void HidePlayer()
    {
        QuestController.Instance.SkipCurrentQuest();
        PlayerPrefs.SetInt("IsFirstLoad", 1);
        PlayerPrefs.SetInt("IntroductionPart", 0);
        cutoutMask.GetChild(0).gameObject.SetActive(false);
        cutoutMask.GetChild(0).gameObject.SetActive(true);
        cutoutMask.DOSizeDelta(Vector2.zero, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            SceneManager.LoadScene(1);
        });
    }

    private IEnumerator LoadNextScene()
    {
        yield return null;
        opeartion = SceneManager.LoadSceneAsync(1);
        opeartion.allowSceneActivation = false;
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        progress = 0;

        while (progress < 89)
        {
            progress = (int)(opeartion.progress * 100);
            yield return null;
        }
        opeartion.allowSceneActivation = true;
    }
}
