using System.Collections.Generic;
using UnityEngine;

public class PetHouse : MonoBehaviour
{
    public static PetHouse Instance { get; private set; }

    [SerializeField] private List<PetItemPair> itemList;
    [SerializeField] private List<PetItemPair> petsList;

    private Dictionary<PetType, Transform> itemMap = new();
    private Dictionary<PetType, Transform> petsMap = new();

    private PetType currentPet;
    private PetUIUnit currentPetUI;

    private void Awake()
    {
        Instance = this;
        InitializeMaps();
    }

    public void SelectPet(PetType type, PetUIUnit sender)
    {
        SetCurrentPetAttributes(false);
        currentPetUI?.Deselect();
        currentPet = type;
        currentPetUI = sender;
        SetCurrentPetAttributes(true);
        petsMap[currentPet].localPosition = Vector3.zero;
    }

    private void SetCurrentPetAttributes(bool state)
    {
        itemMap[currentPet].gameObject.SetActive(state);
        petsMap[currentPet].gameObject.SetActive(state);
    }

    private void InitializeMaps()
    {
        foreach (var pair in itemList)
        {
            itemMap.Add(pair.type, pair.item);
        }
        foreach (var pair in petsList)
        {
            petsMap.Add(pair.type, pair.item);
        }
    }
}

public enum PetType
{
    dog,
    cat,
    dragon
}

[System.Serializable]
public class PetItemPair
{
    public PetType type;
    public Transform item;
}