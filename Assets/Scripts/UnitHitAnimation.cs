using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UnitHitAnimation : MonoBehaviour, IResettable
{
    [SerializeField] private Animation anim;
    [SerializeField] private AnimationClip hitAnimClip;
    [SerializeField] private AnimationClip deathAnimClip;

    public bool IsPlayingDeath => deathAnimClip != null && anim != null && anim.IsPlaying(deathAnimClip.name);
    public bool IsPlayingHit => hitAnimClip != null && anim != null && anim.IsPlaying(hitAnimClip.name);

    private CancellationTokenSource cts;

    private void Awake()
    {
        cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        cts.Cancel();
        cts.Dispose();
    }

    public async UniTask PlayHitAnimation()
    {
        if (anim != null && hitAnimClip != null) {
            if (!anim.IsPlaying(hitAnimClip.name)) {
                anim.Play(hitAnimClip.name);

                while (anim.IsPlaying(hitAnimClip.name)) {
                    if (cts.IsCancellationRequested)
                        break;
                    
                    await UniTask.Yield();
                }
            }
        }
    }

    public async UniTask PlayDeathAnimation()
    {
        if (anim != null && deathAnimClip != null) {
            anim.Play(deathAnimClip.name);
            
            while (anim.IsPlaying(deathAnimClip.name)) {
                if (cts.IsCancellationRequested) {
                    break;
                }
                await UniTask.Yield();
            }
        }
    }

    public void Reset()
    {
        if (anim != null && deathAnimClip != null) {
            cts.Cancel();
            cts.Dispose();
            
            anim.Stop();
            anim.Rewind();
        }
    }
}
