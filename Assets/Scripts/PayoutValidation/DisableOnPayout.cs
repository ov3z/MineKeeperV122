using DG.Tweening;
using UnityEngine;

public class DisableOnPayout : PayoutValidation
{
    [SerializeField] private GuidComponent guidComponent;

    private bool isPayedUp = false;
    private string uniqueID;

    private void Awake()
    {
        uniqueID = guidComponent.GetGuid().ToString();
        isPayedUp = PlayerPrefs.GetInt($"IsBuilded{uniqueID}", 0) == 0 ? false : true;
    }

    private void Start()
    {
        if (isPayedUp)
        {
            foreach (Transform child in transform)
            {
                child.localScale = Vector3.zero;

                if(child.TryGetComponent<Collider>(out var collider))
                {
                    collider.enabled = false;
                }
            }
        }
    }

    public override void OnPayout()
    {
        if (!isPayedUp)
        {
            isPayedUp = true;
            PlayerPrefs.SetInt($"IsBuilded{uniqueID}", 1);
            foreach (Transform child in transform)
            {
                child.localScale = Vector3.zero;
                if (child.TryGetComponent<Collider>(out var collider))
                {
                    collider.enabled = false;
                }
            }
        }
    }
}
