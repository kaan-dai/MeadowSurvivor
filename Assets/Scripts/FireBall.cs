using UnityEngine;

public class FireBall : MonoBehaviour
{
    [Header("FireBall Settings")]
    public float speed = 8f;              // Slightly slower than bullet
    public float lifetime = 2f;           // Initial lifetime of 2 seconds
    public float scale = 1f;              // Base scale of the fireball
    
    [Header("Damage Settings")]
    public int damage = 75;               // More damage than bullet to compensate for slower speed
    
    private Vector3 direction;
    
    void Start()
    {
        // Find the closest enemy and set initial direction
        FindAndSetDirection();
        
        // Set initial scale
        transform.localScale = Vector3.one * scale;
        
        // Destroy fireball after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        MoveFireBall();
    }
    
    void FindAndSetDirection()
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
        
        // Set direction based on closest enemy
        if (closestEnemy != null)
        {
            direction = (closestEnemy.position - transform.position).normalized;
        }
        else
        {
            // Fallback direction
            direction = Vector3.up;
        }

        // Calculate initial rotation to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    void MoveFireBall()
    {
        // Move the fireball in the set direction
        transform.position += direction * speed * Time.deltaTime;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
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
            
            // Don't destroy the fireball, let it continue through enemies
        }
    }
}
