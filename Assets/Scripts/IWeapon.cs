using System;
using UnityEngine;

public interface IWeapon
{
    // Events
    event Action EventFire;
    event Action EventReloadStart;
    event Action EventReloadComplete;
    event Action EventAmmoChanged;
    
    // Weapon properties
    float Damage { get; }
    float FireRate { get; }
    float Range { get; }
    float ReloadTime { get; }
    int MaxAmmo { get; }
    string WeaponName { get; }
    
    // Ammo system
    int CurrentAmmo { get; }
    bool HasAmmo { get; }
    bool IsReloading { get; }
    
    // Weapon actions
    bool CanFire();
    void Fire(Vector3 firePoint, Vector2 direction, Unit owner);
    void Reload();
}