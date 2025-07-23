using UnityEngine;

public class OrbBehavior : MonoBehaviour
{
    [Tooltip("Points awarded when this orb is collected.")]
    public int scoreValue = 10;

    [Tooltip("Speed of rotation in degrees per second.")]
    public float rotationSpeed = 90f; // Default rotation speed

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the Player
        if (other.CompareTag("Player"))
        {
            HandleCollection("Player", other.gameObject);
        }
        // Check if the collider belongs to the GhostPlayer
        else if (other.CompareTag("GhostPlayer"))
        {
            HandleCollection("GhostPlayer", other.gameObject);
        }
    }
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void HandleCollection(string collectorTag, GameObject collector)
    {
        Vector3 collectorPosition = collector.transform.position;
       
        if(gameObject.CompareTag("Orb"))
        {
            collector.GetComponent<PlayerController>().PlayOrbCollectEffect(transform.position);
        }
        else
        {
            collector.GetComponent<GhostPlayer>().PlayOrbCollectEffect(transform.position);
        }

        // Update score based on who collected it
        if (GameManager.Instance != null)
        {
            if (collectorTag == "Player")
            {
                GameManager.Instance.AddScore(scoreValue);
                // Send collect action to network simulator from the player's perspective
                if (GameManager.Instance.networkSimulator != null)
                {
                    GameManager.Instance.networkSimulator.SendAction(new ActionData
                    {
                        actionType = ActionType.Collect,
                        timestamp = Time.time,
                        position = transform.position // Position of the orb when collected
                    });
                }
            }
            else if (collectorTag == "GhostPlayer")
            {
                GameManager.Instance.AddScore_Bot(scoreValue);
                // No need to send action to network simulator from ghost, as ghost is receiving actions.
            }
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null. Score cannot be updated.");
        }

        // Return the orb to the object pool
        if (ObjectPool.Instance != null)
        {
            // Determine the correct pool tag based on the orb's current tag
            // This is important because the ObstacleSpawner creates "Orb" and "GhostOrb"
            ObjectPool.Instance.ReturnObject(gameObject.tag, gameObject);
        }
        else
        {
            Destroy(gameObject); // Fallback if object pooling is not set up
            Debug.LogWarning("ObjectPool.Instance is null. Orb cannot be returned to pool, destroying instead.");
        }
    }


}
