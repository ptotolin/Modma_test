using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    
    // Events
    public event Action<Unit> EventEnemySpawned;
    public event Action<Unit> EventEnemyDestroyed;
    
    // Enemy tracking
    private List<Unit> activeEnemies = new List<Unit>();
    
    // Properties
    public int EnemyCount => activeEnemies.Count;
    public bool HasEnemies => activeEnemies.Count > 0;
    
    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    // Register enemy when spawned
    public void RegisterEnemy(Unit enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy)) {
            activeEnemies.Add(enemy);
            EventEnemySpawned?.Invoke(enemy);
            
            // Subscribe to enemy destruction
            enemy.EventDestroyed += OnEnemyDestroyed;
            
            Debug.Log($"Enemy registered: {enemy.Name}. Total enemies: {activeEnemies.Count}");
        }
    }
    
    // Unregister enemy when destroyed
    private void OnEnemyDestroyed(Unit enemy)
    {
        if (activeEnemies.Contains(enemy)) {
            activeEnemies.Remove(enemy);
            EventEnemyDestroyed?.Invoke(enemy);
            
            // Return to pool instead of destroying
            var pooledObject = enemy.GetComponent<PooledObject>();
            if (pooledObject != null) {
                pooledObject.Despawn();
            }
            
            Debug.Log($"Enemy destroyed and returned to pool: {enemy.Name}. Total enemies: {activeEnemies.Count}");

        }
    }
    
    // Find closest enemy to position
    public Unit GetClosestEnemy(Vector3 position)
    {
        if (activeEnemies.Count == 0) return null;
        
        Unit closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (var enemy in activeEnemies) {
            if (enemy == null || !enemy.IsAlive) continue;
            
            float distance = Vector3.Distance(position, enemy.Position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closest = enemy;
            }
        }
        
        return closest;
    }
}