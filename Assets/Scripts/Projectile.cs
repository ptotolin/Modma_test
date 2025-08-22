using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Events
    public event Action<Unit> EventHitTarget;
    public event Action EventDestroyed;
    
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private LayerMask targetLayers = -1;
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private bool createHitEffect = true;
    
    private Unit owner;
    private bool hasHit = false;
    
    // Properties
    public float Damage => damage;
    public float Speed => speed;
    public Unit Owner => owner;
    public bool HasHit => hasHit;
    
    private void Start()
    {
        // Auto-destroy after lifetime to prevent memory leaks
        Destroy(gameObject, lifetime);
    }
    
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    public void SetOwner(Unit ownerUnit)
    {
        owner = ownerUnit;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
    }
    
    public void Launch(Vector2 direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.velocity = direction.normalized * speed;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }
    
    private void HandleCollision(Collider2D other)
    {
        // Prevent multiple hits
        if (hasHit) return;
        
        // Check if target is on valid layer
        if (!IsValidTarget(other)) return;
        
        // Don't hit owner
        if (owner != null && other.transform == owner.transform) return;
        
        // Try to damage unit
        Unit targetUnit = other.GetComponent<Unit>();
        if (targetUnit != null) {
            DamageTarget(targetUnit);
        }
        
        // Create hit effect
        if (createHitEffect) {
            CreateHitEffect(other.ClosestPoint(transform.position));
        }
        
        hasHit = true;
        EventHitTarget?.Invoke(targetUnit);
        
        // Destroy projectile if needed
        if (destroyOnHit) {
            DestroyProjectile();
        }
    }
    
    private bool IsValidTarget(Collider2D other)
    {
        // Check if other is on target layers
        return (targetLayers.value & (1 << other.gameObject.layer)) != 0;
    }
    
    private void DamageTarget(Unit target)
    {
        HealthComponent targetHealth = target.GetUnitComponent<HealthComponent>();
        if (targetHealth != null && targetHealth.IsAlive) {
            targetHealth.TakeDamage(damage);
            
            Debug.Log($"Projectile from {(owner != null ? owner.Name : "Unknown")} hit {target.Name} for {damage} damage");
        }
    }
    
    private void CreateHitEffect(Vector3 hitPosition)
    {
        if (hitEffect != null) {
            GameObject effect = Instantiate(hitEffect, hitPosition, Quaternion.identity);
            
            // Auto-destroy effect after some time
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            if (particles != null) {
                Destroy(effect, particles.main.duration + particles.main.startLifetime.constantMax);
            } else {
                Destroy(effect, 2f); // Default 2 seconds
            }
        }
    }
    
    private void DestroyProjectile()
    {
        EventDestroyed?.Invoke();
        Destroy(gameObject);
    }
    
    // Public method to manually destroy projectile
    public void ForceDestroy()
    {
        DestroyProjectile();
    }
    
    // Method to check if projectile can hit specific target
    public bool CanHitTarget(Unit target)
    {
        if (hasHit) return false;
        if (owner != null && target == owner) return false;
        
        return IsValidTarget(target.GetComponent<Collider2D>());
    }
}