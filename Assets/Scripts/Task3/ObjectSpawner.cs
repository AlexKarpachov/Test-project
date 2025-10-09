using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns random prefabs at intervals from a list.
/// Integrates with ObjectPoolLite for efficient spawning (fallback to Instantiate if no pool).
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [Header("What to spawn")]
    public List<GameObject> prefabs = new();  

    [Header("Where to spawn")]
    public Transform spawnPoint;              

    [Header("Time interval")]
    public float intervalSeconds = 2f;        

    [Header("Radius (Bonus)")]
    public bool useRadius = false;            // Toggle to enable random offset spawning
    public float radius = 3f;                 // Radius of the spawn circle (XZ plane, Y=0)

    private Coroutine _loop;  // Reference to the spawning coroutine (for stopping)

    /// <summary>
    /// Starts the spawn loop coroutine when the component is enabled.
    /// Ensures interval is positive to avoid infinite loops.
    /// </summary>
    private void OnEnable()
    {
        if (intervalSeconds <= 0f) intervalSeconds = 1f;  
        _loop = StartCoroutine(SpawnLoop());  
    }

    /// <summary>
    /// Stops the spawn loop when the component is disabled (e.g., scene change or pause).
    /// </summary>
    private void OnDisable()
    {
        if (_loop != null) StopCoroutine(_loop);  
    }

    /// <summary>
    /// Infinite coroutine that spawns one object every intervalSeconds.
    /// Waits one frame initially to ensure setup (e.g., spawnPoint assigned).
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        yield return null;  

        while (true)  
        {
            SpawnOne();  
            yield return new WaitForSeconds(intervalSeconds);  // Wait for next interval
        }
    }

    /// <summary>
    /// Spawns a single random prefab at the spawn point (with optional radius offset).
    /// Uses ObjectPoolLite if available; falls back to Instantiate.
    /// </summary>
    public void SpawnOne()
    {
        if (prefabs == null || prefabs.Count == 0) { Debug.LogWarning("No prefabs assigned."); return; }
        if (spawnPoint == null) { Debug.LogWarning("No spawnPoint assigned."); return; }

        // Select a random prefab from the list
        var prefab = prefabs[Random.Range(0, prefabs.Count)];

        Vector3 pos = spawnPoint.position;

        if (useRadius && radius > 0f)
        {
            Vector2 offset2 = Random.insideUnitCircle * radius;  
            pos += new Vector3(offset2.x, 0f, offset2.y);        
        }

        var pool = ObjectPoolLite.Instance;
        if (pool != null)
            pool.Spawn(prefab, pos, spawnPoint.rotation);  
        else
            Instantiate(prefab, pos, spawnPoint.rotation);  // Fallback for no pool
    }
}
