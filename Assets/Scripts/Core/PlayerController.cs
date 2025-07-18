using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Added for Queue

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float jumpForce = 10f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer = 1;

    [Header("Input Delay")]
    [Tooltip("The delay in seconds between player input/intended position and action execution/actual position.")]
    public float inputDelay = 0.15f; // New variable for input delay

    [Header("References")]
    public Transform groundCheck;
    public Rigidbody rb;

    private bool isGrounded;
    private bool canJump = true;
    private Vector3 startPosition;
    private Queue<ActionData> actionQueue = new Queue<ActionData>(); // Queue for player actions

    // New variables for delayed continuous forward movement
    private float currentTargetZ; // The Z position the player is trying to reach
    private float lastUpdateTime; // To track when the target Z was last updated

    void Start()
    {
        startPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Initialize currentTargetZ to the player's starting Z position
        currentTargetZ = transform.position.z;
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        CheckGrounded();
        HandleInput();
        ProcessQueuedActions(); // Process discrete actions (like jump) after delay
        ApplyDelayedForwardMovement(); // Apply continuous forward movement with delay
    }

    void HandleInput()
    {
        // Only queue input if we are grounded and can jump, and the game is running
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && isGrounded && canJump)
        {
            // Enqueue the jump action with the current time as its timestamp
            actionQueue.Enqueue(new ActionData
            {
                actionType = ActionType.Jump,
                timestamp = Time.time,
                position = transform.position // Position at the time of input
            });
            canJump = false; // Prevent queuing multiple jumps before the first one executes
                             // This 'canJump' will be reset in CheckGrounded once jump is processed.
        }
    }

    void ProcessQueuedActions()
    {
        while (actionQueue.Count > 0)
        {
            ActionData action = actionQueue.Peek(); // Look at the next action without removing it

            // If enough time has passed (current time >= timestamp + delay)
            if (Time.time >= action.timestamp + inputDelay)
            {
                actionQueue.Dequeue(); // Remove the action from the queue
                ExecuteAction(action); // Execute the action
            }
            else
            {
                // The action is not yet ready to be executed, break the loop
                break;
            }
        }
    }

    void ExecuteAction(ActionData action)
    {
        switch (action.actionType)
        {
            case ActionType.Jump:
                PerformJump();
                break;
                // Add other player-initiated actions here if needed (e.g., slide, lane change)
        }
    }

    void ApplyDelayedForwardMovement()
    {
        // Calculate the target Z position the player *should* be at without delay
        // This is the position that the player is trying to catch up to.
        float idealDeltaZ = GameManager.Instance.CurrentSpeed * Time.deltaTime;
        currentTargetZ += idealDeltaZ;

        // Calculate the Z position the player *would* be at if there was no delay
        // This is the 'future' position the player is aiming for.
        float targetZWithDelay = currentTargetZ - (GameManager.Instance.CurrentSpeed * inputDelay);

        // Lerp the player's current Z position towards the targetZWithDelay
        // This creates the visual delay. The player's actual position will always lag behind
        // its intended position by approximately 'inputDelay' seconds of movement.
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            Mathf.Lerp(transform.position.z, targetZWithDelay, 10f * Time.deltaTime) // Using a constant interpolation speed for smoothness
        );

        // Update lastUpdateTime for consistent delta calculation
        lastUpdateTime = Time.time;
    }


    void Jump()
    {
        // This is the actual physics jump, called by ExecuteAction
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        // ParticleManager.Instance.PlayJumpEffect(transform.position); // Uncomment if you have a jump effect
    }

    void PerformJump()
    {
        // This function wrapper is called by ExecuteAction to handle actual jump
        Jump();
        // Send jump action to network simulator
        if (GameManager.Instance.networkSimulator != null)
        {
            GameManager.Instance.networkSimulator.SendAction(new ActionData
            {
                actionType = ActionType.Jump,
                timestamp = Time.time, // Timestamp when action is *executed*
                position = transform.position // Position when action is *executed*
            });
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);
        if (isGrounded)
        {
            canJump = true; // Allow jump again once grounded
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Orb"))
        {
            CollectOrb(other.gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            HitObstacle(other.gameObject);
        }
    }

    void CollectOrb(GameObject orb)
    {
        GameManager.Instance.AddScore(10);

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

        // Use Object Pooling to return the orb instead of destroying it
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnObject("Orb", orb);
        }
        else
        {
            Destroy(orb); // Fallback if ObjectPool is not available
        }
        // Play orb collect effect
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayOrbCollectEffect(orb.transform.position);
        }
    }

    void HitObstacle(GameObject obstacle)
    {
        // Send collision action to network simulator
        if (GameManager.Instance.networkSimulator != null)
        {
            GameManager.Instance.networkSimulator.SendAction(new ActionData
            {
                actionType = ActionType.Collision,
                timestamp = Time.time,
                position = obstacle.transform.position
            });
        }

        GameManager.Instance.GameOver();

        // Use Object Pooling to return the obstacle instead of destroying it
        if (ObjectPool.Instance != null)
        {
            // Assuming obstacle for player is tagged "Obstacle"
            ObjectPool.Instance.ReturnObject("Obstacle", obstacle);
        }
        else
        {
            Destroy(obstacle); // Fallback if ObjectPool is not available
        }
        // Play explosion effect
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayExplosionEffect(obstacle.transform.position);
        }
    }

    public void ResetPlayer()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        actionQueue.Clear(); // Clear any pending actions on reset
        canJump = true; // Ensure player can jump after reset
        // Reset currentTargetZ and lastUpdateTime on player reset
        currentTargetZ = startPosition.z;
        lastUpdateTime = Time.time;
    }
}
