using UnityEngine;

public class OrbBehavior : MonoBehaviour
{
    [Tooltip("Points awarded when this orb is collected.")]
    public int scoreValue = 10;

    [Tooltip("Speed of rotation in degrees per second.")]
    public float rotationSpeed = 90f; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HandleCollection("Player", other.gameObject);
        }
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

        if (GameManager.Instance != null)
        {
            if (collectorTag == "Player")
            {
                GameManager.Instance.AddScore(scoreValue);
                if (GameManager.Instance.networkSimulator != null)
                {
                    GameManager.Instance.networkSimulator.SendAction(new ActionData
                    {
                        actionType = ActionType.Collect,
                        timestamp = Time.time,
                        position = transform.position 
                    });
                }
            }
            else if (collectorTag == "GhostPlayer")
            {
                GameManager.Instance.AddScore_Bot(scoreValue);
            }
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null. Score cannot be updated.");
        }

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnObject(gameObject.tag, gameObject);
        }
        else
        {
            Destroy(gameObject); 
            Debug.LogWarning("ObjectPool.Instance is null. Orb cannot be returned to pool, destroying instead.");
        }
    }


}
