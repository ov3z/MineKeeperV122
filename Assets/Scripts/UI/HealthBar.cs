using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image barFill;
    [SerializeField] private Transform barVisual;
    [SerializeField] private TextMeshProUGUI barText;
    [SerializeField] private float appearTimeMax = 1f;

    private float initialHealth;
    private float currentHealth;
    private IDamageable owner;
    private Coroutine disapearRoutine;

    private void Awake()
    {
        owner = GetComponentInParent<IDamageable>();
        owner.OnHealthChange += UpdateHealthBar;
    }

    private void UpdateHealthBar(float value)
    {
        barFill.fillAmount = value;

        if (disapearRoutine != null)
        {
            StopCoroutine(disapearRoutine);
        }

        disapearRoutine = StartCoroutine(ShowBarRoutine());

        if (value == 0)
        {
            StopCoroutine(disapearRoutine);
            gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowBarRoutine()
    {
        yield return barVisual.DOScale(1, 0.1f).SetEase(Ease.Linear);

        yield return new WaitForSeconds(appearTimeMax);

        yield return barVisual.DOScale(0, 0.1f).SetEase(Ease.Linear);
    }
}
