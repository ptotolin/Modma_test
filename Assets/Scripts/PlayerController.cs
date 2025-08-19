using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float decceleration = 10f;
    
    [Header("Input Settings")]
    [SerializeField] private bool useVirtualJoystick = true;
    [SerializeField] private VirtualJoystick virtualJoystick;

    private Transform curTransform;
    private Vector2 speed;
    private Vector2 inputDirection;

    private void Awake()
    {
        curTransform = transform;
        
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
        UpdateMovement();
        ApplyMovement();
    }
    
    private void HandleInput()
    {
        if (useVirtualJoystick && virtualJoystick != null) {
            inputDirection = virtualJoystick.GetInputDirection();
        }
        else {
            // using keyboard for testing in Editor
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
    
    private void UpdateMovement()
    {
        if (Mathf.Abs(inputDirection.x) > 0.1f) {
            speed.x += inputDirection.x * acceleration * Time.deltaTime;
        }
        else
        {
            // deceleration
            var sign = Mathf.Sign(speed.x);
            speed.x = (Mathf.Abs(speed.x) - decceleration * Time.deltaTime) * sign;
            
            if (Mathf.Abs(speed.x) < 0.1f)
                speed.x = 0f;
        }
        
        if (Mathf.Abs(inputDirection.y) > 0.1f) {
            speed.y += inputDirection.y * acceleration * Time.deltaTime;
        }
        else
        {
            // deceleration
            var sign = Mathf.Sign(speed.y);
            speed.y = (Mathf.Abs(speed.y) - decceleration * Time.deltaTime) * sign;
            
            if (Mathf.Abs(speed.y) < 0.1f)
                speed.y = 0f;
        }
        
        speed.x = Mathf.Clamp(speed.x, -maxSpeed, maxSpeed);
        speed.y = Mathf.Clamp(speed.y, -maxSpeed, maxSpeed);
    }
    
    private void ApplyMovement()
    {
        curTransform.position += new Vector3(speed.x, speed.y, 0) * Time.deltaTime;
    }
    
    private void OnJoystickInput(Vector2 direction)
    {
        // Этот метод вызывается когда джойстик двигается
        // inputDirection уже обновляется в HandleInput()
    }
    
    // Публичные методы для получения информации о движении
    public Vector2 GetCurrentSpeed() => speed;
    public Vector2 GetInputDirection() => inputDirection;
    public bool IsMoving() => speed.magnitude > 0.1f;
}