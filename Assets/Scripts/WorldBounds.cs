using UnityEngine;

public class WorldBounds : MonoBehaviour
{
    public static WorldBounds Instance { get; private set; }

    [Header("World Boundaries")]
    [SerializeField] private float leftBound = -10f;
    [SerializeField] private float rightBound = 10f;
    [SerializeField] private float topBound = 5f;
    [SerializeField] private float bottomBound = -5f;
    
    [Header("Physical Boundaries")]
    [SerializeField] private bool createPhysicalBounds = true;
    [SerializeField] private float boundaryThickness = 1f;
    [SerializeField] private string boundaryTag = "Boundary";
    
    [Header("Gizmos")]
    [SerializeField] private bool showBounds = true;
    [SerializeField] private Color boundsColor = Color.red;
    
    // Properties
    public float LeftBound => leftBound;
    public float RightBound => rightBound;
    public float TopBound => topBound;
    public float BottomBound => bottomBound;
    
    public Vector2 MinBounds => new Vector2(leftBound, bottomBound);
    public Vector2 MaxBounds => new Vector2(rightBound, topBound);
    
    private Transform boundariesParent;
    
    private void Awake()
    {
        // Singleton
        if (Instance == null) {
            Instance = this;
            
            if (createPhysicalBounds) {
                CreatePhysicalBoundaries();
            }
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
    
    private void CreatePhysicalBoundaries()
    {
        // Create parent object for boundaries
        GameObject boundariesContainer = new GameObject("World Boundaries");
        boundariesContainer.transform.SetParent(transform);
        boundariesParent = boundariesContainer.transform;
        
        // Create four boundary walls
        CreateBoundaryWall("Left Wall", 
            new Vector3(leftBound - boundaryThickness * 0.5f, (topBound + bottomBound) * 0.5f, 0),
            new Vector3(boundaryThickness, topBound - bottomBound + boundaryThickness * 2, 1));
            
        CreateBoundaryWall("Right Wall",
            new Vector3(rightBound + boundaryThickness * 0.5f, (topBound + bottomBound) * 0.5f, 0),
            new Vector3(boundaryThickness, topBound - bottomBound + boundaryThickness * 2, 1));
            
        CreateBoundaryWall("Top Wall",
            new Vector3((leftBound + rightBound) * 0.5f, topBound + boundaryThickness * 0.5f, 0),
            new Vector3(rightBound - leftBound, boundaryThickness, 1));
            
        CreateBoundaryWall("Bottom Wall",
            new Vector3((leftBound + rightBound) * 0.5f, bottomBound - boundaryThickness * 0.5f, 0),
            new Vector3(rightBound - leftBound, boundaryThickness, 1));
    }
    
    private void CreateBoundaryWall(string wallName, Vector3 position, Vector3 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(boundariesParent);
        wall.transform.position = position;
        wall.transform.localScale = size;
        
        // Add tag
        if (!string.IsNullOrEmpty(boundaryTag)) {
            wall.tag = boundaryTag;
        }
        
        // Add BoxCollider2D
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        
        // Add Rigidbody2D as static
        Rigidbody2D rb = wall.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        rb.gravityScale = 0f;
    }
    
    // Method to recreate boundaries when bounds change
    [ContextMenu("Recreate Physical Boundaries")]
    public void RecreatePhysicalBoundaries()
    {
        DestroyPhysicalBoundaries();
        if (createPhysicalBounds) {
            CreatePhysicalBoundaries();
        }
    }
    
    private void DestroyPhysicalBoundaries()
    {
        if (boundariesParent != null) {
            DestroyImmediate(boundariesParent.gameObject);
            boundariesParent = null;
        }
    }
    
    // Call this when bounds change at runtime
    private void OnValidate()
    {
        // Only recreate in play mode and if we have physical bounds enabled
        if (Application.isPlaying && createPhysicalBounds && boundariesParent != null) {
            RecreatePhysicalBoundaries();
        }
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