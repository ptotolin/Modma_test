using UnityEngine;

public class NonPhysicsMovementComponent : MonoBehaviour, IUnitComponent, IMovement
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    
    [Header("Movement Type")]
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float smoothTime = 0.1f;
    
    private Unit unit;
    private Vector2 currentVelocity;
    private Vector2 inputDirection;
    private Vector2 velocitySmoothing;
    
    // Properties
    public float MaxSpeed => maxSpeed;
    public Vector2 Velocity => currentVelocity;
    public Vector2 InputDirection => inputDirection;
    public bool IsMoving => currentVelocity.magnitude > 0.1f;
    
    public void Initialize(Unit unit)
    {
        this.unit = unit;
    }
    
    private void Update()
    {
        var dt = Time.deltaTime;
        UpdateMovement(dt);
        ApplyMovement(dt);
    }
    
    public void Move(Vector2 direction)
    {
        inputDirection = direction;
    }
    
    private void UpdateMovement(float dt)
    {
        Vector2 targetVelocity = inputDirection * maxSpeed;
        
        if (smoothMovement) {
            // Smooth movement using SmoothDamp
            currentVelocity = Vector2.SmoothDamp(currentVelocity, targetVelocity, 
                ref velocitySmoothing, smoothTime);
        }
        else {
            // Direct lerp movement
            if (inputDirection.magnitude > 0.1f) {
                // Accelerate
                currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, 
                    acceleration * dt);
            }
            else {
                // Decelerate
                currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, 
                    deceleration * dt);
            }
        }
    }
    
    private void ApplyMovement(float dt)
    {
        // Apply movement directly to transform
        transform.position += (Vector3)currentVelocity * dt;
    }
    
    public void SetVelocity(Vector2 velocity)
    {
        currentVelocity = velocity;
    }
    
    public void AddImpulse(Vector2 impulse)
    {
        // Simulate impulse by adding to current velocity
        currentVelocity += impulse;
        
        // Clamp to max speed
        if (currentVelocity.magnitude > maxSpeed) {
            currentVelocity = currentVelocity.normalized * maxSpeed;
        }
    }
    
    public void Stop()
    {
        currentVelocity = Vector2.zero;
        inputDirection = Vector2.zero;
        velocitySmoothing = Vector2.zero;
    }
    
    public void SetMaxSpeed(float newMaxSpeed)
    {
        maxSpeed = newMaxSpeed;
    }
    
    public void SetAcceleration(float newAcceleration)
    {
        acceleration = newAcceleration;
    }
    
    // Method to teleport without velocity
    public void Teleport(Vector3 position)
    {
        Stop();
        transform.position = position;
    }
    
    // Method to check if can move (can be overridden by derived classes)
    public virtual bool CanMove()
    {
        return unit != null && unit.IsAlive;
    }
}