using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float IdleTime = 0.3f;
    
    [Header("Input Settings")]
    [SerializeField] private bool useVirtualJoystick = true;
    [SerializeField] private VirtualJoystick virtualJoystick;

    private IMovement movement;
    private Vector2 inputDirection;
    private Unit unit;
    private bool moving;
    private bool idling = true;
    private float idleTimer = 0.0f;

    public bool Idling => idling;

    private void Awake()
    {
        // Получаем Unit компонент
        unit = GetComponent<Unit>();
        
        if (unit == null) {
            Debug.LogError($"PlayerController on {gameObject.name} requires Unit component!");
            return;
        }
        
        // Подписываемся на событие инициализации Unit
        unit.EventInitialized += OnUnitInitialized;
        
        if (virtualJoystick == null && useVirtualJoystick) {
            virtualJoystick = FindObjectOfType<VirtualJoystick>();
        }
        
        if (virtualJoystick != null) {
            virtualJoystick.OnJoystickMoved += OnJoystickInput;
        }
    }

    private void OnUnitInitialized(Unit unit)
    {
        unit.EventInitialized -= OnUnitInitialized;
        // Get movement component
        movement = GetComponent<IMovement>();
        
        if (movement == null) {
            Debug.LogError($"PlayerController on {gameObject.name} requires a component that implements IMovement!");
            return;
        }
    }

    private void OnDestroy()
    {
        if (unit != null) {
            unit.EventInitialized -= OnUnitInitialized;
        }
        
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
        movement?.Move(inputDirection);
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

        if (moving && inputDirection.magnitude < 0.01f) {
            moving = false;
        } else if (!moving && inputDirection.magnitude > 0.01f) {
            moving = true;
        }

        if (!moving) {
            idleTimer += Time.deltaTime;
            if (idleTimer > IdleTime) {
                idling = true;
            }
        }
        else {
            idling = false;
            idleTimer = 0.0f;
        }
    }
    

    
    private void OnJoystickInput(Vector2 direction)
    {
    }
    
    // Public methods for getting movement information
    public Vector2 GetCurrentVelocity() => movement?.Velocity ?? Vector2.zero;
    public Vector2 GetInputDirection() => inputDirection;
    public bool IsMoving() => movement?.IsMoving ?? false;
    public float GetMaxSpeed() => movement?.MaxSpeed ?? 0f;
    
    // Methods for external control
    public void SetVelocity(Vector2 velocity)
    {
        movement?.SetVelocity(velocity);
    }
    
    public void Stop()
    {
        movement?.Stop();
        inputDirection = Vector2.zero;
    }
    
    public void SetMaxSpeed(float newMaxSpeed)
    {
        movement?.SetMaxSpeed(newMaxSpeed);
    }
}