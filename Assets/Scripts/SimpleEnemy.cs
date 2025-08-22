using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : Unit
{
    private Transform target;
    private IMovement movementComponent;
    private float checkTime = 0.5f;
    private float checkTimer = 0.0f;

    protected override void Awake()
    {
        base.Awake();
        
        movementComponent = GetMovement();
    }
    
    public void SetTarget(Transform target)
    {
        this.target = target;
    }
    
    private void Update()
    {
        if (checkTimer > checkTime) {
            checkTimer -= checkTime;
            // Update direction
            if (movementComponent != null) {
                movementComponent.Move(target.position - transform.position);
            }
        }
        else {
            checkTimer += Time.deltaTime;
        }
    }
}
