using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HitEffectData
{
    public GameObjectPhysicalMaterial BulletMaterial;
    public GameObjectPhysicalMaterial TargetMaterial;
    public GameObject EffectPrefab;
}

[CreateAssetMenu(menuName = "Effects/Bullet HitEffect")]
public class HitEffectsConfig : ScriptableObject
{
    public List<HitEffectData> HitEffects = new();
}