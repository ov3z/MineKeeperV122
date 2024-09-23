using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ScrollController : MonoBehaviour
{
    public static Action<float> OnScrollValueChanged;

    [SerializeField] private UpgradePanel panel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private float unitPerSecond = 0.5f;

    [SerializeField] private Button leftShift;
    [SerializeField] private Button rightShift;

    [SerializeField] private List<SkinSelectionUnit> skins;

    private Tween scrollReleaseTween;
    private Tween scrollToTheEndTween;
    private Coroutine startResealseCoroutine;

    private float currentStepping;
    private List<float> steppingValues = new();

    private int ItemCount => content.childCount - 1;
    private float Stepping => 1f / ItemCount;

    private bool didInitiazlize;

    private int SelectedSkinData
    {
        get
        {
            return PlayerPrefs.GetInt("SelectedSkin", PlayerPrefs.GetInt("Gender", 0) * 10);
        }
    }

    private float HorizontalScroll
    {
        get => PlayerPrefs.GetFloat("HorPro", -1);
        set => PlayerPrefs.SetFloat("HorPro", value);
    }

    private void Awake()
    {
        InitiliazeSteppingValue();

        panel.OnClose += ResetInitialization;
    }

    private void InitiliazeSteppingValue()
    {
        for (int i = 0; i <= ItemCount; i++)
        {
            steppingValues.Add(i * Stepping);
        }
    }

    private void ResetInitialization()
    {
        didInitiazlize = false;
    }

    private void OnEnable()
    {
        MouseEventListener.OnGetMouseButtonDown += OnMouseButtonDown;
        MouseEventListener.OnGetMouseButtonUp += OnMOuseButtonUp;

        scrollRect.onValueChanged.AddListener(FireScrollValueChanged);

        var gender = SelectedSkinData / 10;
        var skinIndex = SelectedSkinData % 10;

        if (transform.GetSiblingIndex() == gender)
        {
            if (!didInitiazlize)
            {
                didInitiazlize = true;

                AnimateScrollHoriz(steppingValues[skinIndex]);
                HorizontalScroll = steppingValues[skinIndex];
            }
            else
                scrollRect.horizontalNormalizedPosition = HorizontalScroll;
        }
        else
        {
            scrollRect.horizontalNormalizedPosition = HorizontalScroll;
        }
    }

    private void OnMouseButtonDown()
    {
        if (scrollToTheEndTween != null || scrollReleaseTween != null)
            return;

        if (startResealseCoroutine != null)
            StopCoroutine(startResealseCoroutine);

        scrollReleaseTween?.Kill();
    }

    private void OnMOuseButtonUp()
    {
        if (scrollToTheEndTween != null || scrollReleaseTween != null)
            return;

        startResealseCoroutine = StartCoroutine(ScrollRectReleaseCoroutine());
    }

    private void FireScrollValueChanged(Vector2 value)
    {
        OnScrollValueChanged?.Invoke(value.x);
        UpdateShiftButton(value.x);
        HorizontalScroll = value.x;
    }

    public void UpdateShiftButton(float value)
    {
        if (value <= 0.01f)
        {
            if (leftShift.transform.gameObject.activeSelf)
                leftShift.transform.gameObject.SetActive(false);

            if (!rightShift.transform.gameObject.activeSelf)
                rightShift.transform.gameObject.SetActive(true);
        }
        else if (value >= 0.99f)
        {
            if (rightShift.transform.gameObject.activeSelf)
                rightShift.transform.gameObject.SetActive(false);

            if (!leftShift.transform.gameObject.activeSelf)
                leftShift.transform.gameObject.SetActive(true);
        }
        else
        {
            if (!rightShift.transform.gameObject.activeSelf)
                rightShift.transform.gameObject.SetActive(true);

            if (!leftShift.transform.gameObject.activeSelf)
                leftShift.transform.gameObject.SetActive(true);
        }
    }

    private IEnumerator ScrollRectReleaseCoroutine()
    {
        yield return null;

        var position = scrollRect.horizontalNormalizedPosition;
        var scrollTarget = GetClosestPosition(position);

        AnimateScrollHoriz(scrollTarget);
    }

    private void AnimateScrollHoriz(float endValue)
    {
        var tweenDuration = Mathf.Abs(endValue - scrollRect.horizontalNormalizedPosition) / unitPerSecond;

        scrollReleaseTween = scrollRect.DOHorizontalNormalizedPos(endValue, tweenDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            scrollReleaseTween = null;

            var selectedSkin = steppingValues.IndexOf(endValue);
            skins[selectedSkin].EnableButtons();

        }).OnUpdate(() =>
        {
            FireScrollValueChanged(scrollRect.normalizedPosition);
        });
    }

    public void ScrollToTheEnd()
    {
        if (scrollToTheEndTween == null)
        {
            scrollToTheEndTween = scrollRect.DOHorizontalNormalizedPos(1f, 1f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                scrollToTheEndTween = null;
            }).OnUpdate(() =>
            {
                FireScrollValueChanged(scrollRect.normalizedPosition);
            });
        }
    }

    private float GetClosestPosition(float currentValue)
    {
        return steppingValues.OrderBy(item => Math.Abs(currentValue - item)).First();
    }

    public void ShiftBy(int direction)
    {
        var position = scrollRect.horizontalNormalizedPosition;
        var scrollTarget = GetClosestPosition(position);
        var currentIndex = steppingValues.IndexOf(scrollTarget);

        var targetIndex = Mathf.Clamp(currentIndex + direction, 0, steppingValues.Count - 1);
        AnimateScrollHoriz(steppingValues[targetIndex]);
    }

    private void OnDisable()
    {
        MouseEventListener.OnGetMouseButtonDown -= OnMouseButtonDown;
        MouseEventListener.OnGetMouseButtonUp -= OnMOuseButtonUp;
        scrollRect.onValueChanged.RemoveListener(FireScrollValueChanged);
        OnScrollValueChanged = (value) => { };
    }
}
