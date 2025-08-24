using System;
using System.Collections.Generic;
using UnityEngine;

public class UIInitializer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Player player;
    
    [Header("Health UI")]
    [SerializeField] private HealthbarView healthbarView;
    
    // Presenters cache
    private List<IDisposable> presenters = new List<IDisposable>();
    
    private void Start()
    {
        InitializeUI();
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