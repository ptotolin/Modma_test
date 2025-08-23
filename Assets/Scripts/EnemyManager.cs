using System.Collections.Generic;
using System.Linq;
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
            
            Debug.Log($"Enemy destroyed: {enemy.Name}. Total enemies: {activeEnemies.Count}");
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
    
    // Find closest enemy to transform
    public Unit GetClosestEnemy(Transform transform)
    {
        return GetClosestEnemy(transform.position);
    }
    
    // Get all enemies within range
    public List<Unit> GetEnemiesInRange(Vector3 position, float range)
    {
        return activeEnemies
            .Where(enemy => enemy != null && enemy.IsAlive)
            .Where(enemy => Vector3.Distance(position, enemy.Position) <= range)
            .ToList();
    }
    
    // Get enemies sorted by distance
    public List<Unit> GetEnemiesSortedByDistance(Vector3 position)
    {
        return activeEnemies
            .Where(enemy => enemy != null && enemy.IsAlive)
            .OrderBy(enemy => Vector3.Distance(position, enemy.Position))
            .ToList();
    }
    
    // Get random enemy
    public Unit GetRandomEnemy()
    {
        if (activeEnemies.Count == 0) return null;
        
        var aliveEnemies = activeEnemies.Where(e => e != null && e.IsAlive).ToList();
        if (aliveEnemies.Count == 0) return null;
        
        return aliveEnemies[UnityEngine.Random.Range(0, aliveEnemies.Count)];
    }
    
    // Clear all enemies (for level reset, etc.)
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies.ToList()) {
            if (enemy != null) {
                enemy.EventDestroyed -= OnEnemyDestroyed;
                Destroy(enemy.gameObject);
            }
        }
        activeEnemies.Clear();
    }
}