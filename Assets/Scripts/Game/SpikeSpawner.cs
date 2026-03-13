using UnityEngine;

public class SpikeSpawner : MonoBehaviour
{
    [Header("Spike Spawning")]
    public GameObject lowerSpikePrefab; // prefab used when spawning at the lower Y
    public GameObject upperSpikePrefab; // prefab used when spawning at the upper Y
    public float spawnInterval = 1.5f;
    public float spawnX = 10f;                 // X position to spawn at (to the right of the camera)
    public Vector2 spawnYRange = new Vector2(-3f, 3f); // random vertical range

    [Header("Parenting")]
    public Transform spikesParent;             // optional parent for spawned spikes

    private float _timer;

    private void Update()
    {
        if (lowerSpikePrefab == null || upperSpikePrefab == null)
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
        // Decide whether to spawn at the lower or upper lane
        bool spawnLower = Random.value < 0.5f;
        float y = spawnLower ? spawnYRange.x : spawnYRange.y;
        Vector3 spawnPos = new Vector3(spawnX, y, 0f);

        // Choose the appropriate prefab for that lane
        GameObject prefabToUse = spawnLower ? lowerSpikePrefab : upperSpikePrefab;

        Transform parent = spikesParent != null ? spikesParent : transform;
        Instantiate(prefabToUse, spawnPos, Quaternion.identity, parent);
    }
}

