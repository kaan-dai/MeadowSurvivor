using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 5f; // Reduced default speed from 10f to 5f
    public float lifetime = 5f;
    
    [Header("Damage Settings")]
    public int damage = 50;
    public int maxPierceCount = 1;
    private int currentPierceCount = 0;
    
    private Vector3 direction;
    private bool directionSet = false;
    
    void Start()
    {
        // Only set initial direction if it hasn't been set manually
        if (!directionSet)
        {
            SetInitialDirection();
        }
        
        // Destroy bullet after lifetime to prevent memory leaks
        Destroy(gameObject, lifetime);
    }

    public void SetInitialDirectionManually(Vector3 newDirection)
    {
        direction = newDirection.normalized;
        directionSet = true;
        
        // Set rotation to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        MoveBullet();
    }
    
    void SetInitialDirection()
    {
        // Find all enemies with the "Enemy" tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (enemies.Length == 0)
        {
            // No enemies found, move straight up as default
            direction = Vector3.up;
            return;
        }
        
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;
        
        // Find the closest enemy
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }
        
        // Set initial direction towards closest enemy or default up
        if (closestEnemy != null)
        {
            direction = (closestEnemy.position - transform.position).normalized;
        }
        else
        {
            direction = Vector3.up;
        }
        
        // Set initial rotation to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    void MoveBullet()
    {
        // Move the bullet in its fixed direction
        transform.position += direction * speed * Time.deltaTime;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Try to get any type of enemy component and apply damage
            Enemy1 enemy1 = other.GetComponent<Enemy1>();
            Enemy2 enemy2 = other.GetComponent<Enemy2>();
            Enemy3 enemy3 = other.GetComponent<Enemy3>();
            Enemy4 enemy4 = other.GetComponent<Enemy4>();
            
            // Apply damage to whichever enemy type we hit
            if (enemy1 != null) enemy1.TakeDamage(damage);
            if (enemy2 != null) enemy2.TakeDamage(damage);
            if (enemy3 != null) enemy3.TakeDamage(damage);
            if (enemy4 != null) enemy4.TakeDamage(damage);
            
            // Increment pierce count
            currentPierceCount++;
            
            // Destroy the bullet if we've reached max pierce count
            if (currentPierceCount >= maxPierceCount)
            {
                Destroy(gameObject);
            }
        }
    }
}
