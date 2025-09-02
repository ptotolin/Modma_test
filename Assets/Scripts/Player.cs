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

    private PlayerController playerController;
    
    protected override void Start()
    {
        base.Start();
        
        playerController = GetComponent<PlayerController>();
        weaponComponent = GetComponent<WeaponComponent>();
        prevIdling = playerController.Idling;
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
            if (weaponComponent.TryFire(currentTarget.transform.position)) {
                TryPlayFireAnimation();
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
            currentTarget.EventDestroyed -= OnTargetDestroyed;
        }
        
        // search for closest enemy
        currentTarget = EnemyManager.Instance.GetClosestEnemy(transform.position);

        if (currentTarget != null) {
            currentTarget.EventDestroyed += OnTargetDestroyed;
        }
    }

    private void OnTargetDestroyed(Unit unit)
    {
        currentTarget = null;
    }


    // private void OnGUI()
    // {
    //     if (rb != null) {
    //         style = new GUIStyle();
    //         style.fontSize = 40;
    //         
    //         var sb = new StringBuilder();
    //         sb.AppendLine($"Player speed: {rb.velocity.magnitude}");
    //         sb.AppendLine($"max speed: {movement.MaxSpeed}");
    //         var str = sb.ToString();
    //         GUI.Label(new Rect(10, 10, 200, 30), $"Player speed = {str}", style);
    //     }
    // }
}
