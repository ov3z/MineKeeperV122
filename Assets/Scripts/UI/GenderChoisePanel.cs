#if UNITY_WEBGL || PLATFORM_WEBGL
using CrazyGames;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenderChoisePanel : MonoBehaviour
{
    [SerializeField] private List<GenderContext> outlines;

    private Dictionary<CharacterGenderTypes, GenderContext> genderContextMap = new();

    private CharacterGenderTypes selectedGender;

    private void Awake()
    {
        foreach (var item in outlines)
        {
            genderContextMap.Add(item.gender, item);
        }
        selectedGender = CharacterGenderTypes.Male;
    }
    private void Start()
    {
#if UNITY_WEBGL || PLATFORM_WEBGL
        CrazySDK.Instance.GameplayStart();
#endif
    }

    public void ChooseCharacter(int characterIndex)
    {
        var gender = (CharacterGenderTypes)characterIndex;

        SetGenderContext(selectedGender, false);
        selectedGender = gender;
        SetGenderContext(selectedGender, true);
    }


    private void SetGenderContext(CharacterGenderTypes gender, bool flag)
    {
        genderContextMap[selectedGender].outline.gameObject.SetActive(flag);
        foreach (var character in genderContextMap[selectedGender].characters)
        {
            character.gameObject.SetActive(flag);
        }
        genderContextMap[selectedGender].button.enabled = !flag;
    }

    public void LoadIntroBeginning()
    {
        PlayerPrefs.SetInt("IsFromCave", 0);
        PlayerPrefs.SetInt("IsFromInitialLoadScene", 0);
        PlayerPrefs.SetInt("ChoseGender", 1);
        PlayerPrefs.SetInt("Gender", (int)selectedGender);
        CaveTraveller.Instance.LoadCaveLoadingScene();
        
       // YsoCorp.GameUtils.YCManager.instance.OnGameStarted(PlayerLevelManager.Instance.Level);//for yso game start
    }

    [System.Serializable]
    public class GenderContext
    {
        public Transform outline;
        public List<Transform> characters;
        public Button button;
        public CharacterGenderTypes gender;
    }
}

public enum CharacterGenderTypes
{
    Male,
    Female
}
