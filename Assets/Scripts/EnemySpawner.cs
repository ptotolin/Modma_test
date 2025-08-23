using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private int maxEnemies = 50;
    [SerializeField] private Rect generationArea;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SimpleEnemy enemyPrefab;
    [SerializeField] private float newEnemySpawnTime = 20;
    
    private float spawnTimer = 0.0f;

    private void Start()
    {
        SpawnEnemyRandomly();
    }

    private void Update()
    {
        if (EnemyManager.Instance.EnemyCount < maxEnemies) {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > newEnemySpawnTime) {
                spawnTimer -= newEnemySpawnTime;
                SpawnEnemyRandomly();
            }
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
        
        EnemyManager.Instance.RegisterEnemy(enemy);
    }
}
