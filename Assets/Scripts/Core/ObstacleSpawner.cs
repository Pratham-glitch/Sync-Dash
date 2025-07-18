using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject obstaclePrefab; // Still needed for initial pool setup
    public GameObject orbPrefab;      // Still needed for initial pool setup
    public float spawnDistance = 20f;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;
    public float orbSpawnChance = 0.9f; // Increased chance for orbs

    [Header("Spawn Positions")]
    public Transform[] playerSpawnPoints; // For X and Y positions within lanes
    public Transform[] ghostSpawnPoints;  // For X and Y positions within lanes

    private bool isSpawning;

    public void StartSpawning()
    {
        isSpawning = true;
        StartCoroutine(SpawnObjects());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnObjects()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));

            if (Random.value < orbSpawnChance)
            {
                SpawnOrb();
            }
            else
            {
                SpawnObstacle();
            }
        }
    }

    void SpawnObstacle()
    {
        if (ObjectPool.Instance == null || GameManager.Instance == null || GameManager.Instance.playerController == null)
        {
            Debug.LogWarning("ObjectPool, GameManager, or PlayerController not initialized. Cannot spawn obstacles." + (ObjectPool.Instance ) + GameManager.Instance == null + GameManager.Instance.playerController);
            return;
        }

        if (playerSpawnPoints.Length == 0)
        {
            Debug.LogWarning("Player Spawn Points not assigned in ObstacleSpawner.");
            return;
        }

        int randomIndex = Random.Range(0, playerSpawnPoints.Length);

        // Player Obstacle
        Vector3 playerSpawnPos = playerSpawnPoints[randomIndex].position;
        // Adjust Z position relative to the player's current Z position
        playerSpawnPos.z = GameManager.Instance.playerController.transform.position.z + spawnDistance;

        GameObject playerObstacle = ObjectPool.Instance.SpawnFromPool("Obstacle", playerSpawnPos, Quaternion.identity);
        if (playerObstacle != null)
        {
            playerObstacle.tag = "Obstacle";
            Rigidbody rb = playerObstacle.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }

        // Ghost Obstacle
        if (ghostSpawnPoints.Length > randomIndex)
        {
            Vector3 ghostSpawnPos = ghostSpawnPoints[randomIndex].position;
            // Adjust Z position relative to the ghost player's current Z position
            ghostSpawnPos.z = GameManager.Instance.ghostPlayer.transform.position.z + spawnDistance;

            GameObject ghostObstacle = ObjectPool.Instance.SpawnFromPool("GhostObstacle", ghostSpawnPos, Quaternion.identity);
            if (ghostObstacle != null)
            {
                ghostObstacle.tag = "GhostObstacle";
                Rigidbody rb = ghostObstacle.GetComponent<Rigidbody>();
                if (rb != null) rb.linearVelocity = Vector3.zero;
            }
        }
    }

    void SpawnOrb()
    {
        if (ObjectPool.Instance == null || GameManager.Instance == null || GameManager.Instance.playerController == null)
        {
            Debug.LogWarning("ObjectPool, GameManager, or PlayerController not initialized. Cannot spawn orbs.");
            return;
        }

        if (playerSpawnPoints.Length == 0)
        {
            Debug.LogWarning("Player Spawn Points not assigned in ObstacleSpawner.");
            return;
        }

        int randomIndex = Random.Range(0, playerSpawnPoints.Length);

        // Player Orb
        Vector3 playerSpawnPos = playerSpawnPoints[randomIndex].position;
        // Adjust Z position relative to the player's current Z position
        playerSpawnPos.z = GameManager.Instance.playerController.transform.position.z + spawnDistance;
        playerSpawnPos.y += 1f; // Orbs typically float above the ground

        GameObject playerOrb = ObjectPool.Instance.SpawnFromPool("Orb", playerSpawnPos, Quaternion.identity);
        if (playerOrb != null)
        {
            playerOrb.tag = "Orb";
            Rigidbody rb = playerOrb.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }

        // Ghost Orb
        if (ghostSpawnPoints.Length > randomIndex)
        {
            Vector3 ghostSpawnPos = ghostSpawnPoints[randomIndex].position;
            // Adjust Z position relative to the ghost player's current Z position
            ghostSpawnPos.z = GameManager.Instance.ghostPlayer.transform.position.z + spawnDistance;
            ghostSpawnPos.y += 1f;

            GameObject ghostOrb = ObjectPool.Instance.SpawnFromPool("GhostOrb", ghostSpawnPos, Quaternion.identity);
            if (ghostOrb != null)
            {
                ghostOrb.tag = "GhostOrb";
                Rigidbody rb = ghostOrb.GetComponent<Rigidbody>();
                if (rb != null) rb.linearVelocity = Vector3.zero;
            }
        }
    }
}
