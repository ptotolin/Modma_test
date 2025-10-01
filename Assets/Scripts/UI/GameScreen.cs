using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameScreen : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Player player;
    
    [Header("Health UI")]
    [SerializeField] private HealthbarView healthbarView;

    [Header("FPS Counter")]
    [SerializeField] private FPSCounter fpsCounter;

    [SerializeField] private EnemySpawner enemySpawner;
    
    [Header("Next wave effect")] 
    [SerializeField] private WaveIndicationPresenter wavePresenter;
    
    // Presenters cache
    private List<IDisposable> presenters = new List<IDisposable>();

    private void OnEnable()
    {
        DebugLogOnGUI.Instance.WatchVariable("fpsCounter", GetCurrentFPS);
        enemySpawner.EventNextWave += OnNextWave;
    }

    private void OnDisable()
    {
        DebugLogOnGUI.Instance.UnwatchVariable("fpsCounter");
        enemySpawner.EventNextWave -= OnNextWave;
    }

    private async void OnNextWave(int index)
    {
        Debug.Log($"[Client]OnNextWave({index})");
        // TODO: optimize it (either pool or object to reactivate)
        wavePresenter.Show(index + 1);

        await UniTask.Delay(3000);
        wavePresenter.Hide();
    }

    private void Start()
    {
        InitializeUI();
    }

    public UniTask Initialize()
    {
        InitializeUI();
        return UniTask.CompletedTask;
    }
    
    private object GetCurrentFPS()
    {
        return fpsCounter.CurrentFPS;
    }

    
    private void InitializeUI()
    {
        if (player == null) {
            Debug.LogError($"UIInitializer on {gameObject.name}: Player is not assigned!");
            return;
        }
        
        // Initialize healthbar
        if (healthbarView != null) {
            var healthComponent = player.GetUnitComponent<HealthComponent>();
            if (healthComponent != null) {
                var healthPresenter = new HealthbarPresenter(healthbarView, healthComponent);
                presenters.Add(healthPresenter);
            }
        }
    }
    
    private void OnDestroy()
    {
        // Cleanup all presenters
        foreach (var presenter in presenters) {
            presenter?.Dispose();
        }
        presenters.Clear();
    }
}
