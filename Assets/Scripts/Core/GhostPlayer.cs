using UnityEngine;
using System.Collections.Generic;

public class GhostPlayer : MonoBehaviour
{
    [Header("Sync Settings")]
    public float interpolationSpeed = 10f; 
    public float networkDelay = 0.1f;    

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
        transform.Translate(Vector3.forward * GameManager.Instance.CurrentSpeed * Time.deltaTime);
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

        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayOrbCollectEffect(orb.transform.position);
        }

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
            ObjectPool.Instance.ReturnObject("GhostOrb", orb);
        }
        else
        {
            Destroy(orb); 
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
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, GameManager.Instance.playerController.jumpForce, rb.linearVelocity.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 10f, rb.linearVelocity.z);
        }

        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayJumpEffect(transform.position);
        }
    }

    void PlayCollectEffect(Vector3 position)
    {

        Debug.Log("Ghost collected orb!");
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayOrbCollectEffect(position);
        }
    }

    void PlayCollisionEffect(Vector3 position)
    {
        Debug.Log("Ghost hit obstacle!");
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayExplosionEffect(position);
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
        actionQueue.Clear(); 
    }
}
