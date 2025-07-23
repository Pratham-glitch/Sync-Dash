using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GhostPlayer : MonoBehaviour
{
    [Header("Sync Settings")]
    public float networkDelay = 0.1f; 

    [Header("References")]
    public Rigidbody rb;

    private Queue<ActionData> actionQueue = new Queue<ActionData>();
    private Vector3 startPosition;

   
    private float idealZPosition;  
    private float smoothedZ;    
    private float zVelocitySmoothDampRef; 
    private const float SMOOTH_TIME = 0.1f;

    public ParticleSystem orbCollected;

    private bool isJumpingPhysics = false; 
    
    [Header("Ground Check")]
    public LayerMask groundLayer = 1;
    public float groundCheckDistance = 0.1f;
    public Transform groundCheck;
    private bool isGrounded; 

    void Start()
    {
        startPosition = transform.position;
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        idealZPosition = transform.position.z;
        smoothedZ = transform.position.z; 

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GhostGroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
            Debug.LogWarning("GhostPlayer: GroundCheck Transform was not assigned. A new one was created as a child.");
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        CheckGrounded(); 
        ProcessActions();
    }

    void FixedUpdate() 
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        ApplyDelayedForwardMovement(); 
    }

    void ApplyDelayedForwardMovement()
    {
        float currentGhostSpeed = GameManager.Instance.CurrentSpeed;

        idealZPosition += currentGhostSpeed * Time.fixedDeltaTime;

        float desiredVisualZ = idealZPosition - (currentGhostSpeed * networkDelay);

        smoothedZ = Mathf.SmoothDamp(rb.position.z, desiredVisualZ, ref zVelocitySmoothDampRef, SMOOTH_TIME);

        Vector3 newPosition = new Vector3(
            rb.position.x,
            rb.position.y,
            smoothedZ
        );

        rb.MovePosition(newPosition);
    }

    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            if (isJumpingPhysics && !wasGrounded)
            {
                isJumpingPhysics = false;
            }
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
            case ActionType.Collect:
                PlayCollectEffect_Simulated(action.position);
                break;
            case ActionType.Collision:
                PlayCollisionEffect_Simulated(action.position);
                break;
        }
    }

    void PerformJump()
    {
        if (!isGrounded) return;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float jumpForce = 10f; 
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            jumpForce = GameManager.Instance.playerController.jumpForce;
        }
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

        isJumpingPhysics = true; 

        // Play jump effect
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayJumpEffect(transform.position);
        }
        else
        {
            Debug.LogWarning("ParticleManager.Instance is null. Jump effect for ghost cannot be played.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GhostOrb"))
        {
            Debug.Log("Ghost Player triggered GhostOrb.");
           
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore_Bot(10);
            }

            StartCoroutine(DoWithDelay(other));
        }
        
    }

    private IEnumerator DoWithDelay(Collider other)
    {
        yield return new WaitForSeconds(1);
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnObject("GhostOrb", other.gameObject);
        }
        else
        {
            Destroy(other.gameObject);
            Debug.LogWarning("ObjectPool.Instance is null. GhostOrb cannot be returned to pool, destroying instead.");
        }

    }

    void PlayCollectEffect_Simulated(Vector3 position)
    {
        Debug.Log("Ghost received simulated collect action!");
       

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore_Bot(10);
        }
    }

    void PlayCollisionEffect_Simulated(Vector3 position)
    {
        Debug.Log("Ghost received simulated collision action!");
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayExplosionEffect(position);
        }
        else
        {
            Debug.LogWarning("ParticleManager.Instance is null. Explosion effect for ghost (simulated) cannot be played.");
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

        isJumpingPhysics = false;
        idealZPosition = startPosition.z; 
        zVelocitySmoothDampRef = 0f;      
    }

    public void PlayOrbCollectEffect(Vector3 position)
    {

        orbCollected.transform.position = position;

        orbCollected.Play();
    }
}
