using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight object pooling system for efficient spawning/reusing of GameObjects.
/// Supports predefined pools (via serialized list) and dynamic pool creation for unlisted prefabs.
/// Helps avoid garbage collection spikes from frequent Instantiate/Destroy calls.
/// </summary>
public class ObjectPoolLite : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for easy access from other scripts (e.g., ObjectSpawner).
    /// </summary>
    public static ObjectPoolLite Instance { get; private set; }

    [System.Serializable]
    public struct Pool
    {
        public GameObject prefab;  
        public int size;           
    }

    [SerializeField] private List<Pool> pools = new List<Pool>();  

    private Dictionary<GameObject, Queue<GameObject>> _dict;
    private Dictionary<GameObject, Transform> _parents;

    /// <summary>
    /// Initializes the singleton and builds all predefined pools on Awake.
    /// Destroys duplicates if multiple instances exist.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Build();  
    }

    /// <summary>
    /// Builds the internal dictionaries and creates initial instances for predefined pools.
    /// Instantiates inactive objects, adds PooledObjectLite components, and organizes under parent GameObjects.
    /// </summary>
    private void Build()
    {
        _dict = new Dictionary<GameObject, Queue<GameObject>>();  
        _parents = new Dictionary<GameObject, Transform>();      

        foreach (Pool p in pools)
        {
            // Skip invalid pools (null prefab or negative size)
            if (p.prefab == null || p.size < 0) continue;

            Transform parent = new GameObject("Pool_" + p.prefab.name).transform;
            parent.SetParent(transform);  
            _parents[p.prefab] = parent;

            // Create a queue and pre-instantiate 'size' inactive objects
            Queue<GameObject> q = new Queue<GameObject>(p.size);
            for (int i = 0; i < p.size; i++)
            {
                // Instantiate under parent, deactivate immediately
                GameObject go = Instantiate(p.prefab, parent);
                go.SetActive(false);
                
                // Ensure each instance has a PooledObjectLite component for tracking
                PooledObjectLite po = go.GetComponent<PooledObjectLite>();
                if (po == null) po = go.AddComponent<PooledObjectLite>();
                po.Init(this, p.prefab);  
                
                q.Enqueue(go);  
            }
            _dict[p.prefab] = q;  
        }
    }

    /// <summary>
    /// Spawns (activates and positions) an instance of the given prefab.
    /// Reuses from pool if available; creates a new one if pool is empty.
    /// Dynamically creates a pool if the prefab isn't predefined.
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) { Debug.LogError("Spawn prefab is null"); return null; }

        Queue<GameObject> q;
        if (!_dict.TryGetValue(prefab, out q))
        {
            // Dynamic pool creation: If prefab not predefined, create a new pool on-the-fly
            Transform parent = new GameObject("Pool_" + prefab.name + "_Dynamic").transform;
            parent.SetParent(transform);

            if (_parents == null) _parents = new Dictionary<GameObject, Transform>();
            _parents[prefab] = parent;

            if (_dict == null) _dict = new Dictionary<GameObject, Queue<GameObject>>();
            q = new Queue<GameObject>();  
            _dict[prefab] = q;
        }

        // Dequeue if available, or create an extra instance
        GameObject go = (q.Count > 0) ? q.Dequeue() : CreateExtra(prefab);
        go.transform.SetPositionAndRotation(pos, rot);  
        go.SetActive(true);  
        return go;
    }

    /// <summary>
    /// Creates an extra instance for a pool when the queue is empty.
    /// Used internally by Spawn() for overflow.
    /// Ensures the new instance has a parent and PooledObjectLite component.
    /// </summary>
    private GameObject CreateExtra(GameObject prefab)
    {
        Transform parent;
        if (!_parents.TryGetValue(prefab, out parent))
        {
            parent = new GameObject("Pool_" + prefab.name + "_Auto").transform;
            parent.SetParent(transform);
            _parents[prefab] = parent;
        }

        GameObject go = Instantiate(prefab, parent);
        PooledObjectLite po = go.GetComponent<PooledObjectLite>();
        if (po == null) po = go.AddComponent<PooledObjectLite>();
        po.Init(this, prefab);  
        go.SetActive(false);    
        return go;
    }

    /// <summary>
    /// Despawns (deactivates and returns to pool) the given instance.
    /// Uses PooledObjectLite to identify the correct queue.
    /// If no valid pool or component, destroys the object instead.
    /// </summary>
    /// <param name="instance">The active GameObject to return to the pool.</param>
    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        // Get the tracking component
        PooledObjectLite po = instance.GetComponent<PooledObjectLite>();
        if (po == null || po.PrefabKey == null || !_dict.ContainsKey(po.PrefabKey))
        {
            // Fallback: Destroy if not pooled (e.g., manually instantiated)
            Destroy(instance);
            return;
        }

        instance.SetActive(false);
        _dict[po.PrefabKey].Enqueue(instance);
    }
}
