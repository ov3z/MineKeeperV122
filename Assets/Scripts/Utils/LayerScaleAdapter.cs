using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class LayerScaleAdapter : MonoBehaviour
{
    [SerializeField] private RectTransform Canvas;
    [SerializeField] private bool AdaptInUpdate = false;

    private RectTransform _rect;

    private IEnumerator Start()
    {
        _rect = GetComponent<RectTransform>();
        Adapt();
        yield return new WaitForSeconds(1);

        Adapt();
    }

    private void Adapt()
    {
        Vector2 size = new Vector2(1080, 2400);
        Vector2 rect = GetSizes(size, Canvas.sizeDelta);

        float x = rect.x / _rect.sizeDelta.x;
        float y = rect.y / _rect.sizeDelta.y;

        float unit = Mathf.Min(x, y);
        transform.localScale = new Vector3(unit, unit, unit);

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
            return GetSizesVertical(size, max);
        }
        return GetSizesHorizontal(size, max);
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
