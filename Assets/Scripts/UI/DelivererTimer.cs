using UnityEngine;
using UnityEngine.UI;

public class DelivererTimer : MonoBehaviour
{
    [SerializeField] private Transform visuals;
    [SerializeField] private Image fill;

    public void Enable(bool state)
    {
        visuals.gameObject.SetActive(state);
    }

    public void UpdateTimer(float progress)
    {
        fill.fillAmount = progress;
    }
}
