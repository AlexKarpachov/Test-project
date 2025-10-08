using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    [System.Serializable]
    public class Pool
    {
        public string prefabTag;
        public GameObject prefab;
        public int poolSize;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.prefabTag, objectPool);
        }
    }

    // Retrieves an object from the pool and ensures it is ready to use.
    public GameObject GetObject(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"Pool with tag {tag} doesn't exist!");
            return null;
        }

        Queue<GameObject> objectPool = poolDictionary[tag];

        GameObject objectToSpawn;

        if (objectPool.Count == 0)
        {
            objectToSpawn = Instantiate(pools.Find(p => p.prefabTag == tag).prefab);
        }
        else
        {
            objectToSpawn = objectPool.Dequeue();
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    // Returns an object to the pool.
    public void ReturnObject(GameObject obj, string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            return;
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }
}
