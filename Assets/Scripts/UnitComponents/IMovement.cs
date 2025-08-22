using UnityEngine;

public interface IMovement
{
    // Properties
    float MaxSpeed { get; }
    Vector2 Velocity { get; }
    Vector2 InputDirection { get; }
    bool IsMoving { get; }
    
    // Movement methods
    void Move(Vector2 direction);
    void Stop();
    void SetVelocity(Vector2 velocity);
    
    // Configuration methods
    void SetMaxSpeed(float newMaxSpeed);
}