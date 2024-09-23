using System;
using UnityEngine;

public class MouseEventListener : MonoBehaviour
{
    public static MouseEventListener Instance;

    public static event Action OnGetMouseButtonDown;
    public static event Action OnGetMouseButtonUp;
    public static event Action OnGetMouseButton;
    public static event Action OnGetMouseButtonUnhold;
    
    private bool areMouseEventAvailable = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
    }

    private void StopMouseEventService()
    {
        areMouseEventAvailable = false;
    }

    void Update()
    {
        /*
        Debug.Log(areMouseEventAvailable +$" are mouse eventavailable , time scale {Time.timeScale}" );
        */
        if (areMouseEventAvailable && Time.timeScale > 0)
        {
            if (Input.GetMouseButtonDown(0))
                OnGetMouseButtonDown?.Invoke();
            if (Input.GetMouseButton(0))
                OnGetMouseButton?.Invoke();
            if (!Input.GetMouseButton(0))
                OnGetMouseButtonUnhold?.Invoke();
            if (Input.GetMouseButtonUp(0))
                OnGetMouseButtonUp?.Invoke();
        }
    }

    private void OnDestroy()
    {
        OnGetMouseButtonDown = () => { };
        OnGetMouseButtonUp = () => { };
        OnGetMouseButton = () => { };
        OnGetMouseButtonUnhold = () => { };
    }
}
