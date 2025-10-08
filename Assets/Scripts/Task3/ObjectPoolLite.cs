using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolLite : MonoBehaviour
{
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

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Build();
    }

    private void Build()
    {
        _dict = new Dictionary<GameObject, Queue<GameObject>>();
        _parents = new Dictionary<GameObject, Transform>();

        foreach (Pool p in pools)
        {
            if (p.prefab == null || p.size < 0) continue;

            Transform parent = new GameObject("Pool_" + p.prefab.name).transform;
            parent.SetParent(transform);
            _parents[p.prefab] = parent;

            Queue<GameObject> q = new Queue<GameObject>(p.size);
            for (int i = 0; i < p.size; i++)
            {
                GameObject go = Instantiate(p.prefab, parent);
                go.SetActive(false);
                PooledObjectLite po = go.GetComponent<PooledObjectLite>();
                if (po == null) po = go.AddComponent<PooledObjectLite>();
                po.Init(this, p.prefab);
                q.Enqueue(go);
            }
            _dict[p.prefab] = q;
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) { Debug.LogError("Spawn prefab is null"); return null; }

        Queue<GameObject> q;
        if (!_dict.TryGetValue(prefab, out q))
        {
            // Створюємо динамічний пул, якщо такого ще нема
            Transform parent = new GameObject("Pool_" + prefab.name + "_Dynamic").transform;
            parent.SetParent(transform);

            if (_parents == null) _parents = new Dictionary<GameObject, Transform>();
            _parents[prefab] = parent;

            if (_dict == null) _dict = new Dictionary<GameObject, Queue<GameObject>>();
            q = new Queue<GameObject>();
            _dict[prefab] = q;
        }

        GameObject go = (q.Count > 0) ? q.Dequeue() : CreateExtra(prefab);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        return go;
    }

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

    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        PooledObjectLite po = instance.GetComponent<PooledObjectLite>();
        if (po == null || po.PrefabKey == null || !_dict.ContainsKey(po.PrefabKey))
        {
            Destroy(instance);
            return;
        }

        instance.SetActive(false);
        _dict[po.PrefabKey].Enqueue(instance);
    }
}
