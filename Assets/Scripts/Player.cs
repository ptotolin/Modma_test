using System.Text;
using UnityEngine;

[RequireComponent(typeof(WeaponComponent))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class Player : Unit
{
    [SerializeField] private Animation anim;
    [SerializeField] private AnimationClip fireAnimationClip;
    
    private GUIStyle style;
    private Unit currentTarget;
    private bool prevIdling;
    private WeaponComponent weaponComponent;
    private Rigidbody2D rb;

    private PlayerController playerController;
    
    protected override void Start()
    {
        base.Start();
        
        playerController = GetComponent<PlayerController>();
        weaponComponent = GetComponent<WeaponComponent>();
        prevIdling = playerController.Idling;
        // Debug
        rb = GetComponent<Rigidbody2D>();
        DebugLogOnGUI.Instance.Initialize(40);
        DebugLogOnGUI.Instance.WatchVariable("player speed", GetSpeed);
        DebugLogOnGUI.Instance.WatchVariable("current target", GetCurrentTarget);
    }

    private void OnDestroy()
    {
        DebugLogOnGUI.Instance.UnwatchVariable("player speed");
        DebugLogOnGUI.Instance.UnwatchVariable("current target");
    }

    private object GetSpeed()
    {
        if (rb != null) {
            return rb.velocity.magnitude;
        }

        return null;
    }

    private object GetCurrentTarget()
    {
        return currentTarget;
    }

    private void Update()
    {
        if (!prevIdling && playerController.Idling) {
            SetNewTarget();
        }
        else if (currentTarget == null) {
            SetNewTarget();
        }

        if (currentTarget != null) {
            if (weaponComponent.TryFire(currentTarget.transform.position, out var fireFailureReason)) {
                TryPlayFireAnimation();
            } else if ((fireFailureReason & (int)FireFailureReason.NoReason) == 0) {
                if ((fireFailureReason & (int)FireFailureReason.OutOfRange) != 0) {
                    currentTarget = null;
                }
            }
        }
        
        prevIdling = playerController.Idling;
    }

    private void TryPlayFireAnimation()
    {
        if (anim != null && fireAnimationClip != null) {
            anim.Play(fireAnimationClip.name);
        }
    }

    private void SetNewTarget()
    {
        if (currentTarget != null) {
            currentTarget.GetUnitComponent<HealthComponent>().EventDeath -= OnTargetDestroyed;
        }
        
        // search for closest enemy
        currentTarget = EnemyManager.Instance.GetClosestEnemy(transform.position);

        if (currentTarget != null) {
            currentTarget.GetUnitComponent<HealthComponent>().EventDeath += OnTargetDestroyed;
        }
    }

    private void OnTargetDestroyed(Unit unit)
    {
        currentTarget = null;
    }


    // private void OnGUI()
    // {
    //     style = new GUIStyle();
    //     style.fontSize = 40;
    //
    //     var sb = new StringBuilder();
    //     var currentTargetStr = currentTarget == null ? "null" : currentTarget.gameObject.name;
    //     sb.AppendLine($"current target: {currentTargetStr}");
    //     sb.AppendLine($"current speed: {rb.velocity}");
    //     var str = sb.ToString();
    //     GUI.Label(new Rect(10, 150, 200, 30), $"{str}", style);
    // }
}
