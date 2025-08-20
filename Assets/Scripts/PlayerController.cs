using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    
    [Header("Physics Settings")]
    [SerializeField] private bool usePhysicsMovement = true;
    [SerializeField] private float drag = 5f;
    [SerializeField] private ForceMode2D forceMode = ForceMode2D.Force;
    
    [Header("Input Settings")]
    [SerializeField] private bool useVirtualJoystick = true;
    [SerializeField] private VirtualJoystick virtualJoystick;

    private Transform curTransform;
    private Rigidbody2D rb;
    private Vector2 inputDirection;

    private void Awake()
    {
        curTransform = transform;
        rb = GetComponent<Rigidbody2D>();
        
        // Create Rigidbody2D if it doesn't exist
        if (rb == null) {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure Rigidbody2D for top-down movement
        rb.gravityScale = 0f; // No gravity for top-down games
        rb.drag = drag;
        rb.freezeRotation = true; // Prevent rotation
        
        if (virtualJoystick == null && useVirtualJoystick) {
            virtualJoystick = FindObjectOfType<VirtualJoystick>();
        }
        
        if (virtualJoystick != null) {
            virtualJoystick.OnJoystickMoved += OnJoystickInput;
        }
    }

    private void OnDestroy()
    {
        if (virtualJoystick != null) {
            virtualJoystick.OnJoystickMoved -= OnJoystickInput;
        }
    }

    private void Update()
    {
        HandleInput();
    }
    
    private void FixedUpdate()
    {
        if (usePhysicsMovement) {
            ApplyPhysicsMovement();
        }
        else {
            ApplyDirectMovement();
        }
    }
    
    private void HandleInput()
    {
        if (useVirtualJoystick && virtualJoystick != null) {
            inputDirection = virtualJoystick.GetInputDirection();
        }
        else {
            inputDirection.x = 0f;
            inputDirection.y = 0f;
            
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                inputDirection.x = -1f;
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                inputDirection.x = 1f;
                
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                inputDirection.y = 1f;
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                inputDirection.y = -1f;
        }
    }
    
    private void ApplyPhysicsMovement()
    {
        Vector2 force = inputDirection * acceleration;
        
        // Apply force to rigidbody
        rb.AddForce(force, forceMode);
        
        // Clamp velocity to max speed
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }
    
    private void ApplyDirectMovement()
    {
        // Direct velocity control (alternative method)
        Vector2 targetVelocity = inputDirection * maxSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
    }
    
    private void OnJoystickInput(Vector2 direction)
    {
    }
    
    // Public methods for getting movement information
    public Vector2 GetCurrentVelocity() => rb != null ? rb.velocity : Vector2.zero;
    public Vector2 GetInputDirection() => inputDirection;
    public bool IsMoving() => rb != null && rb.velocity.magnitude > 0.1f;
    public Rigidbody2D GetRigidbody() => rb;
    
    // Methods for external control
    public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
    {
        if (rb != null) {
            rb.AddForce(force, mode);
        }
    }
    
    public void SetVelocity(Vector2 velocity)
    {
        if (rb != null) {
            rb.velocity = velocity;
        }
    }
    
    public void Stop()
    {
        if (rb != null) {
            rb.velocity = Vector2.zero;
        }
    }
}