using UnityEngine;
using System.Collections;

public class Enemy3 : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 5f;
    public float followDistance = 0.5f; // Minimum distance to maintain from player
    public bool smoothMovement = true;
    
    [Header("Combat Settings")]
    public int damage = 75;
    
    [Header("Health Settings")]
    public int maxHealth = 200;
    public int currentHealth = 200;
    
    [Header("Flash Effect Settings")]
    [SerializeField] private float flashDuration = 0.1f; // Duration of red flash
    [SerializeField] private Color flashColor = Color.red; // Color to flash
    
    [Header("Drop Settings")]
    public GameObject exBallPrefab; // Assign ExBall25 prefab in inspector
    [Header("Powerup Drops")]
    public GameObject healthPackPrefab; // Assign HealthPack prefab in inspector
    public GameObject bombPrefab; // Assign Bomb prefab in inspector
    public GameObject speedBoostPrefab; // Assign SpeedBoost prefab in inspector
    public GameObject magnetPrefab; // Assign Magnet prefab in inspector
    
    [Header("Drop Probabilities")]
    [SerializeField] private float exBallDropChance = 0.8f; // 80% chance for ExBall
    [SerializeField] private float powerupDropChance = 0.2f; // 20% chance for powerup
    
    private Transform playerTransform;
    private Player playerScript;
    private SpriteRenderer spriteRenderer;
    private Color originalColor; // Store original sprite color
    private Animator animator; // Animation controller
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Find the player in the scene
        FindPlayer();
        
        // Get sprite renderer and store original color
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Get animator component
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // If no player found, try to find one
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }
        
        // Follow the player
        FollowPlayer();
    }
    
    void FindPlayer()
    {
        // Try to find player by component first
        playerScript = FindFirstObjectByType<Player>();
        if (playerScript != null)
        {
            playerTransform = playerScript.transform;
            return;
        }
        
        // Try to find player by tag as fallback
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }
    
    void FollowPlayer()
    {
        if (playerTransform == null) return;
        
        // Calculate direction to player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Flip sprite based on movement direction
        if (spriteRenderer != null)
        {
            if (direction.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (direction.x > 0)
            {
                spriteRenderer.flipX = false;
            }
        }
        
        // Only move if we're farther than the follow distance
        if (distanceToPlayer > followDistance)
        {
            // Calculate target position
            Vector3 targetPosition = transform.position + direction * moveSpeed * Time.deltaTime;
            
            // Make sure we don't overshoot and get too close
            float newDistance = Vector3.Distance(targetPosition, playerTransform.position);
            if (newDistance < followDistance)
            {
                // Stop at the follow distance
                targetPosition = playerTransform.position - direction * followDistance;
            }
            
            // Apply movement
            if (smoothMovement)
            {
                // Smooth movement using Lerp
                transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
            else
            {
                // Direct movement
                transform.position = targetPosition;
            }
            
            // Set running animation
            if (animator != null)
            {
                animator.SetBool("isRunning", true);
            }
        }
        else
        {
            // Stop running animation when not moving
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
            }
        }
    }
    
    // Optional: Visualize follow distance in scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, followDistance);
        
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Flash red when taking damage
        FlashRed();
        
        // Print to console
       // Debug.Log("Enemy took " + damage + " damage! Current health: " + currentHealth);
        
        // Check if enemy is dead
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            //Debug.Log("Enemy has died!");
            
            // Drop system with probability
            DropItem();
            
            // Destroy the enemy GameObject
            Destroy(gameObject);
        }
    }
    
    private void FlashRed()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashCoroutine());
        }
    }
    
    private System.Collections.IEnumerator FlashCoroutine()
    {
        // Change to flash color
        spriteRenderer.color = flashColor;
        
        // Wait for flash duration
        yield return new WaitForSeconds(flashDuration);
        
        // Return to original color
        spriteRenderer.color = originalColor;
    }
    
    private void DropItem()
    {
        // Generate random number between 0 and 1
        float randomValue = Random.Range(0f, 1f);
        
        if (randomValue <= exBallDropChance)
        {
            // 80% chance - Drop ExBall
            if (exBallPrefab != null)
            {
                Instantiate(exBallPrefab, transform.position, Quaternion.identity);
                Debug.Log("Enemy3 dropped ExBall");
            }
        }
        else
        {
            // 20% chance - Drop random powerup
            DropRandomPowerup();
        }
    }
    
    private void DropRandomPowerup()
    {
        // Create list of available powerups
        System.Collections.Generic.List<GameObject> powerups = new System.Collections.Generic.List<GameObject>();
        
        if (healthPackPrefab != null) powerups.Add(healthPackPrefab);
        if (bombPrefab != null) powerups.Add(bombPrefab);
        if (speedBoostPrefab != null) powerups.Add(speedBoostPrefab);
        if (magnetPrefab != null) powerups.Add(magnetPrefab);
        
        // Drop random powerup if any are available
        if (powerups.Count > 0)
        {
            int randomIndex = Random.Range(0, powerups.Count);
            GameObject selectedPowerup = powerups[randomIndex];
            Instantiate(selectedPowerup, transform.position, Quaternion.identity);
            Debug.Log($"Enemy3 dropped powerup: {selectedPowerup.name}");
        }
        else
        {
            // Fallback to ExBall if no powerups are assigned
            if (exBallPrefab != null)
            {
                Instantiate(exBallPrefab, transform.position, Quaternion.identity);
                Debug.Log("Enemy3 dropped ExBall (fallback)");
            }
        }
    }
}
