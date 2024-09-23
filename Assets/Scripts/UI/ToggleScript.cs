using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ToggleScript : MonoBehaviour
{
    [SerializeField] private Sprite turnedOnBackground;
    [SerializeField] private Sprite turnedOffBackground;

    [SerializeField] private Image background;
    [SerializeField] private RectTransform toggleTransform;
    [SerializeField] private ToggleFunction toggleFunction;

    private bool isTurnedOn = true;
    private Tween toggleHandleMotionTween;

    private void Start()
    {
        LoadToggleState();
        ToggleNeededFunction();
        UpdateToggleVisauls();
    }

    private void LoadToggleState()
    {
        isTurnedOn = PlayerPrefs.GetInt($"{(int)toggleFunction}State", 1) == 1 ? true : false;
    }

    private void SaveToggleState()
    {
        if (isTurnedOn)
            PlayerPrefs.SetInt($"{(int)toggleFunction}State", 1);
        else
            PlayerPrefs.SetInt($"{(int)toggleFunction}State", 0);
    }


    public void Toggle()
    {
        isTurnedOn = !isTurnedOn;
        SaveToggleState();
        ToggleNeededFunction();
        UpdateToggleVisauls();

        SoundManager.Instance.Play(SoundTypes.Button);
    }

    private void ToggleNeededFunction()
    {
        switch (toggleFunction)
        {
            case ToggleFunction.Haptics:
                ToggleHaptics();
                break;
            case ToggleFunction.Music:
                ToggleMusic();
                break;
            case ToggleFunction.Sound:
                ToggleSound();
                break;
            case ToggleFunction.Quality:
                ToggleQuality();
                break;
        }
    }
    private void ToggleHaptics()
    {
        HapticManager.Instance.SetHapticsActive(isTurnedOn);
    }

    private void ToggleMusic()
    {
        SoundManager.Instance.ToggleMusic(!isTurnedOn);
    }

    private void ToggleSound()
    {
        SoundManager.Instance.ToggleSound(!isTurnedOn);
    }

    private void ToggleQuality()
    {
        if (isTurnedOn)
            QualityManager.Instance.SetHighSettings();
        else
            QualityManager.Instance.SetLowSettings();
    }

    private void UpdateToggleVisauls()
    {
        if (isTurnedOn)
        {
            background.sprite = turnedOnBackground;
            toggleHandleMotionTween?.Kill();
            toggleHandleMotionTween = toggleTransform.DOAnchorPosX(45, 0.3f);
        }
        else
        {
            background.sprite = turnedOffBackground;
            toggleHandleMotionTween?.Kill();
            toggleHandleMotionTween = toggleTransform.DOAnchorPosX(-45, 0.3f);
        }
    }

    public enum ToggleFunction
    {
        Haptics,
        Music,
        Sound,
        Quality
    }
}
