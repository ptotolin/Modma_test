using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BulletHitEffectsRepository
{
    private static BulletHitEffectsRepository _instance;
    private Dictionary<(GameObjectPhysicalMaterial, GameObjectPhysicalMaterial), GameObject> hitEffects;
    
    public static BulletHitEffectsRepository Instance => _instance ??= new BulletHitEffectsRepository();

    private BulletHitEffectsRepository()
    {
        hitEffects = new Dictionary<(GameObjectPhysicalMaterial, GameObjectPhysicalMaterial), GameObject>();
        LoadFromAddressables();
    }

    public GameObject GetEffectPrefab(GameObjectPhysicalMaterial bulletMaterial,
        GameObjectPhysicalMaterial targetMaterial)
    {
        if (hitEffects != null) {
            if (hitEffects.TryGetValue((bulletMaterial, targetMaterial), out var prefab)) {
                return prefab;
            }
        }

        return null;
    }
    
    private async void LoadFromAddressables()
    {
        var config = await Addressables.LoadAssetAsync<HitEffectsConfig>("Assets/Configs/HitEffectsConfig.asset");
        if (config != null) {
            foreach (var holder in config.HitEffects) {
                hitEffects[(holder.BulletMaterial, holder.TargetMaterial)] = holder.EffectPrefab;
            }
        }
    }
}