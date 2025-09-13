using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private float explosionRadius = 8f; // Radius to kill enemies
    [SerializeField] private float lifetime = 30f; // How long the bomb stays before disappearing
    
    [Header("Visual Effects")]
    [SerializeField] private float explosionEffectDuration = 0.3f;
    [SerializeField] private Color explosionColor = Color.red;
    
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
                // Trigger explosion effect
                StartCoroutine(ExplodeEffect());
                
                // Find and destroy all enemies within radius
                ExplodeEnemies();
                
                Debug.Log($"Bomb exploded! Killed all enemies within {explosionRadius} units.");
                
                // Destroy the bomb
                Destroy(gameObject);
            }
        }
    }
    
    private void ExplodeEnemies()
    {
        // Find all enemies with the "Enemy" tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int killedCount = 0;
        
        foreach (GameObject enemy in enemies)
        {
            // Calculate distance from bomb to enemy
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            // If enemy is within explosion radius, kill it
            if (distance <= explosionRadius)
            {
                // Try to get any type of enemy component and deal massive damage to kill it instantly
                Enemy1 enemy1 = enemy.GetComponent<Enemy1>();
                Enemy2 enemy2 = enemy.GetComponent<Enemy2>();
                Enemy3 enemy3 = enemy.GetComponent<Enemy3>();
                Enemy4 enemy4 = enemy.GetComponent<Enemy4>();
                
                // Deal 9999 damage to ensure the enemy dies regardless of health
                if (enemy1 != null)
                {
                    enemy1.TakeDamage(9999);
                    killedCount++;
                }
                else if (enemy2 != null)
                {
                    enemy2.TakeDamage(9999);
                    killedCount++;
                }
                else if (enemy3 != null)
                {
                    enemy3.TakeDamage(9999);
                    killedCount++;
                }
                else if (enemy4 != null)
                {
                    enemy4.TakeDamage(9999);
                    killedCount++;
                }
            }
        }
        
        Debug.Log($"Bomb killed {killedCount} enemies!");
    }
    
    private System.Collections.IEnumerator ExplodeEffect()
    {
        // Flash the bomb sprite to red to show explosion
        if (spriteRenderer != null)
        {
            spriteRenderer.color = explosionColor;
            
            // Scale up briefly to show explosion
            Vector3 originalScale = transform.localScale;
            transform.localScale = originalScale * 1.5f;
            
            yield return new WaitForSeconds(explosionEffectDuration);
            
            // Restore original appearance (though object will be destroyed anyway)
            spriteRenderer.color = originalColor;
            transform.localScale = originalScale;
        }
    }
    
    // Optional: Visualize explosion radius in scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
} 