using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("What to spawn")]
    public List<GameObject> prefabs = new();   

    [Header("Where to spawn")]
    public Transform spawnPoint;               

    [Header("Time interval")]
    public float intervalSeconds = 2f;         

    [Header("Radius")]
    public bool useRadius = false;
    public float radius = 3f;

    private Coroutine _loop;

    private void OnEnable()
    {
        if (intervalSeconds <= 0f) intervalSeconds = 1f;
        _loop = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (_loop != null) StopCoroutine(_loop);
    }

    private IEnumerator SpawnLoop()
    {
        yield return null; 

        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(intervalSeconds);
        }
    }

    public void SpawnOne()
    {
        if (prefabs == null || prefabs.Count == 0) { Debug.LogWarning("No prefabs assigned."); return; }
        if (spawnPoint == null) { Debug.LogWarning("No spawnPoint assigned."); return; }

        var prefab = prefabs[Random.Range(0, prefabs.Count)];

        Vector3 pos = spawnPoint.position;
        if (useRadius && radius > 0f)
        {
            Vector2 offset2 = Random.insideUnitCircle * radius; 
            pos += new Vector3(offset2.x, 0f, offset2.y);
        }

        var pool = ObjectPoolLite.Instance;
        if (pool != null) pool.Spawn(prefab, pos, spawnPoint.rotation);
        else Instantiate(prefab, pos, spawnPoint.rotation);
    }
}
