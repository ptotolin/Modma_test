using UnityEngine;

public class SimpleEnemy : Unit
{
    private Transform target;
    private IMovement movementComponent;
    private float checkTime = 0.5f;
    private float checkTimer = 0.0f;

    protected override void Start()
    {
        base.Start();
        
        movementComponent = GetMovement();
    }
    
    public void SetTarget(Transform target)
    {
        this.target = target;
    }
    
    private void Update()
    {
        movementComponent?.Move(target.position - transform.position);
        // if (checkTimer > checkTime) {
        //     checkTimer -= checkTime;
        //     // Update direction
        //     
        // }
        // else {
        //     checkTimer += Time.deltaTime;
        // }
    }
}
