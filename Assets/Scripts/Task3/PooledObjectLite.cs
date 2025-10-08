using UnityEngine;

public class PooledObjectLite : MonoBehaviour
{
    private ObjectPoolLite _pool;
    private GameObject _prefabKey;
    public GameObject PrefabKey { get { return _prefabKey; } }

    public void Init(ObjectPoolLite pool, GameObject key)
    {
        _pool = pool;
        _prefabKey = key;
    }

    public void ReturnToPool()
    {
        if (_pool != null) _pool.Despawn(gameObject);
        else gameObject.SetActive(false);
    }
}
