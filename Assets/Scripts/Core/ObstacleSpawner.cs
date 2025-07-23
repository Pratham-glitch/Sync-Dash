using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject obstaclePrefab; 
    public GameObject orbPrefab;      
    public float spawnDistance = 20f;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;
    public float orbSpawnChance = 0.9f;  

    [Header("Spawn Positions")]
    public Transform[] playerSpawnPoints;
    public Transform[] ghostSpawnPoints; 

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
            //Debug.LogWarning("ObjectPool, GameManager, or PlayerController not initialized. Cannot spawn obstacles." + (ObjectPool.Instance ) + GameManager.Instance == null + GameManager.Instance.playerController);
            return;
        }

        if (playerSpawnPoints.Length == 0)
        {
            Debug.LogWarning("Player Spawn Points not assigned in ObstacleSpawner.");
            return;
        }

        int randomIndex = Random.Range(0, playerSpawnPoints.Length);

         Vector3 playerSpawnPos = playerSpawnPoints[randomIndex].position;
         playerSpawnPos.z = GameManager.Instance.playerController.transform.position.z + spawnDistance;

        GameObject playerObstacle = ObjectPool.Instance.SpawnFromPool("Obstacle", playerSpawnPos, Quaternion.identity);
        if (playerObstacle != null)
        {
            playerObstacle.tag = "Obstacle";
            Rigidbody rb = playerObstacle.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }

         if (ghostSpawnPoints.Length > randomIndex)
        {
            Vector3 ghostSpawnPos = ghostSpawnPoints[randomIndex].position;
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
            //Debug.LogWarning("ObjectPool, GameManager, or PlayerController not initialized. Cannot spawn orbs.");
            return;
        }

        if (playerSpawnPoints.Length == 0)
        {
            Debug.LogWarning("Player Spawn Points not assigned in ObstacleSpawner.");
            return;
        }

        int randomIndex = Random.Range(0, playerSpawnPoints.Length);

        Vector3 playerSpawnPos = playerSpawnPoints[randomIndex].position;
        playerSpawnPos.z = GameManager.Instance.playerController.transform.position.z + spawnDistance;
        playerSpawnPos.y += 1f; 

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
