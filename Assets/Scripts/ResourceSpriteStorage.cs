using System.Collections.Generic;
using UnityEngine;

public class ResourceSpriteStorage : MonoBehaviour
{
    public static ResourceSpriteStorage Instance { get; private set; }

    [SerializeField] private List<ResourceTypeIconPair> iconPairs;

    private Dictionary<ResourceTypes, Sprite> iconsMap = new Dictionary<ResourceTypes, Sprite>();

    private void Awake()
    {
        Instance = this;
        InitializeIconsMap();
    }

    private void InitializeIconsMap()
    {  
        foreach (var pair in iconPairs)
            iconsMap.Add(pair.Type, pair.Icon);
    }

    public Sprite GetIcon(ResourceTypes type) => iconsMap[type];

    [System.Serializable]
    public class ResourceIconPair
    {
        public ResourceTypes Type;
        public Sprite Icon;
    }
}
