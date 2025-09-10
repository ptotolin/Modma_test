using System;
using UnityEngine;

public class PhysicsBasedMovement : MonoBehaviour, IUnitComponent, IMovement
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float drag = 5f;
    
    [Header("Physics")]
    [SerializeField] private ForceMode2D forceMode = ForceMode2D.Force;
    
    private Unit unit;
    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private float currentMaxSpeed;
    
    // Properties
    public float MaxSpeed => maxSpeed;
    public Vector2 Velocity => rb != null ? rb.velocity : Vector2.zero;
    public Vector2 InputDirection => inputDirection;
    public bool IsMoving => Velocity.magnitude > 0.1f;
    
    public void Initialize(Unit unit)
    {
        this.unit = unit;
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null) {
            Debug.LogWarning($"PhysicsBasedMovement on {gameObject.name} requires Rigidbody2D!");
            return;
        }
        
        // Configure rigidbody
        rb.gravityScale = 0f;
        rb.drag = drag;
        rb.freezeRotation = true;
        
        Reset();
    }

    public void Reset()
    {
        currentMaxSpeed = maxSpeed;
        inputDirection = Vector2.zero;
        rb.velocity = Vector2.zero;
    }

    public void Move(Vector2 direction)
    {
        inputDirection = direction;
        
        if (rb == null) return;
        
        Vector2 force = direction.normalized * acceleration;
        rb.AddForce(force, forceMode);
        
        // Clamp velocity
        if (rb.velocity.magnitude > currentMaxSpeed) {
            rb.velocity = rb.velocity.normalized * currentMaxSpeed;
        }
    }
    
    public void SetVelocity(Vector2 velocity)
    {
        if (rb != null) {
            rb.velocity = velocity;
        }
    }
    
    public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
    {
        if (rb != null) {
            rb.AddForce(force, mode);
        }
    }
    
    public void Stop()
    {
        if (rb != null) {
            rb.velocity = Vector2.zero;
        }
    }
    
    public void SetMaxSpeed(float newMaxSpeed)
    {
        currentMaxSpeed = newMaxSpeed;
    }
}