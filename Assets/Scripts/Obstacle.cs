using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(Rigidbody2D))]
public class Obstacle : MonoBehaviour, IHasMaterial
{
    [Header("Obstacle Settings")]
    [SerializeField] private bool isStatic = true;
    [SerializeField] private bool canPushPlayer = false;
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private AssetReferenceT<GameObjectPhysicalMaterial> physicalMaterialReference;

    private GameObjectPhysicalMaterial physicalMaterial;
    
    public GameObjectPhysicalMaterial Material => physicalMaterial;
    
    private void Awake()
    {
        SetupRigidbody();
        physicalMaterialReference.LoadAssetAsync<GameObjectPhysicalMaterial>().Completed += handle => {
            if (handle.Status == AsyncOperationStatus.Succeeded) {
                physicalMaterial = handle.Result;
            }
        };
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