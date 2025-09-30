using System;
using UnityEngine;

public class Projectile : MonoBehaviour, IHasMaterial
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
    [SerializeField] private bool createHitEffect = true;

    [Header("Material")] 
    [SerializeField] private GameObjectPhysicalMaterial physicalMaterial;
    
    private Unit owner;
    private bool hasHit = false;
    private PooledObject pooledObject;
    
    // Properties
    public float Damage => damage;
    public float Speed => speed;
    public Unit Owner => owner;
    public bool HasHit => hasHit;
    // IHasMaterial
    public GameObjectPhysicalMaterial Material => physicalMaterial;

    
    private void Start()
    {
        pooledObject = GetComponent<PooledObject>();
        if (pooledObject != null) {
            pooledObject.EventSpawned += OnSpawned;
            pooledObject.EventDespawned += OnDespawned;
        }
    }
    
    private void OnSpawned()
    {
        // Reset projectile state when spawned from pool
        hasHit = false;
        owner = null;
        
        // Start lifetime timer
        Invoke(nameof(DestroyProjectile), lifetime);
    }
    
    private void OnDespawned()
    {
        // Cancel lifetime timer when returned to pool
        CancelInvoke(nameof(DestroyProjectile));
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
        if (hasHit) 
            return;
        
        // Check if target is on valid layer
        if (!IsValidTarget(other)) 
            return;
        
        // Don't hit owner
        if (owner != null && other.transform == owner.transform) 
            return;
        
        // Temporary solution
        Unit targetUnit = other.GetComponentInParent<Unit>();
        if (targetUnit != null) {
            DamageTarget(targetUnit);
        }
        
        // Create hit effect
        if (createHitEffect) {
            IHasMaterial targetObject = other.GetComponentInParent<IHasMaterial>();
            if (targetObject != null) {
                CreateHitEffect(other.ClosestPoint(transform.position), targetObject.Material);
            }
            else {
                Debug.LogWarning($"No hit effect found for bullet because IHasMaterial");
            }
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
            
            //Debug.Log($"Projectile from {(owner != null ? owner.Name : "Unknown")} hit {target.Name} for {damage} damage");
        }
    }
    
    private void CreateHitEffect(Vector3 hitPosition, GameObjectPhysicalMaterial targetMaterial)
    {
        Debug.Log($"[Client] === Creating Hit Effect ===");
        Debug.Log($"[Client] Position: {hitPosition}");
        Debug.Log($"[Client] Bullet Material: {Material?.name ?? "NULL"}");
        Debug.Log($"[Client] Target Material: {targetMaterial?.name ?? "NULL"}");
        
        var hitEffect = BulletHitEffectsRepository.Instance.GetEffectPrefab(Material, targetMaterial);
        Debug.Log($"[Client] Hit Effect Prefab: {hitEffect?.name ?? "NULL"}");
        if (hitEffect != null) {
            Debug.Log("[Client] ✅ Instantiating effect...");
            GameObject effect = Instantiate(hitEffect, hitPosition, Quaternion.identity);
            
            // Auto-destroy effect after some time
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null) {
                Destroy(effect, particles.main.duration + particles.main.startLifetime.constantMax);
            } else {
                Destroy(effect, 2f); // Default 2 seconds
            }
        } else {
            Debug.LogError("[Client] ❌ Hit Effect is NULL!");
        }
    }
    
    private void DestroyProjectile()
    {
        EventDestroyed?.Invoke();
        ObjectPool.Instance.Despawn(this.gameObject);
    }
    
    private void OnDestroy()
    {
        if (pooledObject != null) {
            pooledObject.EventSpawned -= OnSpawned;
            pooledObject.EventDespawned -= OnDespawned;
        }
    }
}