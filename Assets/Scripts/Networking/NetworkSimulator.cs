using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NetworkSimulator : MonoBehaviour
{
    [Header("Network Settings")]
    public float latencyMin = 0.05f;
    public float latencyMax = 0.15f;
    public float packetLossChance = 0.02f;

    [Header("References")]
    public GhostPlayer ghostPlayer;

    private bool isSimulating;
    private Queue<ActionData> sendQueue = new Queue<ActionData>();

    public void StartSimulation()
    {
        isSimulating = true;
        StartCoroutine(ProcessNetworkQueue());
    }

    public void StopSimulation()
    {
        isSimulating = false;
        sendQueue.Clear();
    }

    public void SendAction(ActionData action)
    {
        if (!isSimulating) return;

        float latency = Random.Range(latencyMin, latencyMax);
        action.timestamp += latency;

        sendQueue.Enqueue(action);
    }

    IEnumerator ProcessNetworkQueue()
    {
        while (isSimulating)
        {
            if (sendQueue.Count > 0)
            {
                ActionData action = sendQueue.Dequeue();
                if (ghostPlayer != null)
                    ghostPlayer.AddAction(action);
            }

            yield return new WaitForSeconds(0.01f); // 100Hz network tick
        }
    }
}