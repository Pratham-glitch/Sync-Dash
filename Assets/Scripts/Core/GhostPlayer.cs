using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using System;

public class GhostPlayer : MonoBehaviour
{
    [Header("Sync Settings")]
    public float interpolationSpeed = 10f;
    public float networkDelay = 0.1f;

    [Header("References")]
    public Rigidbody rb;

    private Queue<ActionData> actionQueue = new Queue<ActionData>();
    private Vector3 startPosition;

    private float currentTargetZ;
    private float smoothedZ;
    private float zVelocity; 
    private const float SMOOTH_TIME = 0.1f; 

    private bool isJumping = false;
    private float jumpStartTime;
    private Vector3 jumpStartPosition;
    private Vector3 jumpTargetPosition;
    private float jumpDuration = 0.5f; 

    [Header("Ground Check")]
    public LayerMask groundLayer = 1;
    public float groundCheckDistance = 0.1f;
    public Transform groundCheck;
    private bool isGrounded;

    public ParticleSystem orbCollected;

    void Start()
    {
        startPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        currentTargetZ = transform.position.z;
        smoothedZ = transform.position.z;

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        CheckGrounded();
        ProcessActions();
        ApplyDelayedForwardMovement();
    }

    void FixedUpdate()
    {
        if (isJumping && !isGrounded)
        {
            SmoothJumpMovement();
        }
    }

    void ApplyDelayedForwardMovement()
    {
        float idealDeltaZ = GameManager.Instance.CurrentSpeed * Time.deltaTime;
        currentTargetZ += idealDeltaZ;

        float targetZWithDelay = currentTargetZ - (GameManager.Instance.CurrentSpeed * networkDelay);

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

    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);

        if (isGrounded && !wasGrounded && isJumping)
        {
            isJumping = false;
        }
    }

    void ProcessActions()
    {
        while (actionQueue.Count > 0)
        {
            ActionData action = actionQueue.Peek();
            if (Time.time >= action.timestamp + networkDelay)
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

    void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float jumpForce = 10f; 
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            jumpForce = GameManager.Instance.playerController.jumpForce;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        isJumping = true;
        jumpStartTime = Time.time;
        jumpStartPosition = transform.position;

        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayJumpEffect(transform.position);
        }
        else
        {
            Debug.LogWarning("ParticleManager.Instance is null. Jump effect for ghost cannot be played.");
        }
    }

    void PlayCollisionEffect(Vector3 position)
    {
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayExplosionEffect(transform.position);
        }
        else
        {
            Debug.LogWarning("ParticleManager.Instance is null. Collision effect for ghost cannot be played.");
        }
    }

    public void AddAction(ActionData action)
    {
        actionQueue.Enqueue(action);
    }

    public void ResetPlayer()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        actionQueue.Clear();

        isJumping = false;
        currentTargetZ = startPosition.z;
        smoothedZ = startPosition.z;
        zVelocity = 0f;
    }

    internal void PlayOrbCollectEffect(Vector3 position)
    {
        orbCollected.transform.position = position;
        orbCollected.Play();
    }
}