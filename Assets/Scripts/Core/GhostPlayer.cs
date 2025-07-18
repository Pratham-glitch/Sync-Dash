using UnityEngine;
using System.Collections.Generic;

public class GhostPlayer : MonoBehaviour
{
    [Header("Sync Settings")]
    public float interpolationSpeed = 10f; // For smoothing movement between received states
    public float networkDelay = 0.1f;    // Configurable delay for network simulation

    [Header("References")]
    public Rigidbody rb;

    private Queue<ActionData> actionQueue = new Queue<ActionData>();
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        ProcessActions();
        MoveForward();
    }

    void MoveForward()
    {
        // Ghost player also moves forward at the current game speed
        transform.Translate(Vector3.forward * GameManager.Instance.CurrentSpeed * Time.deltaTime);
    }

    void ProcessActions()
    {
        // Process actions from the queue that have passed their simulated network delay
        while (actionQueue.Count > 0)
        {
            ActionData action = actionQueue.Peek();

            // Check if the current time has surpassed the action's timestamp plus the network delay
            if (Time.time >= action.timestamp + networkDelay)
            {
                actionQueue.Dequeue(); // Remove the action from the queue
                ExecuteAction(action); // Execute the action
            }
            else
            {
                break; // Actions in the queue are not yet ready to be processed
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GhostOrb"))
        {
            CollectOrb(other.gameObject);
        }
    }

    void CollectOrb(GameObject orb)
    {
        //GameManager.Instance.AddScore_Bot(10);

        // Play orb collect particle effect at orb's position
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayOrbCollectEffect(orb.transform.position);
        }

        // Send collect action to network simulator
        if (GameManager.Instance.networkSimulator != null)
        {
            GameManager.Instance.networkSimulator.SendAction(new ActionData
            {
                actionType = ActionType.Collect,
                timestamp = Time.time,
                position = orb.transform.position
            });
        }

        // Return orb to object pool instead of destroying
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnObject("GhostOrb", orb);
        }
        else
        {
            Destroy(orb); // Fallback if object pooling is not set up
        }
    }


    void ExecuteAction(ActionData action)
    {
        switch (action.actionType)
        {
            case ActionType.Jump:
                PerformJump();
                break;
            case ActionType.Collect:
                PlayCollectEffect(action.position);
                break;
            case ActionType.Collision:
                PlayCollisionEffect(action.position);
                break;
        }
    }

    void PerformJump()
    {
        // Ensure the ghost player jumps with the same force as the main player
        // Access GameManager to get the player's jump force for consistency
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, GameManager.Instance.playerController.jumpForce, rb.linearVelocity.z);
        }
        else
        {
            // Fallback if playerController reference is not available
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 10f, rb.linearVelocity.z);
        }

        // Play jump particle effect for the ghost
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayJumpEffect(transform.position);
        }
    }

    void PlayCollectEffect(Vector3 position)
    {

        Debug.Log("Ghost collected orb!");
        // Play orb collect particle effect for the ghost at the collected orb's position
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayOrbCollectEffect(position);
        }
        // TODO: Optionally, find and "collect" the corresponding ghost orb in the scene
        // This would involve finding the ghost orb near 'position' and returning it to the pool.
        // For now, only the particle effect is played.
    }

    void PlayCollisionEffect(Vector3 position)
    {
        Debug.Log("Ghost hit obstacle!");
        // Play explosion particle effect for the ghost at the obstacle's position
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayExplosionEffect(position);
        }
        // TODO: Optionally, find and "dissolve" the corresponding ghost obstacle in the scene
        // For now, only the particle effect is played.
    }

    public void AddAction(ActionData action)
    {
        actionQueue.Enqueue(action);
    }

    public void ResetPlayer()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        actionQueue.Clear(); // Clear any pending actions on reset
    }
}
