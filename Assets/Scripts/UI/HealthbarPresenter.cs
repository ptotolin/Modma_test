using System;

public class HealthbarPresenter : IDisposable
{
    // Dependencies
    private readonly HealthbarView view;
    private readonly HealthComponent healthComponent;
    
    public HealthbarPresenter(HealthbarView view, HealthComponent healthComponent)
    {
        this.view = view;
        this.healthComponent = healthComponent;
        
        Initialize();
    }
    
    private void Initialize()
    {
        // Subscribe to health events
        healthComponent.EventHealthChanged += OnHealthChanged;
        
        // Initial update
        UpdateView();
    }
    
    private void OnHealthChanged(float current, float max)
    {
        UpdateView();
    }
    
    private void UpdateView()
    {
        view.SetHealth(healthComponent.CurrentHealth, healthComponent.MaxHealth);
    }
    
    public void Dispose()
    {
        if (healthComponent != null) {
            healthComponent.EventHealthChanged -= OnHealthChanged;
        }
    }
}