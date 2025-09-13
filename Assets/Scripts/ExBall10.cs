using UnityEngine;

public class ExBall10 : MonoBehaviour
{
    [Header("Experience Settings")]
    public int experienceValue = 10;
    
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float detectionRange = 3f; // Distance to start moving towards player
    public float lifetime = 30f; // ExBall disappears after 30 seconds if not collected
    
    private Transform playerTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find the player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        
        // Destroy after lifetime to prevent memory leaks
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        // Only move towards player if within detection range
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= detectionRange)
            {
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Give experience to player
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.GainExperience(experienceValue);
            }
            
            // Destroy this ExBall
            Destroy(gameObject);
        }
    }
}
