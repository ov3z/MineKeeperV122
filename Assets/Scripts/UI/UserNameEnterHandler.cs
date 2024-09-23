using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserNameEnterHandler : MonoBehaviour
{
    public static UserNameEnterHandler Instance;

    public event Action OnNameSubmit;

    [SerializeField] private UsernameUpdater usernameUpdater;
    [SerializeField] private bool EnableOnStartup;

    private Transform inputField;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        inputField = transform.GetChild(0);

        if (EnableOnStartup)
        {
            EneableInputField();
        }
    }

    public void OnNameEnter(string name)
    {
        if (name != "")
        {
            PlayerPrefs.SetString("UserName", name);
            usernameUpdater.UpdateUsername();
            DisableInputField();
            OnNameSubmit?.Invoke();
        }
    }

    public void EneableInputField()
    {
        inputField.gameObject.SetActive(true);
        inputField.GetComponentInChildren<InputField>().ActivateInputField();
    }

    public void DisableInputField()
    {
        inputField.gameObject.SetActive(false);
    }
}
