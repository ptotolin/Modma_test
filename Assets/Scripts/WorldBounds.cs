using UnityEngine;

public class WorldBounds : MonoBehaviour
{
    public static WorldBounds Instance { get; private set; }

    [Header("World Boundaries")]
    [SerializeField] private float leftBound = -10f;
    [SerializeField] private float rightBound = 10f;
    [SerializeField] private float topBound = 5f;
    [SerializeField] private float bottomBound = -5f;
    
    [Header("Gizmos")]
    [SerializeField] private bool showBounds = true;
    [SerializeField] private Color boundsColor = Color.red;
    
    // Singleton для удобного доступа
    
    // Properties для получения границ
    public float LeftBound => leftBound;
    public float RightBound => rightBound;
    public float TopBound => topBound;
    public float BottomBound => bottomBound;
    
    public Vector2 MinBounds => new Vector2(leftBound, bottomBound);
    public Vector2 MaxBounds => new Vector2(rightBound, topBound);
    
    private void Awake()
    {
        // Singleton
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }
    
    public Vector3 ClampPosition(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, leftBound, rightBound),
            Mathf.Clamp(position.y, bottomBound, topBound),
            position.z
        );
    }
    
    public Vector2 ClampPosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, leftBound, rightBound),
            Mathf.Clamp(position.y, bottomBound, topBound)
        );
    }
    
    public bool IsWithinBounds(Vector3 position)
    {
        return position.x >= leftBound && position.x <= rightBound &&
               position.y >= bottomBound && position.y <= topBound;
    }
    
    public float GetDistanceToNearestBound(Vector3 position)
    {
        var distanceToLeft = position.x - leftBound;
        var distanceToRight = rightBound - position.x;
        var distanceToTop = topBound - position.y;
        var distanceToBottom = position.y - bottomBound;
        
        return Mathf.Min(distanceToLeft, distanceToRight, distanceToTop, distanceToBottom);
    }
    
    private void OnDrawGizmos()
    {
        if (!showBounds) return;
        
        Gizmos.color = boundsColor;
        
        var topLeft = new Vector3(leftBound, topBound, 0);
        var topRight = new Vector3(rightBound, topBound, 0);
        var bottomLeft = new Vector3(leftBound, bottomBound, 0);
        var bottomRight = new Vector3(rightBound, bottomBound, 0);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        var cornerSize = 0.5f;
        Gizmos.DrawWireCube(topLeft, Vector3.one * cornerSize);
        Gizmos.DrawWireCube(topRight, Vector3.one * cornerSize);
        Gizmos.DrawWireCube(bottomLeft, Vector3.one * cornerSize);
        Gizmos.DrawWireCube(bottomRight, Vector3.one * cornerSize);
    }
}