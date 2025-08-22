using System;
using System.Collections;
using UnityEngine;

public class ProjectileWeapon : MonoBehaviour, IWeapon
{
    // Events
    public event Action EventFire;
    public event Action EventReloadStart;
    public event Action EventReloadComplete;
    public event Action EventAmmoChanged;
    
    [SerializeField] private ProjectileWeaponConfig config;
    
    // Runtime state (each instance will have its own)
    private int currentAmmo;
    private bool isReloading = false;
    private float lastFireTime;
    
    // Properties
    public float Damage => config.Damage;
    public float FireRate => config.FireRate;
    public float Range => config.Range;
    public float ReloadTime => config.ReloadTime;
    public int MaxAmmo => config.MaxAmmo;
    public string WeaponName => config.WeaponName;
    
    public int CurrentAmmo => currentAmmo;
    public bool HasAmmo => config.MaxAmmo == -1 || currentAmmo > 0;
    public bool IsReloading => isReloading;
    
    private void Awake()
    {
        Initialize();
    }
    
    // Initialize weapon (called when equipped)
    public void Initialize()
    {
        currentAmmo = config.MaxAmmo;
        isReloading = false;
        lastFireTime = 0f;
    }
    
    public bool CanFire()
    {
        return HasAmmo && !isReloading && Time.time >= lastFireTime + (1f / config.FireRate);
    }
    
    public void Fire(Vector3 firePoint, Vector2 direction, Unit owner)
    {
        if (!CanFire()) return;
        
        lastFireTime = Time.time;
        
        // Consume ammo
        if (config.MaxAmmo > 0) {
            currentAmmo--;
            EventAmmoChanged?.Invoke();
        }
        
        // Create projectile
        if (config.ProjectilePrefab != null) {
            CreateProjectile(firePoint, direction, owner);
        }
        
        EventFire?.Invoke();
    }
    
    public void Reload()
    {
        if (isReloading || config.MaxAmmo == -1 || currentAmmo == config.MaxAmmo) return;
        
        StartCoroutine(ReloadCoroutine());
    }
    
    private void CreateProjectile(Vector3 firePoint, Vector2 direction, Unit owner)
    {
        var projectileObject = Instantiate(config.ProjectilePrefab, firePoint, Quaternion.identity);
        
        // Setup projectile
        var projectile = projectileObject.GetComponent<Projectile>();
        if (projectile != null) {
            projectile.SetDamage(config.Damage);
            projectile.SetSpeed(config.ProjectileSpeed);
            projectile.SetOwner(owner);
            projectile.Launch(direction);
        }
    }
    
    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        EventReloadStart?.Invoke();
        
        yield return new WaitForSeconds(config.ReloadTime);
        
        currentAmmo = config.MaxAmmo;
        isReloading = false;
        
        EventReloadComplete?.Invoke();
        EventAmmoChanged?.Invoke();
    }
}