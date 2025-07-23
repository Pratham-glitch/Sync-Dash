using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float jumpForce = 10f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer = 1;

    [Header("Input Delay")]
    [Tooltip("The delay in seconds between player input/intended position and action execution/actual position.")]
    public float inputDelay = 0.15f;

    [Header("References")]
    public Transform groundCheck;
    public Rigidbody rb;
    [Tooltip("Assign the CameraShake script attached to the player's camera.")]
    //public CameraShake playerCameraShake;

    private bool isGrounded;
    private bool canJump = true;
    private Vector3 startPosition;
    private Queue<ActionData> actionQueue = new Queue<ActionData>();

    private float currentTargetZ;
    private float lastUpdateTime;

    public ParticleSystem orbCollected;

    void Start()
    {
        startPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        currentTargetZ = transform.position.z;
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        CheckGrounded();
        HandleInput();
        ProcessQueuedActions();
        ApplyDelayedForwardMovement();
    }

    void HandleInput()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && isGrounded && canJump)
        {
            actionQueue.Enqueue(new ActionData
            {
                actionType = ActionType.Jump,
                timestamp = Time.time,
                position = transform.position
            });
            canJump = false;
        }
    }

    void ProcessQueuedActions()
    {
        while (actionQueue.Count > 0)
        {
            ActionData action = actionQueue.Peek();

            if (Time.time >= action.timestamp + inputDelay)
            {
                actionQueue.Dequeue();
                ExecuteAction(action);
            }
            else
            {
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
        }
    }

    void ApplyDelayedForwardMovement()
    {
        float idealDeltaZ = GameManager.Instance.CurrentSpeed * Time.deltaTime;
        currentTargetZ += idealDeltaZ;

        float targetZWithDelay = currentTargetZ - (GameManager.Instance.CurrentSpeed * inputDelay);

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            Mathf.Lerp(transform.position.z, targetZWithDelay, 10f * Time.deltaTime)
        );

        lastUpdateTime = Time.time;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    void PerformJump()
    {
        Jump();
        // Play jump particle effect for the player
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayJumpEffect(transform.position);
        }

        if (GameManager.Instance.networkSimulator != null)
        {
            GameManager.Instance.networkSimulator.SendAction(new ActionData
            {
                actionType = ActionType.Jump,
                timestamp = Time.time,
                position = transform.position
            });
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);
        if (isGrounded)
        {
            canJump = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            HitObstacle(other.gameObject);
        }
    }

    void CollectOrb(GameObject orb)
    {
        GameManager.Instance.AddScore(10);

       

        if (GameManager.Instance.networkSimulator != null)
        {
            GameManager.Instance.networkSimulator.SendAction(new ActionData
            {
                actionType = ActionType.Collect,
                timestamp = Time.time,
                position = orb.transform.position
            });
        }

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnObject("Orb", orb);
        }
        else
        {
            Destroy(orb);
        }
    }

    void HitObstacle(GameObject obstacle)
    {
        

        if (GameManager.Instance.networkSimulator != null)
        {
            GameManager.Instance.networkSimulator.SendAction(new ActionData
            {
                actionType = ActionType.Collision,
                timestamp = Time.time,
                position = obstacle.transform.position
            });
        }

        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayExplosionEffect(transform.position);
        }
        else
        {
            Debug.LogWarning("ParticleManager.Instance is null. Jump effect for ghost cannot be played.");
        }

        GameManager.Instance.GameOver();

    }

    public void ResetPlayer()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        actionQueue.Clear();
        canJump = true;
        currentTargetZ = startPosition.z;
        lastUpdateTime = Time.time;
    }

    internal void PlayOrbCollectEffect(Vector3 position)
    {
        orbCollected.transform.position = position;
        orbCollected.Play();
    }
}
