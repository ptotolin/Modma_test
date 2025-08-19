using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 2f;
    
    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1, -10);
    
    [Header("Boundaries")]
    [SerializeField] private bool respectWorldBounds = true;
    [SerializeField] private float cameraHalfWidth = 5f;
    [SerializeField] private float cameraHalfHeight = 3f;
    
    [Header("Smoothing")]
    [SerializeField] private bool useSmoothDamping = true;
    [SerializeField] private float smoothTime = 0.3f;
    
    [Header("Dead Zone")]
    [SerializeField] private bool useDeadZone = false;
    [SerializeField] private float deadZoneWidth = 2f;
    [SerializeField] private float deadZoneHeight = 1f;
    
    private Vector3 velocity = Vector3.zero;
    private Vector3 lookAheadPosition;
    private Camera cam;
    private PlayerController playerController;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        if (target == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
                target = player.transform;
        }
        
        if (target != null)
            playerController = target.GetComponent<PlayerController>();
        
        if (cam != null) {
            cameraHalfHeight = cam.orthographicSize;
            cameraHalfWidth = cameraHalfHeight * cam.aspect;
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) 
            return;
        
        var targetPosition = CalculateTargetPosition();
        
        if (respectWorldBounds && WorldBounds.Instance != null) {
            targetPosition = ClampCameraToWorldBounds(targetPosition);
        }
        
        MoveCamera(targetPosition);
    }
    
    private Vector3 CalculateTargetPosition()
    {
        Vector3 basePosition = target.position + offset;
        
        // Добавляем look-ahead (камера смотрит немного вперед по направлению движения)
        if (playerController != null && lookAheadDistance > 0)
        {
            var inputDirection = playerController.GetInputDirection();
            var lookAheadTarget = new Vector3(inputDirection.x * lookAheadDistance, 0, 0);
            
            lookAheadPosition = Vector3.Lerp(lookAheadPosition, lookAheadTarget, lookAheadSpeed * Time.deltaTime);
            
            basePosition += lookAheadPosition;
        }
        
        // Применяем dead zone
        if (useDeadZone) {
            basePosition = ApplyDeadZone(basePosition);
        }
        
        return basePosition;
    }
    
    private Vector3 ApplyDeadZone(Vector3 targetPosition)
    {
        var currentPosition = transform.position;
        var difference = targetPosition - currentPosition;
        
        // Если цель в пределах dead zone, не двигаем камеру
        if (Mathf.Abs(difference.x) < deadZoneWidth * 0.5f) {
            targetPosition.x = currentPosition.x;
        }
        
        if (Mathf.Abs(difference.y) < deadZoneHeight * 0.5f) {
            targetPosition.y = currentPosition.y;
        }
        
        return targetPosition;
    }
    
    private Vector3 ClampCameraToWorldBounds(Vector3 targetPosition)
    {
        var bounds = WorldBounds.Instance;
        
        // Ограничиваем камеру так, чтобы она не выходила за границы мира
        float clampedX = Mathf.Clamp(targetPosition.x, 
            bounds.LeftBound + cameraHalfWidth, 
            bounds.RightBound - cameraHalfWidth);
            
        float clampedY = Mathf.Clamp(targetPosition.y,
            bounds.BottomBound + cameraHalfHeight,
            bounds.TopBound - cameraHalfHeight);
        
        return new Vector3(clampedX, clampedY, targetPosition.z);
    }
    
    private void MoveCamera(Vector3 targetPosition)
    {
        if (useSmoothDamping) {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, 
                ref velocity, smoothTime);
        }
        else {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 
                followSpeed * Time.deltaTime);
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
            playerController = target.GetComponent<PlayerController>();
    }
    
    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    private void OnDrawGizmos()
    {
        if (!useDeadZone) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadZoneWidth, deadZoneHeight, 0));
    }
}