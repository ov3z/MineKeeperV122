using UnityEngine;

public class QualityManager : MonoBehaviour
{
    public static QualityManager Instance { get; private set; }

    [SerializeField] Camera cameraWithStack;
    [SerializeField] Camera cameraWithNoStack;

    private void Awake()
    {
        Instance = this;
    }

    public void SetHighSettings()
    {
        QualitySettings.SetQualityLevel(1);
    }

    public void SetLowSettings()
    {
        QualitySettings.SetQualityLevel(0);
    }
}
