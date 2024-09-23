using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadVideoPlayer : MonoBehaviour
{
    [SerializeField] private Image darkness;

    [SerializeField] private AsyncSceneLoader sceneLoader;
    [SerializeField] private List<VideoPlayUnit> units;

    private Dictionary<int, VideoPlayUnit> unitsMap = new();

    private void Start()
    {
        foreach (var unit in units)
            unitsMap.Add((int)unit.Gender, unit);

        int gender = PlayerPrefs.GetInt("Gender", 0);

        unitsMap[gender].Enable();


        StartCoroutine(VideoLoaderCoroutine());
    }

    private IEnumerator VideoLoaderCoroutine()
    {
        yield return null;
        darkness.DOFade(0, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        {
            sceneLoader.gameObject.SetActive(true);
        });
    }

    [Serializable]
    public class VideoPlayUnit
    {
        public CharacterGenderTypes Gender;
        public Image output;

        public void Enable()
        {
            output.gameObject.SetActive(true);
        }
    }
}
