using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    // Singleton
    public static ObjectPool Instance { get; private set; }
    
    // Events
    public event Action<GameObject> EventObjectSpawned;
    public event Action<GameObject> EventObjectDespawned;
    
    // Pool storage - автоматически создается по мере необходимости
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation = default)
    {
        string poolName = prefab.name;
        
        if (!pools.ContainsKey(poolName)) {
            CreatePool(poolName, prefab);
        }
        
        GameObject obj = GetObjectFromPool(poolName);
        if (obj == null) return null;
        
        // Setup object
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        
        // Notify pooled object
        var pooledObject = obj.GetComponent<PooledObject>();
        if (pooledObject != null) {
            pooledObject.OnSpawn();
        }
        
        EventObjectSpawned?.Invoke(obj);
        return obj;
    }
    
    public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation = default) where T : Component
    {
        GameObject obj = Spawn(prefab, position, rotation);
        return obj?.GetComponent<T>();
    }
    
    private void CreatePool(string poolName, GameObject prefab)
    {
        prefabMap[poolName] = prefab;
        pools[poolName] = new Queue<GameObject>();
        
        Debug.Log($"ObjectPool: Auto-created pool '{poolName}'");
    }
    
    private GameObject GetObjectFromPool(string poolName)
    {
        var pool = pools[poolName];
        
        // Get existing object
        if (pool.Count > 0) {
            return pool.Dequeue();
        }
        
        // Create new object if pool is empty
        CreateNewObject(poolName);
        return pool.Dequeue();
    }
    
    static int counter = 0;
    private void CreateNewObject(string poolName)
    {
        if (!prefabMap.ContainsKey(poolName)) return;
        
        GameObject obj = Instantiate(prefabMap[poolName], transform);
        obj.name = (++counter).ToString();
        obj.SetActive(false);
        
        // Add pool identifier
        var poolObject = obj.GetComponent<PooledObject>();
        if (poolObject == null) {
            poolObject = obj.AddComponent<PooledObject>();
        }
        poolObject.Initialize(poolName, this);
        
        pools[poolName].Enqueue(obj);
    }
    
    public void Despawn(GameObject obj)
    {
        if (obj == null) return;
        
        var pooledObject = obj.GetComponent<PooledObject>();
        if (pooledObject == null) {
            Debug.LogError($"ObjectPool: Trying to despawn non-pooled object: {obj.name}");
            return;
        }
        
        string poolName = pooledObject.PoolName;
        if (!pools.ContainsKey(poolName)) {
            Debug.LogError($"ObjectPool: Pool '{poolName}' doesn't exist for despawn!");
            return;
        }
        
        // Reset object
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        
        // Notify pooled object
        pooledObject.OnDespawn();
        
        // Return to pool
        pools[poolName].Enqueue(obj);
        
        EventObjectDespawned?.Invoke(obj);
    }
    
    public int GetPoolSize(string poolName)
    {
        return pools.ContainsKey(poolName) ? pools[poolName].Count : 0;
    }
    
    public bool HasPool(string poolName)
    {
        return pools.ContainsKey(poolName);
    }

    // private void OnGUI()
    // {
    //     var style = new GUIStyle {
    //         fontSize = 40
    //     };
    //
    //     var sb = new StringBuilder();
    //     sb.AppendLine($"pools count: {pools.Count}");
    //     foreach (var pool in pools) {
    //         sb.AppendLine($"pool[{pool.Key}].Count = {pool.Value.Count}");
    //     }
    //
    //     var str = sb.ToString();
    //     GUI.Label(new Rect(10, 10, 200, 30), $"{str}", style);
    // }
}