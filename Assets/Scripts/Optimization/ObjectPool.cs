using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Debug.Log("ObjectPool: Awake() called.");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("ObjectPool: Instance set successfully.");
        }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    void Start()
    {
        Debug.Log("ObjectPool: Start() called.");
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        // Check if there are available objects in the queue
        if (poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning($"Pool for tag {tag} is empty. Consider increasing pool size or dynamically expanding.");

            return null; 
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public void ReturnObject(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist for returning object.");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.position = Vector3.zero; 
        obj.transform.rotation = Quaternion.identity; 
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        poolDictionary[tag].Enqueue(obj);
        Debug.Log($"Object with tag {tag} returned to pool.");
    }
}
