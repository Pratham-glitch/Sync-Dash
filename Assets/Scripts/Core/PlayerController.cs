using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    private bool isGrounded;
    private bool canJump = true;
    private Vector3 startPosition;
    private Queue<ActionData> actionQueue = new Queue<ActionData>();

    private float currentTargetZ;
    private float smoothedZ;
    private float zVelocity; 
    private const float SMOOTH_TIME = 0.1f; 

    private bool isJumping = false;
    private float jumpStartTime;
    private Vector3 jumpStartPosition;
    private Vector3 jumpTargetPosition;
    private float jumpDuration = 0.5f;
    public float maxSpeed = 50f;


    public ParticleSystem orbCollected;

    void Start()
    {
        startPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        currentTargetZ = transform.position.z;
        smoothedZ = transform.position.z;
        GameManager.Instance.SetTargetSpeed(maxSpeed);

    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        CheckGrounded();
        HandleInput();
        ProcessQueuedActions();
        ApplyDelayedForwardMovement();
    }

    void FixedUpdate()
    {
        // Handle jump physics in FixedUpdate for smoother physics
        if (isJumping && !isGrounded)
        {
            SmoothJumpMovement();
        }
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

        smoothedZ = Mathf.SmoothDamp(smoothedZ, targetZWithDelay, ref zVelocity, SMOOTH_TIME);

        if (!isJumping)
        {
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                smoothedZ
            );
        }
    }


    void SmoothJumpMovement()
    {
        float newZ = Mathf.SmoothDamp(transform.position.z, smoothedZ, ref zVelocity, SMOOTH_TIME);
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            newZ
        );
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        isJumping = true;
        jumpStartTime = Time.time;
        jumpStartPosition = transform.position;
    }

    void PerformJump()
    {
        Jump();
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
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            canJump = true;

            if (!wasGrounded && isJumping)
            {
                isJumping = false;
            }
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
        isJumping = false;
        currentTargetZ = startPosition.z;
        smoothedZ = startPosition.z;
        zVelocity = 0f;
    }

    public void PlayOrbCollectEffect(Vector3 position)
    {

        orbCollected.transform.position = position;
        orbCollected.Play();
    }
}