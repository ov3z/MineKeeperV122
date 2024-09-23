using DG.Tweening;
using UnityEngine;

public class LevitateAnimation : MonoBehaviour
{
    [SerializeField] private float deltaY = 0.05f;
    [SerializeField] private float ascendTime = 1.0f;
    [SerializeField] private float angularVelocity = 150;

    private void Start()
    {
        transform.DOMoveY(transform.position.y + deltaY, ascendTime).SetEase(Ease.InSine).OnComplete(() =>
        {
            LevitateAnim();
        });
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward * angularVelocity * Time.deltaTime);
    }

    private void LevitateAnim()
    {
        transform.DOMoveY(transform.position.y - 2 * deltaY, 2 * ascendTime).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            transform.DOMoveY(transform.position.y + 2 * deltaY, 2 * ascendTime).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                LevitateAnim();
            });
        });
    }
}
