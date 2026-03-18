using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerBootstrap
{
    private const string PlayerPrefabPath = "ImportedPlayer/Body";
    private const string PlayerTag = "Player";
    private const string ExpectedModelName = "Magician_RIO_Unity";
    private const float DefaultSpawnX = 15f;
    private const float DefaultSpawnZ = 0f;
    private const float SpawnOffsetFromSpawner = 70f;
    private const float PlayerScale = 10f;
    private const string GroundObjectName = "Cube";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SpawnPlayer();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnPlayer();
    }

    private static void SpawnPlayer()
    {
        ConfigureSceneHazards();

        GameObject existingPlayer = FindExistingPlayer();
        if (existingPlayer != null && !HasExpectedModel(existingPlayer))
        {
            existingPlayer.tag = "Untagged";
            Object.Destroy(existingPlayer);
            existingPlayer = null;
        }

        if (existingPlayer != null)
        {
            ConfigurePlayer(existingPlayer);
            return;
        }

        SpikeSpawner spawner = Object.FindAnyObjectByType<SpikeSpawner>();
        if (spawner == null)
        {
            return;
        }

        GameObject playerPrefab = Resources.Load<GameObject>(PlayerPrefabPath);
        if (playerPrefab == null)
        {
            Debug.LogError($"Unable to load player prefab from Resources/{PlayerPrefabPath}.");
            return;
        }

        Vector3 spawnFeetPosition = GetSpawnFeetPosition(spawner);
        GameObject player = Object.Instantiate(playerPrefab, spawnFeetPosition, Quaternion.identity);
        player.name = "Player";
        player.tag = PlayerTag;
        player.transform.localScale = Vector3.one * PlayerScale;

        PlayerLaneController controller = player.GetComponent<PlayerLaneController>();
        if (controller == null)
        {
            controller = player.AddComponent<PlayerLaneController>();
        }

        controller.Configure(spawnFeetPosition);
    }

    private static void ConfigurePlayer(GameObject player)
    {
        if (!player.CompareTag(PlayerTag))
        {
            player.tag = PlayerTag;
        }

        player.transform.localScale = Vector3.one * PlayerScale;

        PlayerLaneController controller = player.GetComponent<PlayerLaneController>();
        if (controller == null)
        {
            controller = player.AddComponent<PlayerLaneController>();
        }

        controller.ConfigureFromCurrentPosition();
    }

    private static GameObject FindExistingPlayer()
    {
        return GameObject.FindGameObjectWithTag(PlayerTag);
    }

    private static bool HasExpectedModel(GameObject player)
    {
        if (player == null)
        {
            return false;
        }

        Transform[] transforms = player.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in transforms)
        {
            if (child.name == ExpectedModelName)
            {
                return true;
            }
        }

        return false;
    }

    private static Vector3 GetSpawnFeetPosition(SpikeSpawner spawner)
    {
        float spawnX = spawner != null ? spawner.spawnX + SpawnOffsetFromSpawner : DefaultSpawnX;
        float groundTopY = GetGroundTopY();
        return new Vector3(spawnX, groundTopY, DefaultSpawnZ);
    }

    private static float GetGroundTopY()
    {
        GameObject ground = GameObject.Find(GroundObjectName);
        if (ground != null)
        {
            Collider groundCollider = ground.GetComponent<Collider>();
            if (groundCollider != null)
            {
                return groundCollider.bounds.max.y;
            }
        }

        return 50f;
    }

    private static void ConfigureSceneHazards()
    {
        GameObject lava = GameObject.Find("lava");
        if (lava == null)
        {
            return;
        }

        Collider lavaCollider = lava.GetComponent<Collider>();
        if (lavaCollider != null)
        {
            lavaCollider.isTrigger = false;
        }
    }
}
