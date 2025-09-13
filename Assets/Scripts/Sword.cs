using UnityEngine;
using System.Collections.Generic;

public class Sword : MonoBehaviour
{
    [Header("Sword Settings")]
    public float slashDuration = 0.5f;
    public float slashCooldown = 1f;
    public float slashAngle = 120f; // Total angle of the slash arc
    public float slashRadius = 1.5f; // Distance from player (increased from default)
    public float baseRotationOffset = 90f; // Offset to make handle point to player (adjust in inspector if needed)
    
    [Header("Combat Settings")]
    public int damage = 75; // Higher damage than scythe since it's not constantly hitting
    
    private float slashTimer;
    private bool isSlashing;
    private Vector3 startPosition;
    private float currentAngle;
    private Transform playerTransform;
    private bool facingRight = true;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>(); // Track enemies hit this slash
    
    void Start()
    {
        // Find and store the player transform
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
        StartSlash();
    }

    void Update()
    {
        if (isSlashing)
        {
            // Update slash timer
            slashTimer += Time.deltaTime;
            
            // Calculate slash progress (0 to 1)
            float progress = Mathf.Clamp01(slashTimer / slashDuration);
            
            // Calculate current angle based on progress
            // Start at +60 degrees (top) and move to -60 degrees (bottom)
            float startAngle = slashAngle/2f;
            float endAngle = -slashAngle/2f;
            currentAngle = Mathf.Lerp(startAngle, endAngle, progress);
            
            // Calculate position around the player
            float angleInRadians = (facingRight ? currentAngle : (180f - currentAngle)) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angleInRadians) * slashRadius;
            float y = Mathf.Sin(angleInRadians) * slashRadius;
            
            // Update sword position relative to player
            transform.position = playerTransform.position + new Vector3(x, y, 0);
            
            // Calculate rotation to make handle point at player
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            
            // Apply rotation with offset to make handle point inward
            transform.rotation = Quaternion.Euler(0, 0, angleToPlayer + baseRotationOffset);
            
            // Check for enemies in range every frame during slash
            CheckForEnemiesInRange();
            
            // Check if slash duration is complete
            if (slashTimer >= slashDuration)
            {
                EndSlash();
            }
        }
        else
        {
            // Update cooldown timer
            slashTimer += Time.deltaTime;
            
            // Check if cooldown is complete
            if (slashTimer >= slashCooldown)
            {
                StartSlash();
            }
        }

        // Update facing direction based on player's sprite renderer
        if (playerTransform != null)
        {
            SpriteRenderer playerSprite = playerTransform.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                facingRight = !playerSprite.flipX;
            }
        }
    }
    
    void StartSlash()
    {
        // Reset timer and state
        isSlashing = true;
        slashTimer = 0f;
        hitEnemies.Clear(); // Clear hit enemies for new slash
        
        // Show the sword
        GetComponent<SpriteRenderer>().enabled = true;
    }
    
    void EndSlash()
    {
        // End slashing
        isSlashing = false;
        slashTimer = 0f;
        
        // Hide the sword
        GetComponent<SpriteRenderer>().enabled = false;
    }

    void CheckForEnemiesInRange()
    {
        if (!isSlashing || playerTransform == null) return;
        
        // Get the sword's collider
        Collider2D swordCollider = GetComponent<Collider2D>();
        if (swordCollider == null) return;
        
        // Find ALL enemy colliders overlapping with the sword (no limit)
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(-1); // All layers
        filter.useTriggers = true;
        
        List<Collider2D> overlappingEnemies = new List<Collider2D>();
        int count = swordCollider.Overlap(filter, overlappingEnemies);
        
        // Process all overlapping enemies
        for (int i = 0; i < overlappingEnemies.Count; i++)
        {
            Collider2D enemyCollider = overlappingEnemies[i];
            if (enemyCollider != null && enemyCollider.CompareTag("Enemy") && !hitEnemies.Contains(enemyCollider))
            {
                DamageEnemy(enemyCollider);
                hitEnemies.Add(enemyCollider); // Mark this enemy as hit
            }
        }
    }

    void DamageEnemy(Collider2D enemyCollider)
    {
        // Deal damage to the enemy
        Enemy1 enemy1 = enemyCollider.GetComponent<Enemy1>();
        Enemy2 enemy2 = enemyCollider.GetComponent<Enemy2>();
        Enemy3 enemy3 = enemyCollider.GetComponent<Enemy3>();
        Enemy4 enemy4 = enemyCollider.GetComponent<Enemy4>();
        
        if (enemy1 != null) enemy1.TakeDamage(damage);
        else if (enemy2 != null) enemy2.TakeDamage(damage);
        else if (enemy3 != null) enemy3.TakeDamage(damage);
        else if (enemy4 != null) enemy4.TakeDamage(damage);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isSlashing && !hitEnemies.Contains(other))
        {
            DamageEnemy(other);
            hitEnemies.Add(other);
        }
    }
}
