using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IUnitComponent
{
    // Events
    public event Action<float, float> EventHealthChanged; // current, max
    public event Action<float> EventDamageTaken; // damage amount
    public event Action<float> EventHealed; // heal amount
    public event Action<Unit> EventDeath;
    
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    private float currentHealth;
    
    // Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsAlive => currentHealth > 0f;
    public bool IsFullHealth => Mathf.Approximately(currentHealth, maxHealth);
    
    private Unit unit;
    
    public void Initialize(Unit unit)
    {
        this.unit = unit;
        currentHealth = maxHealth;
    }
    
    public void Reset()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        if (!IsAlive || damage <= 0f) return;
        
        var previousHealth = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        
        EventDamageTaken?.Invoke(damage);
        EventHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0f && previousHealth > 0f) {
            Die();
        }
    }
    
    public void Heal(float healAmount)
    {
        if (!IsAlive || healAmount <= 0f || IsFullHealth) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        EventHealed?.Invoke(healAmount);
        EventHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        float healthPercentage = HealthPercentage;
        maxHealth = newMaxHealth;
        currentHealth = maxHealth * healthPercentage;
        
        EventHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void FullHeal()
    {
        if (!IsAlive) return;
        
        float healAmount = maxHealth - currentHealth;
        currentHealth = maxHealth;
        
        if (healAmount > 0f) {
            EventHealed?.Invoke(healAmount);
            EventHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
    
    private void Die()
    {
        EventDeath?.Invoke(unit);
    }
    
    public void OnDestroy()
    {
        // Cleanup if needed
    }
}