using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingSystem : MonoBehaviour
{
    public static PoolingSystem Instance { get; private set; }

    [SerializeField] private PoolableObject pickUIUnit;

    [SerializeField] List<ResourcePoolUnit> collectiblePoolElements = new ();
    [SerializeField] List<ResourcePoolUnit> resourcePoolElements = new ();
    [SerializeField] List<ParticlePoolUnit> particlePoolElements = new ();
    [SerializeField] List<ProjectilePoolUnit> projectilePoolElements = new ();

    private Dictionary<ResourceTypes, ObjectPool> collectiblePoolsMap = new();
    private Dictionary<ResourceTypes, ObjectPool> resourcePoolsMap = new();
    private Dictionary<ProjectileTypes, ObjectPool> projectilePoolsMap = new();
    private Dictionary<string, ObjectPool> animEffectsPoolMap = new();

    private ObjectPool pickUIUnitPool;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (var unit in collectiblePoolElements)
            collectiblePoolsMap.Add(unit.Type, ObjectPool.CreatePool(unit.Prefab, 100));
        foreach (var unit in resourcePoolElements)
            resourcePoolsMap.Add(unit.Type, ObjectPool.CreatePool(unit.Prefab, 200));
        foreach (var unit in particlePoolElements)
            animEffectsPoolMap.Add(unit.EffectAnimKey, ObjectPool.CreatePool(unit.Prefab, 50));
        foreach (var unit in projectilePoolElements)
            projectilePoolsMap.Add(unit.Type, ObjectPool.CreatePool(unit.Prefab, 30));

        pickUIUnitPool = ObjectPool.CreatePool(pickUIUnit, 20);
    }

    public ObjectPool GetCollectiblePool(ResourceTypes type) => collectiblePoolsMap[type];
    public ObjectPool GetResourcePool(ResourceTypes type) => resourcePoolsMap[type];
    public ObjectPool GetParticlePool(string effectKey) => animEffectsPoolMap[effectKey];
    public ObjectPool GetProjectilePool(ProjectileTypes projectileType) => projectilePoolsMap[projectileType];
    public PoolableObject GetPickUIUnit() => pickUIUnitPool.GetObject();
}

[System.Serializable]
public class ResourcePoolUnit
{
    public ResourceTypes Type;
    public PoolableObject Prefab;
}
[System.Serializable]
public class ParticlePoolUnit
{
    public string EffectAnimKey;
    public PoolableObject Prefab;
}
[System.Serializable]
public class ProjectilePoolUnit
{
    public ProjectileTypes Type;
    public PoolableObject Prefab;
}
