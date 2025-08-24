using System;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IWeapon
{
    // Events
    public event Action EventFire;
    public event Action EventReloadStart;
    public event Action EventReloadComplete;
    public event Action EventAmmoChanged;
    
    [Header("Melee Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 1.5f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private LayerMask targetLayers = -1;
    
    private float lastAttackTime;
    private Unit owner;
    
    // IWeapon implementation
    public float Damage => damage;
    public float FireRate => attackRate;
    public float Range => range;
    public float ReloadTime => 0f;
    public int MaxAmmo => -1;
    public string WeaponName => "Melee Weapon";
    
    public int CurrentAmmo => -1;
    public bool HasAmmo => true;
    public bool IsReloading => false;
    
    public bool CanFire(Vector2 targetPos)
    {
        float distance = Vector2.Distance(transform.position, targetPos);
        return distance <= range && Time.time >= lastAttackTime + (1f / attackRate);
    }
    
    public void Fire(Vector2 firePoint, Vector2 targetPos, Unit owner)
    {
        if (!CanFire(targetPos)) return;
        
        lastAttackTime = Time.time;
        this.owner = owner;
        
        // Ищем врагов в радиусе
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, targetLayers);
        
        foreach (var hit in hits) {
            Unit target = hit.GetComponentInParent<Unit>();
            if (target != null && target != owner) {
                DamageTarget(target);
            }
        }
        
        EventFire?.Invoke();
    }
    
    private void DamageTarget(Unit target)
    {
        var healthComponent = target.GetUnitComponent<HealthComponent>();
        if (healthComponent != null && healthComponent.IsAlive) {
            healthComponent.TakeDamage(damage);
            Debug.Log($"Melee attack from {owner.Name} hit {target.Name} for {damage} damage");
        }
    }
    
    public void Reload() 
    {
        EventReloadComplete?.Invoke();
    }
}