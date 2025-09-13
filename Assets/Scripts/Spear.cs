using UnityEngine;

public class Spear : MonoBehaviour
{
    [Header("Spear Settings")]
    public float stabDuration = 0.2f;         // Duration of each stab
    public float stabCooldown = 0.1f;         // Cooldown between stabs
    public float sequenceCooldown = 1f;       // Cooldown after 3 stabs
    public float stabDistance = 2f;           // How far the spear stabs
    public float baseRotationOffset = 0f;     // Adjust if needed based on sprite orientation
    public float idleOffset = 0.5f;          // Distance from player when idle
    
    [Header("Combat Settings")]
    public int damage = 60;                   // Damage per stab
    public int level = 1;                     // Current level of the spear
    public float attackDirection = 0f;        // Direction in degrees (0 = right, 180 = left, 90 = up, 270 = down)
    public bool followPlayerDirection = false; // Whether this spear should follow player direction
    
    private Transform playerTransform;
    private float stabTimer;
    private float sequenceTimer;
    private bool isStabbing;
    private int stabCount;
    private bool facingRight = true;
    private Vector3 startPosition;
    private Vector3 stabDirection;
    private Vector3 stabStartPosition;
    private SpriteRenderer playerSprite;
    
    void Start()
    {
        // Find and store the player transform
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (playerTransform != null)
        {
            playerSprite = playerTransform.GetComponent<SpriteRenderer>();
        }
        UpdateStartPosition();
        StartStabSequence();
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Update facing direction based on player's sprite renderer
        if (playerSprite != null)
        {
            facingRight = !playerSprite.flipX;
        }

        // If this is a direction-following spear, update its attack direction
        if (followPlayerDirection)
        {
            attackDirection = facingRight ? 0f : 180f;
        }

        // Always update the ideal position relative to player
        UpdateStartPosition();

        if (isStabbing)
        {
            // Update stab timer
            stabTimer += Time.deltaTime;
            
            // Calculate stab progress (0 to 1)
            float progress = Mathf.Clamp01(stabTimer / stabDuration);
            
            // Use SmoothStep for more dynamic motion
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            
            // If in first half of stab, move forward, otherwise return
            float adjustedProgress = progress <= 0.5f ? smoothProgress * 2 : 2 - (smoothProgress * 2);
            
            // Calculate position based on the current player position and stab direction
            transform.position = startPosition + (stabDirection * stabDistance * adjustedProgress);
            
            // Update rotation based on attack direction
            transform.rotation = Quaternion.Euler(0, 0, attackDirection + baseRotationOffset);
            
            // Check if stab is complete
            if (stabTimer >= stabDuration)
            {
                EndStab();
            }
        }
        else
        {
            // Update sequence timer
            sequenceTimer += Time.deltaTime;
            
            // Check if we should start next stab
            if (stabCount < 3 && sequenceTimer >= stabCooldown)
            {
                StartStab();
            }
            // Check if sequence cooldown is complete
            else if (stabCount >= 3 && sequenceTimer >= sequenceCooldown)
            {
                StartStabSequence();
            }
            else
            {
                // When not stabbing, just use the updated position
                transform.position = startPosition;
            }
        }
    }
    
    void StartStabSequence()
    {
        stabCount = 0;
        sequenceTimer = 0f;
        StartStab();
    }
    
    void StartStab()
    {
        isStabbing = true;
        stabTimer = 0f;
        stabCount++;
        GetComponent<SpriteRenderer>().enabled = true;
        
        // Calculate stab direction based on attack direction
        float angleInRadians = attackDirection * Mathf.Deg2Rad;
        stabDirection = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0);
        stabStartPosition = startPosition;
    }
    
    void EndStab()
    {
        isStabbing = false;
        sequenceTimer = 0f;
        GetComponent<SpriteRenderer>().enabled = false;
    }
    
    void UpdateStartPosition()
    {
        if (playerTransform != null)
        {
            // Calculate offset based on attack direction
            float angleInRadians = attackDirection * Mathf.Deg2Rad;
            Vector3 directionVector = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0);
            startPosition = playerTransform.position + (directionVector * idleOffset);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isStabbing)
        {
            // Deal damage to the enemy
            Enemy1 enemy1 = other.GetComponent<Enemy1>();
            Enemy2 enemy2 = other.GetComponent<Enemy2>();
            Enemy3 enemy3 = other.GetComponent<Enemy3>();
            Enemy4 enemy4 = other.GetComponent<Enemy4>();
            
            if (enemy1 != null) enemy1.TakeDamage(damage);
            if (enemy2 != null) enemy2.TakeDamage(damage);
            if (enemy3 != null) enemy3.TakeDamage(damage);
            if (enemy4 != null) enemy4.TakeDamage(damage);
        }
    }
}
