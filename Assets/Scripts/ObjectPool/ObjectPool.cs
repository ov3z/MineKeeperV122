using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private PoolableObject objectPrefab;
    private int poolSize;

    public List<PoolableObject> pool;

    private ObjectPool(PoolableObject objectPrefab, int poolSize)
    {
        this.objectPrefab = objectPrefab;
        this.poolSize = poolSize;

        pool = new List<PoolableObject>(poolSize);
    }

    public static ObjectPool CreatePool(PoolableObject objectPrefab, int size)
    {
        ObjectPool pool = new ObjectPool(objectPrefab, size);
        GameObject poolObject = new GameObject(objectPrefab.name + "Pool");

        pool.CreateObjects(poolObject.transform, size);
        return pool;
    }

    private void CreateObjects(Transform parentTransfrom, int count)
    {
        for (int i = 0; i < count; i++)
        {
            PoolableObject poolableObject = GameObject.Instantiate(objectPrefab, Vector3.zero, Quaternion.identity, parentTransfrom);
            poolableObject.parent = this;
            poolableObject.gameObject.SetActive(false);
            pool.Add(poolableObject);
        }
    }

    public void ReturnObjectToPool(PoolableObject poolableObject)
    {
        pool.Add(poolableObject);
    }

    public PoolableObject GetObject()
    {
        if (pool.Count > 0)
        {
            PoolableObject instance = pool[0];

            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf)
                {
                    instance = pool[i];
                    pool.RemoveAt(i);
                    break;
                }
            }
            return instance;
        }
        else
        {
            return null;
        }
    }
}
