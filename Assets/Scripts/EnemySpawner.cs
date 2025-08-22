using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Rect generationArea;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SimpleEnemy enemyPrefab;
    [SerializeField] private float newEnemySpawnTime = 20;
    
    private float spawnTimer = 0.0f;
    private List<Unit> enemies = new();

    private void Start()
    {
        SpawnEnemyRandomly();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer > newEnemySpawnTime) {
            spawnTimer -= newEnemySpawnTime;
            SpawnEnemyRandomly();
        }
    }

    private void SpawnEnemyRandomly()
    {
        Vector2 pos = new Vector2(
            Random.Range(generationArea.xMin, generationArea.xMax),
            Random.Range(generationArea.yMin, generationArea.yMax));
        
        var enemy = Instantiate(enemyPrefab);
        enemy.transform.position = pos;
        enemy.SetTarget(playerTransform);
        var healthComponent = enemy.GetUnitComponent<HealthComponent>();
        enemy.GetUnitComponent<HealthComponent>().EventDeath += OnEnemyDie;
        enemies.Add(enemy);
    }

    private void OnEnemyDie(Unit unit)
    {
        enemies.Remove(unit);
    }
}
