using System;
using System.Collections;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    // Events
    public event Action EventSpawned;
    public event Action EventDespawned;
    
    private string poolName;
    private ObjectPool objectPool;
    
    // Properties
    public string PoolName => poolName;
    public bool IsPooled => objectPool != null;
    
    public void Initialize(string poolName, ObjectPool objectPool)
    {
        this.poolName = poolName;
        this.objectPool = objectPool;
    }
    
    public void OnSpawn()
    {
        EventSpawned?.Invoke();
    }
    
    public void OnDespawn()
    {
        EventDespawned?.Invoke();
    }
    
    // Convenience method to despawn self
    public void Despawn()
    {
        if (objectPool != null) {
            objectPool.Despawn(gameObject);
        }
    }
    
    // Auto-despawn after time
    public void DespawnAfter(float time)
    {
        if (gameObject.activeInHierarchy) {
            StartCoroutine(DespawnAfterCoroutine(time));
        }
    }
    
    private IEnumerator DespawnAfterCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        Despawn();
    }
}