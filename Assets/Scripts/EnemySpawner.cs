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
    public float BeforeTime;
    public float Duration;
    public float AfterTime;
    public List<EnemyAppearData> EnemiesAppearData;
    public float NewEnemySpawnTime;
}

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
    [SerializeField] private float newEnemySpawnTime = 20;
    [SerializeField] private List<WaveInfo> waves = new();
    
    private float spawnTimer = 0.0f;
    private WorldBounds worldBounds;
    private bool playerDied;
    private int currentWaveIndex;
    private float currentWaveTotalDuration;

    private void Start()
    {
        worldBounds = WorldBounds.Instance;
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
        var currentWave = waves[currentWaveIndex];
        if (currentWaveTotalDuration <= currentWave.BeforeTime) {
            //Debug.Log($"[Client] Entered Before wave phase");
        } else 
        if (currentWaveTotalDuration > currentWave.BeforeTime && 
            currentWaveTotalDuration < currentWave.BeforeTime + currentWave.Duration) {
            //Debug.Log($"[Client] Entered wave phase");
            if (EnemyManager.Instance.EnemyCount < maxEnemies) {
                spawnTimer += Time.deltaTime;
                if (spawnTimer > currentWave.NewEnemySpawnTime) {
                    spawnTimer -= currentWave.NewEnemySpawnTime;

                    var enemyPrefab = GetEnemyFromWave(currentWave);
                    if (enemyPrefab != null) {
                        SpawnEnemy(enemyPrefab);
                    }
                    else {
                        Debug.LogError($"enemy prefab was not found");
                    }
                }
            }
        } else if (currentWaveTotalDuration > currentWave.BeforeTime + currentWave.Duration && 
                   currentWaveTotalDuration < currentWave.BeforeTime + currentWave.Duration + currentWave.AfterTime) {
            // after wave period
            //Debug.Log($"[Client] Entered After wave phase");
        }
        else {
            if (currentWaveIndex < waves.Count - 1) {
                currentWaveIndex++;
                currentWaveTotalDuration = 0.0f;
            }
            else {
                Debug.Break();
            }
        }
        
        currentWaveTotalDuration += Time.deltaTime;
       
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


    private void SpawnEnemy(GameObject enemyPrefab)
    {
        var enemy = ObjectPool.Instance.Spawn<SimpleEnemy>(enemyPrefab.gameObject, GenerateRandomPointOutsideBounds());
        if (enemy != null) {
            enemy.SetTarget(player);
            EnemyManager.Instance.RegisterEnemy(enemy);
        }
        else {
            Debug.LogError($"Can't instantiate enemy for some reason");
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