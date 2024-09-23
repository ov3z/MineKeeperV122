using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static PlayerController;

public class SkinManager : MonoBehaviour
{
    [SerializeField] private ScrollRect maleSkins;
    [SerializeField] private ScrollRect femaleSkins;

    [SerializeField] private Button maleSkinsButton;
    [SerializeField] private Button femaleSkinsButton;

    [SerializeField] private Sprite maleEnabledIcon;
    [SerializeField] private Sprite femaleEnabledIcon;
    [SerializeField] private Sprite maleDisabledIcon;
    [SerializeField] private Sprite femaleDisabledIcon;


    private void Awake()
    {
        maleSkinsButton.onClick.AddListener(EnableMaleSkins);
        femaleSkinsButton.onClick.AddListener(EnableFemaleSkins);
    }

    private void Start()
    {
        var skinGender = PlayerPrefs.GetInt("SelectedSkin", PlayerPrefs.GetInt("Gender", 0) * 10);
        var selectedGender = (PlayerGender)(skinGender / 10);
        ToggleSkins(selectedGender, true);
    }


    private void EnableMaleSkins()
    {
        ToggleSkins(PlayerGender.Male, false);
    }

    private void EnableFemaleSkins()
    {
        ToggleSkins(PlayerGender.Female, false);
    }

    private void ToggleSkins(PlayerGender gender, bool isFromStart)
    {
        var isMale = gender == PlayerGender.Male;

        maleSkins.gameObject.SetActive(isMale);
        femaleSkins.gameObject.SetActive(!isMale);

        if (isMale)
        {
            maleSkinsButton.image.sprite = maleEnabledIcon;
            femaleSkinsButton.image.sprite = femaleDisabledIcon;
        }
        else
        {
            maleSkinsButton.image.sprite = maleDisabledIcon;
            femaleSkinsButton.image.sprite = femaleEnabledIcon;
        }
    }
}
