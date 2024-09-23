using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FPSCOunter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private float updateTimer;
    private float upadteTimerMax = 0.5f;

    private List<float> fpss = new List<float>();

    private void Update()
    {
        updateTimer += Time.deltaTime;
        fpss.Add(1 / Time.deltaTime);
        if (updateTimer >= upadteTimerMax)
        {
            int avgFPS = Mathf.RoundToInt(fpss.Sum() / fpss.Count);
            fpss.Clear();
            text.text = $"FPS: {avgFPS}";
            updateTimer= 0;
        }
    }
}
