using UnityEngine;

public class SimpleEnemy : Unit
{
    private Transform targetTransform;
    private Unit target;
    private HealthComponent targetHealthComponent;
    private IMovement movementComponent;
    private WeaponComponent weaponComponent;
    private float checkTime = 0.5f;
    private float checkTimer = 0.0f;

    protected override void Start()
    {
        base.Start();
        
        movementComponent = GetMovement();
        weaponComponent = GetComponent<WeaponComponent>();
    }
    
    public void SetTarget(Unit target)
    {
        this.target = target;
        targetTransform = target.transform;

        targetHealthComponent = target.GetComponent<HealthComponent>();
        targetHealthComponent.EventDeath += OnDie;
    }

    private void OnDie(Unit unit)
    {
        target = null;
        targetHealthComponent.EventDeath -= OnDie;
    }

    private void Update()
    {
        if (target == null) {
            return;
        }
        
        movementComponent?.Move(targetTransform.position - transform.position);
        weaponComponent.TryFire(targetTransform.position);
    }
}