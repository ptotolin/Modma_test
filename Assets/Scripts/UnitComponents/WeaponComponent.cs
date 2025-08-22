using UnityEngine;
using System;

public class WeaponComponent : MonoBehaviour, IUnitComponent
{
    // Events (forwarded from weapon)
    public event Action EventFire;
    public event Action EventReloadStart;
    public event Action EventReloadComplete;
    public event Action EventAmmoChanged;
    public event Action<Vector2> EventAim;
    
    [Header("Weapon Settings")]
    [SerializeField] private Transform firePoint;
    
    [Header("Current Weapon")]
    [SerializeField] private MonoBehaviour weaponBehaviour;
    
    private Unit unit;
    private IWeapon currentWeapon;
    
    // Properties (forwarded from weapon)
    public IWeapon CurrentWeapon => currentWeapon;
    public bool HasWeapon => currentWeapon != null;
    public bool CanFire => HasWeapon && currentWeapon.CanFire();
    public float Damage => HasWeapon ? currentWeapon.Damage : 0f;
    public float FireRate => HasWeapon ? currentWeapon.FireRate : 0f;
    public float Range => HasWeapon ? currentWeapon.Range : 0f;
    
    public void Initialize(Unit unit)
    {
        this.unit = unit;
        
        // Find fire point if not assigned
        if (firePoint == null) {
            firePoint = transform;
        }
        
        // Initialize weapon from inspector assignment
        if (weaponBehaviour != null) {
            SetWeapon(weaponBehaviour as IWeapon);
        }
    }
    
    public void SetWeapon(IWeapon weapon)
    {
        // Unsubscribe from old weapon
        if (currentWeapon != null) {
            UnsubscribeFromWeapon(currentWeapon);
        }
        
        // Set new weapon
        currentWeapon = weapon;
        
        // Subscribe to new weapon events
        if (currentWeapon != null) {
            SubscribeToWeapon(currentWeapon);
        }
    }
    
    public bool TryFire(Vector2 direction)
    {
        if (!CanFire) return false;
        
        currentWeapon.Fire(firePoint.position, direction, unit);
        EventAim?.Invoke(direction);
        
        return true;
    }
    
    public void Reload()
    {
        if (HasWeapon) {
            currentWeapon.Reload();
        }
    }
    
    private void SubscribeToWeapon(IWeapon weapon)
    {
        weapon.EventFire += OnWeaponFire;
        weapon.EventReloadStart += OnWeaponReloadStart;
        weapon.EventReloadComplete += OnWeaponReloadComplete;
        weapon.EventAmmoChanged += OnWeaponAmmoChanged;
    }

    private void UnsubscribeFromWeapon(IWeapon weapon)
    {
        weapon.EventFire -= OnWeaponFire;
        weapon.EventReloadStart -= OnWeaponReloadStart;
        weapon.EventReloadComplete -= OnWeaponReloadComplete;
        weapon.EventAmmoChanged -= OnWeaponAmmoChanged;
    }
    
    private void OnWeaponFire() => EventFire?.Invoke();
    private void OnWeaponReloadStart() => EventReloadStart?.Invoke();
    private void OnWeaponReloadComplete() => EventReloadComplete?.Invoke();
    private void OnWeaponAmmoChanged() => EventAmmoChanged?.Invoke();
    
    public void OnDestroy()
    {
        if (currentWeapon != null) {
            UnsubscribeFromWeapon(currentWeapon);
        }
    }
}