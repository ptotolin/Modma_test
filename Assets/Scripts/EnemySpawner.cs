using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    enum Sides
    {
        Left,
        Right,
        Top,
        Bottom, 
        SidesMax
    };
    
    [SerializeField] private int maxEnemies = 50;
    [SerializeField] private Rect generationArea;
    [SerializeField] private Player player;
    [SerializeField] private SimpleEnemy enemyPrefab;
    [SerializeField] private float newEnemySpawnTime = 20;
    
    private float spawnTimer = 0.0f;
    private WorldBounds worldBounds;
    private bool playerDied;

    private void Start()
    {
        worldBounds = WorldBounds.Instance;
        SpawnEnemy();
        player.GetComponent<HealthComponent>().EventDeath += OnDie;
    }

    private void OnDie(Unit playerUnit)
    {
        playerDied = true;
    }

    private void Update()
    {
        if (playerDied) {
            return;
        }
        
        if (EnemyManager.Instance.EnemyCount < maxEnemies) {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > newEnemySpawnTime) {
                spawnTimer -= newEnemySpawnTime;
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        var enemy = ObjectPool.Instance.Spawn<SimpleEnemy>(enemyPrefab.gameObject, GenerateRandomPointOutsideBounds());
        if (enemy != null) {
            enemy.SetTarget(player);
            EnemyManager.Instance.RegisterEnemy(enemy);
        }
    }

    private Vector2 GenerateRandomPointOutsideBounds()
    {
        var side = (Sides)Random.Range(0, (int)Sides.SidesMax);
        switch (side) {
            case Sides.Left:
                return new Vector2(worldBounds.MinBounds.x,
                    Random.Range(worldBounds.MinBounds.y, worldBounds.MaxBounds.y));
                
            case Sides.Right:
                return new Vector2(worldBounds.MaxBounds.x,
                    Random.Range(worldBounds.MinBounds.y, worldBounds.MaxBounds.y));
            
            case Sides.Top:
                return new Vector2(Random.Range(worldBounds.MinBounds.x, worldBounds.MaxBounds.x), worldBounds.MaxBounds.y);
            
            case Sides.Bottom:
                return new Vector2(Random.Range(worldBounds.MinBounds.x, worldBounds.MaxBounds.x), worldBounds.MinBounds.y);
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}