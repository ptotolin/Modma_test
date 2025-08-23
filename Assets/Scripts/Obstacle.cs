using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private bool isStatic = true;
    [SerializeField] private bool canPushPlayer = false;
    [SerializeField] private float pushForce = 5f;
    
    private void Awake()
    {
        SetupRigidbody();
    }
    
    private void SetupRigidbody()
    {
        // Configure existing Rigidbody2D if present
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) {
            if (isStatic) {
                rb.bodyType = RigidbodyType2D.Static;
            } else {
                rb.gravityScale = 0f; // No gravity for top-down games
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (canPushPlayer && collision.gameObject.CompareTag("Player")) {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) {
                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) {
                    rb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}