using UnityEngine;

public class SpikeSpawner : MonoBehaviour
{
    [Header("Spike Spawning")]
    public GameObject spikePrefab;
    public float spawnInterval = 1.5f;
    public float spawnX = 10f;                 // X position to spawn at (to the right of the camera)
    public Vector2 spawnYRange = new Vector2(-3f, 3f); // random vertical range

    [Header("Parenting")]
    public Transform spikesParent;             // optional parent for spawned spikes

    private float _timer;

    private void Update()
    {
        if (spikePrefab == null)
            return;

        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            SpawnSpike();
        }
    }

    private void SpawnSpike()
    {
        // Only spawn at either the min (x) or max (y) height you set
        float y = Random.value < 0.5f ? spawnYRange.x : spawnYRange.y;
        Vector3 spawnPos = new Vector3(spawnX, y, 0f);

        Transform parent = spikesParent != null ? spikesParent : transform;
        Instantiate(spikePrefab, spawnPos, Quaternion.identity, parent);
    }
}

