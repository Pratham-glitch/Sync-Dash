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
    public ParticleSystem orbCollected;

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
        transform.Translate(Vector3.forward * GameManager.Instance.CurrentSpeed * Time.deltaTime);
    }


   /* void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GhostOrb"))
        {
            CollectOrb(other.gameObject);
        }
    }

    void CollectOrb(GameObject orb)
    {
        //GameManager.Instance.AddScore_Bot(10);

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnObject("GhostOrb", orb);
        }
        else
        {
            Debug.Log($"GhostOrb found at {orb.name} for pooling.");
            if (ObjectPool.Instance != null)
            {
                ObjectPool.Instance.ReturnObject("GhostOrb", orb);
                return; // Only return one orb
            }
            else
            {
                Debug.LogWarning("ObjectPool.Instance is null. GhostOrb cannot be returned to pool.");
            }
            Destroy(orb);
        }
    }*/

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
                //PlayCollectEffect(action.position);
                break;
            case ActionType.Collision:
                //PlayCollisionEffect(action.position);
                break;
        }
    }

    void PerformJump()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, GameManager.Instance.playerController.jumpForce, rb.linearVelocity.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 10f, rb.linearVelocity.z);
        }

        // Play jump particle effect for the ghost
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
            Debug.LogWarning("ParticleManager.Instance is null. Jump effect for ghost cannot be played.");
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
        rb.angularVelocity = Vector3.zero; // Also reset angular velocity
        actionQueue.Clear();
    }

    internal void PlayOrbCollectEffect(Vector3 position)
    {
        orbCollected.transform.position = position;
        orbCollected.Play();
    }
}
