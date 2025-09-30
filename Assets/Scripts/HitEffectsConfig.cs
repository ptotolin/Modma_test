using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class HitEffectData
{
    public GameObjectPhysicalMaterial BulletMaterial;
    public GameObjectPhysicalMaterial TargetMaterial;
    public AssetReferenceGameObject EffectPrefab;
    
    public string GetEffectName()
    {
        if (EffectPrefab != null && EffectPrefab.RuntimeKeyIsValid()) {
            return EffectPrefab.RuntimeKey.ToString();
        }
        return "Invalid Reference";
    }
}

[CreateAssetMenu(menuName = "Effects/Bullet HitEffect")]
public class HitEffectsConfig : ScriptableObject
{
    public List<HitEffectData> HitEffects = new();
}