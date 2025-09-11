using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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
            
            var healthComponent = enemy.GetUnitComponent<HealthComponent>();
            if (healthComponent != null) {
                healthComponent.EventDeath += OnEnemyDestroyed;
            }
            
            Debug.Log($"<color=green>Enemy spawned: {enemy.gameObject.name}. Total enemies: {activeEnemies.Count}</color>");
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
            else {
                Debug.Log($"The object '{enemy.gameObject.name}' is not a poolable object. Destroying...");
                Destroy(enemy.gameObject);
            }

            enemy.GetUnitComponent<HealthComponent>().EventDeath -= OnEnemyDestroyed;
            Debug.Log($"<color=red>Enemy destroyed and returned to pool: {enemy.gameObject.name}. Total enemies: {activeEnemies.Count}</color>");
        }
        else {
            Debug.LogError($"Tried to destroy enemy which is not in activeEnemies list");
        }
    }
    
    // Find closest enemy to position
    public Unit GetClosestEnemy(Vector3 position, float range = float.MaxValue)
    {
        if (activeEnemies.Count == 0) return null;
        
        Unit closest = null;
        var closestDistance = float.MaxValue;
        var possibleTargets = activeEnemies.Where(t => t.GetUnitComponent<HealthComponent>() != null);
        foreach (var enemy in possibleTargets) {
            if (enemy == null || !enemy.GetUnitComponent<HealthComponent>().IsAlive) {
                continue;
            }
            
            var distance = Vector3.Distance(position, enemy.Position);
            if (distance < closestDistance) {
                closestDistance = distance;
                if (closestDistance < range) {
                    closest = enemy;
                }
            }
        }
        
        return closest;
    }
}