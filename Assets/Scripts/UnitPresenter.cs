using System;

public class UnitPresenter : IDisposable
{
    private HealthComponent health;
    private UnitHitAnimation animation;
    
    public UnitPresenter(Unit unit)
    {
        health = unit.GetComponent<HealthComponent>();
        animation = unit.GetComponent<UnitHitAnimation>();

        if (health != null) {
            health.EventDamageTaken += OnDamaged;
            health.EventDeath += OnDied;
        }
    }
    
    public void Dispose()
    {
        if (health != null) {
            health.EventDamageTaken -= OnDamaged;
            health.EventDeath -= OnDied;
        }
    }
    
    private void OnDamaged(float damage)
    {
        if (animation != null) {
            animation.PlayHitAnimation();
        }
    }
    
    private async void OnDied(Unit unit)
    {
        if (animation != null) {
            await animation.PlayDeathAnimation();
        }

        var resettables = unit.GetComponentsInChildren<IResettable>();
        foreach (var resettable in resettables) {
            resettable.Reset();
        }
        
        ObjectPool.Instance.Despawn(unit.gameObject);
    }
}