using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("HealthPack Settings")]
    [SerializeField] private float lifetime = 30f; // How long the health pack stays before disappearing
    
    [Header("Visual Effects")]
    [SerializeField] private float collectFlashDuration = 0.2f;
    [SerializeField] private Color collectFlashColor = Color.green;
    
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
                // Restore player health to maximum using the public FullHeal method
                player.FullHeal();
                
                Debug.Log("HealthPack collected!");
                
                // Visual feedback
                StartCoroutine(CollectEffect(player));
                
                // Destroy the health pack
                Destroy(gameObject);
            }
        }
    }
    
    private System.Collections.IEnumerator CollectEffect(Player player)
    {
        // Flash the player green briefly to show healing
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