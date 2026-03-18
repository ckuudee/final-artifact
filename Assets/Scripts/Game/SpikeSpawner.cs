using UnityEngine;

public class SpikeSpawner : MonoBehaviour
{
    [Header("Spike Spawning")]
    public GameObject lowerSpikePrefab;
    public GameObject upperSpikePrefab;
    public float spawnInterval = 3.5f;
    public float spawnX = 10f;
    public Vector2 spawnYRange = new Vector2(-3f, 3f);

    [Header("Difficulty Scaling")]
    public float startSpeed = 20f;
    public float maxSpeed = 50f;
    public float speedRampTime = 60f;
    public float minSpawnInterval = 0.75f;
    public float intervalRampTime = 45f;

    [Header("Parenting")]
    public Transform spikesParent;

    private float _timer;
    private float _elapsedTime;

    public float CurrentSpeed => Mathf.Lerp(startSpeed, maxSpeed, Mathf.Clamp01(_elapsedTime / speedRampTime));

    private void Update()
    {
        if (lowerSpikePrefab == null || upperSpikePrefab == null)
            return;

        _elapsedTime += Time.deltaTime;

        float currentInterval = Mathf.Lerp(spawnInterval, minSpawnInterval, Mathf.Clamp01(_elapsedTime / intervalRampTime));

        _timer += Time.deltaTime;
        if (_timer >= currentInterval)
        {
            _timer = 0f;
            SpawnSpike();
        }
    }

    private void SpawnSpike()
    {
        bool spawnLower = Random.value < 0.5f;
        float y = spawnLower ? spawnYRange.x : spawnYRange.y;
        Vector3 spawnPos = new Vector3(spawnX, y, 0f);

        GameObject prefabToUse = spawnLower ? lowerSpikePrefab : upperSpikePrefab;

        Transform parent = spikesParent != null ? spikesParent : transform;
        GameObject spike = Instantiate(prefabToUse, spawnPos, Quaternion.identity, parent);

        SpikeController controller = spike.GetComponent<SpikeController>();
        if (controller != null)
        {
            controller.speed = CurrentSpeed;
        }
    }
}

