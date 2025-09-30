using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BulletHitEffectsRepository : IDisposable
{
    private static BulletHitEffectsRepository _instance;
    private Dictionary<(GameObjectPhysicalMaterial, GameObjectPhysicalMaterial), GameObject> hitEffects;
    
    public static BulletHitEffectsRepository Instance => _instance ??= new BulletHitEffectsRepository();

    private BulletHitEffectsRepository()
    {
        hitEffects = new Dictionary<(GameObjectPhysicalMaterial, GameObjectPhysicalMaterial), GameObject>();
    }

    public async UniTask Initialize()
    {
        await LoadFromAddressables();
    }

    public GameObject GetEffectPrefab(GameObjectPhysicalMaterial bulletMaterial,
        GameObjectPhysicalMaterial targetMaterial)
    {
        Debug.Log($"[Repository] === GetEffectPrefab Called ===");
        Debug.Log($"[Repository] Input Bullet Material: {bulletMaterial?.name ?? "NULL"} (Instance ID: {bulletMaterial?.GetInstanceID() ?? -1})");
        Debug.Log($"[Repository] Input Target Material: {targetMaterial?.name ?? "NULL"} (Instance ID: {targetMaterial?.GetInstanceID() ?? -1})");
        Debug.Log($"[Repository] Dictionary has {hitEffects?.Count ?? 0} entries");
        
        if (hitEffects != null) {
            Debug.Log("[Repository] === Dictionary Contents ===");
            int index = 0;
            foreach (var kvp in hitEffects) {
                var dictBullet = kvp.Key.Item1;
                var dictTarget = kvp.Key.Item2;
                Debug.Log($"[Repository] Entry {index}: Bullet='{dictBullet?.name ?? "NULL"}' (ID: {dictBullet?.GetInstanceID() ?? -1}), Target='{dictTarget?.name ?? "NULL"}' (ID: {dictTarget?.GetInstanceID() ?? -1})");
                index++;
            }
            
            Debug.Log("[Repository] === Comparison Results ===");
            Debug.Log($"[Repository] Bullet materials equal: {bulletMaterial == hitEffects.Keys.FirstOrDefault().Item1}");
            Debug.Log($"[Repository] Target materials equal: {targetMaterial == hitEffects.Keys.FirstOrDefault().Item2}");
            Debug.Log($"[Repository] Bullet ReferenceEquals: {ReferenceEquals(bulletMaterial, hitEffects.Keys.FirstOrDefault().Item1)}");
            Debug.Log($"[Repository] Target ReferenceEquals: {ReferenceEquals(targetMaterial, hitEffects.Keys.FirstOrDefault().Item2)}");
            
            // Try direct match first
            if (hitEffects.TryGetValue((bulletMaterial, targetMaterial), out var prefab)) {
                Debug.Log($"[Repository] ✅ Found direct match: {prefab?.name ?? "NULL"}");
                return prefab;
            }
            
            // Try reversed order
            if (hitEffects.TryGetValue((targetMaterial, bulletMaterial), out prefab)) {
                Debug.Log($"[Repository] ✅ Found reversed match: {prefab?.name ?? "NULL"}");
                return prefab;
            }
            
            // Try matching by name (fallback for different instances)
            foreach (var kvp in hitEffects) {
                var dictBullet = kvp.Key.Item1;
                var dictTarget = kvp.Key.Item2;
                
                bool bulletMatch = (bulletMaterial?.name == dictBullet?.name) || (bulletMaterial?.name == dictTarget?.name);
                bool targetMatch = (targetMaterial?.name == dictTarget?.name) || (targetMaterial?.name == dictBullet?.name);
                
                if (bulletMatch && targetMatch) {
                    Debug.Log($"[Repository] ✅ Found name-based match: {kvp.Value?.name ?? "NULL"}");
                    return kvp.Value;
                }
            }
            
            Debug.LogError("[Repository] ❌ No matching combination found in dictionary!");
            Debug.LogError($"[Repository] Searched for: ({bulletMaterial?.name}, {targetMaterial?.name})");
        } else {
            Debug.LogError("[Repository] ❌ hitEffects dictionary is NULL!");
        }

        return null;
    }

    public void Dispose()
    {
        foreach (var kvp in hitEffects)
        {
            if (kvp.Value != null) {
                Addressables.Release(kvp.Value);
            }
        }
        
        hitEffects?.Clear();
    }
    
    private async UniTask LoadFromAddressables()
    {
        Debug.Log("[Client] === Loading HitEffectsConfig ===");
        try {
            var config = await Addressables.LoadAssetAsync<HitEffectsConfig>("Assets/Configs/HitEffectsConfig.asset");
            if (config != null) {
                Debug.Log($"[Client] ✅ Config loaded: {config.name}");
                Debug.Log($"[Client] Found {config.HitEffects.Count} hit effects");
                foreach (var holder in config.HitEffects) {
                    Debug.Log($"[Client] Loading prefab {holder.GetEffectName()}");
                    Debug.Log($"[Client] Config Bullet Material: {holder.BulletMaterial?.name ?? "NULL"} (Instance ID: {holder.BulletMaterial?.GetInstanceID() ?? -1})");
                    Debug.Log($"[Client] Config Target Material: {holder.TargetMaterial?.name ?? "NULL"} (Instance ID: {holder.TargetMaterial?.GetInstanceID() ?? -1})");
                    
                    var prefab = await holder.EffectPrefab.LoadAssetAsync<GameObject>();
                    if (prefab != null) {
                        hitEffects[(holder.BulletMaterial, holder.TargetMaterial)] = prefab;
                        Debug.Log($"[Client] ✅ Added to dictionary: ({holder.BulletMaterial?.name}, {holder.TargetMaterial?.name}) → {prefab.name}");
                    } else {
                        Debug.LogError($"[Client] ❌ Failed to load prefab for {holder.GetEffectName()}");
                    }

                    Debug.Log($"[Client] Effect: {prefab?.name ?? "NULL"}");
                }
            }
            else {
                Debug.LogError("[Client] ❌ Config is NULL!");
            }
        } catch (System.Exception e)
        {
            Debug.LogError($"[Client] ❌ Exception: {e.Message}");
        }
    }
}