using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct EnemyAppearData
{
    public GameObject EnemyPrefab;
    public int AppearScore;
}

[Serializable]
public struct WaveInfo
{
    public uint MaxEnemies;
    public List<EnemyAppearData> EnemiesAppearData;
    public float NewEnemySpawnTime;
}

public class EnemySpawner : MonoBehaviour
{
    public event Action<int> EventNextWave;
    
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
    [SerializeField] private float newEnemySpawnTime = 20;
    [SerializeField] private List<WaveInfo> waves = new();
    
    private float spawnTimer = 0.0f;
    private WorldBounds worldBounds;
    private bool playerDied;
    private int currentWaveIndex;
    private float currentWaveTotalDuration;
    private int currentWaveEnemiesSpawned;

    private void Start()
    {
        worldBounds = WorldBounds.Instance;
        player.GetComponent<HealthComponent>().EventDeath += OnDie;
        EventNextWave?.Invoke(0);
        EnemyManager.Instance.EventEnemyDestroyed += OnEnemyDestroyed;
        DebugLogOnGUI.Instance.WatchVariable("enemies destroyed", () => currentWaveEnemiesSpawned);
    }

    private void OnDie(Unit playerUnit)
    {
        playerDied = true;
        EnemyManager.Instance.EventEnemyDestroyed -= OnEnemyDestroyed;
        DebugLogOnGUI.Instance.UnwatchVariable("enemies destroyed");

    }

    private void OnEnemyDestroyed(Unit unit)
    {
    }

    private void Update()
    {
        if (playerDied) {
            return;
        }

        var currentWave = waves[currentWaveIndex];

        if (EnemyManager.Instance.EnemyCount < maxEnemies) {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > currentWave.NewEnemySpawnTime) {
                spawnTimer -= currentWave.NewEnemySpawnTime;

                if (currentWaveEnemiesSpawned < currentWave.MaxEnemies) {
                    var enemyPrefab = GetEnemyFromWave(currentWave);
                    if (enemyPrefab != null) {
                        if (TrySpawnEnemy(enemyPrefab)) {
                            currentWaveEnemiesSpawned++;
                        }
                    }
                    else {
                        Debug.LogError($"enemy prefab was not found");
                    }
                } else if (EnemyManager.Instance.EnemyCount == 0) {
                    MoveToNextWave();
                }
            }
        }
        
        currentWaveTotalDuration += Time.deltaTime;
    }

    private void MoveToNextWave()
    {
        currentWaveIndex++;
        EventNextWave?.Invoke(currentWaveIndex);
        currentWaveTotalDuration = 0.0f;
        currentWaveEnemiesSpawned = 0;
    }

    private GameObject GetEnemyFromWave(WaveInfo wave)
    {
        var sum = wave.EnemiesAppearData.Sum(t => t.AppearScore);
        var score = Random.Range(0, sum + 1);
        var index = 0;
        int currentScore = wave.EnemiesAppearData[index].AppearScore;
        for (var i = 0; i < wave.EnemiesAppearData.Count; ++i) {
            if (score > currentScore) {
                index++;
                if (index < wave.EnemiesAppearData.Count) {
                    currentScore += wave.EnemiesAppearData[index].AppearScore;
                }
                else {
                    Debug.LogError($"Shouldn't happen");
                }
            }
        }

        return wave.EnemiesAppearData[index].EnemyPrefab;
    }


    private bool TrySpawnEnemy(GameObject enemyPrefab)
    {
        var enemy = ObjectPool.Instance.Spawn<SimpleEnemy>(enemyPrefab.gameObject, GenerateRandomPointOutsideBounds());
        if (enemy != null) {
            enemy.SetTarget(player);
            EnemyManager.Instance.RegisterEnemy(enemy);
            return true;
        }

        Debug.LogError($"Can't instantiate enemy for some reason");

        return false;
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