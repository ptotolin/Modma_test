using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileConfig", menuName = "Weapons/Projectile")]
public class ProjectileWeaponConfig : ScriptableObject
{
    [Header("Weapon Stats")]
    public string WeaponName = "Projectile Gun";
    public float Damage = 25f;
    public float FireRate = 2f;
    public float Range = 10f;
    public float ReloadTime = 2f;
    public int MaxAmmo = 30;
    
    [Header("Projectile Settings")]
    public GameObject ProjectilePrefab;
    public float ProjectileSpeed = 15f;
}