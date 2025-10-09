using UnityEngine;

/// <summary>
/// Lightweight component attached to pooled GameObject instances.
/// Tracks the owning pool and prefab key for proper despawning.
/// Added automatically by ObjectPoolLite during instantiation.
/// Call ReturnToPool() externally (e.g., from object lifecycle scripts) to return to pool.
/// </summary>
public class PooledObjectLite : MonoBehaviour
{
    private ObjectPoolLite _pool;         
    private GameObject _prefabKey;        // Key (prefab) to identify the correct queue in the pool
    /// <summary>
    /// Public getter for the prefab key (used by pool for enqueueing).
    /// </summary>
    public GameObject PrefabKey { get { return _prefabKey; } }

    /// <summary>
    /// Initializes the component with pool reference and prefab key.
    /// Called by ObjectPoolLite during pool building or extra creation.
    /// </summary>
    public void Init(ObjectPoolLite pool, GameObject key)
    {
        _pool = pool;
        _prefabKey = key;
    }

    /// <summary>
    /// Returns this instance to its pool (deactivates and enqueues).
    /// If no pool, just deactivates the object.
    /// </summary>
    public void ReturnToPool()
    {
        if (_pool != null)
            _pool.Despawn(gameObject);  
        else
            gameObject.SetActive(false);  // Fallback: Deactivate without pooling
    }
}
