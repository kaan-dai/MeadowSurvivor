using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [Header("SpeedBoost Settings")]
    [SerializeField] private float speedMultiplier = 1.1f; // Multiply player speed by this amount
    [SerializeField] private float lifetime = 30f; // How long the speed boost stays before disappearing
    
    [Header("Visual Effects")]
    [SerializeField] private float collectFlashDuration = 0.2f;
    [SerializeField] private Color collectFlashColor = Color.cyan;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    void Start()
    {
        // Get sprite renderer and store original color
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Destroy after lifetime to prevent clutter
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // Get the current speed
                float oldSpeed = player.moveSpeed;
                
                // Multiply the player's movement speed
                player.moveSpeed *= speedMultiplier;
                
                // Log the speed increase
                Debug.Log($"SpeedBoost collected! Speed increased from {oldSpeed:F1} to {player.moveSpeed:F1} (x{speedMultiplier})");
                
                // Visual feedback
                StartCoroutine(CollectEffect(player));
                
                // Destroy the speed boost
                Destroy(gameObject);
            }
        }
    }
    
    private System.Collections.IEnumerator CollectEffect(Player player)
    {
        // Flash the player cyan briefly to show speed boost
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            Color playerOriginalColor = playerSprite.color;
            playerSprite.color = collectFlashColor;
            
            yield return new WaitForSeconds(collectFlashDuration);
            
            playerSprite.color = playerOriginalColor;
        }
    }
} 