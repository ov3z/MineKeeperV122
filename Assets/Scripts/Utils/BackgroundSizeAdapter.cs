using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class BackgroundSizeAdapter : MonoBehaviour
{
    [SerializeField] private bool isAdaptedWithScale;

    [SerializeField] private RectTransform Canvas;
    [SerializeField] private bool AdaptInUpdate = false;

    private RectTransform _rect;
    private Image _img;

    private IEnumerator Start()
    {
        _rect = GetComponent<RectTransform>();
        _img = GetComponent<Image>();
        Adapt();
        yield return new WaitForSeconds(1);

        Adapt();
    }

    private void Adapt()
    {
        Vector2 size = new Vector2(_img.sprite.rect.width, _img.sprite.rect.height);
        Vector2 rect = GetSizes(size, Canvas.sizeDelta);

        if (isAdaptedWithScale)
        {
            var scaleFactor = rect.x / _rect.sizeDelta.x;
            _rect.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }
        else
        {
            _rect.sizeDelta = rect;
        }
    }

    private void Update()
    {
        if (AdaptInUpdate)
        {
            Adapt();
        }
    }


    private Vector2 GetSizes(Vector2 size, Vector2 max)
    {
        float xAspect = max.x / size.x;
        float yAspect = max.y / size.y;
        if (xAspect > yAspect)
        {
            return GetSizesHorizontal(size, max);
        }
        return GetSizesVertical(size, max);
    }

    private Vector2 GetSizesHorizontal(Vector2 size, Vector2 max)
    {
        Vector2 res = new();

        float rY = max.x / size.x;
        res.x = (int)(size.x * rY);
        res.y = (int)(size.y * rY);
        // res.y = (mHeight - res.height) / 2;

        return res;
    }

    private Vector2 GetSizesVertical(Vector2 size, Vector2 max)
    {
        Vector2 res = new();

        float rY = max.y / size.y;
        res.x = (int)(size.x * rY);
        res.y = (int)(size.y * rY);
        // res.y = (mHeight - res.height) / 2;

        return res;
    }

}
