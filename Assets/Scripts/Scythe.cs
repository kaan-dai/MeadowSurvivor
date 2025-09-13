using UnityEngine;

public class Scythe : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 200f;        // Degrees per second
    public float distanceFromPlayer = 2f;     // How far from player the scythe rotates
    public bool clockwise = true;             // Rotation direction
    
    [Header("Combat Settings")]
    public int damage = 50;
    
    private Transform playerTransform;
    private float currentAngle;
    
    void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Set random starting angle
        currentAngle = Random.Range(0f, 360f);
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // Update angle based on rotation speed and direction
            float angleChange = rotationSpeed * Time.deltaTime;
            currentAngle += clockwise ? -angleChange : angleChange;
            
            // Keep angle between 0 and 360 degrees
            if (currentAngle >= 360f) currentAngle -= 360f;
            if (currentAngle < 0f) currentAngle += 360f;
            
            // Calculate position around player
            float radian = currentAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(radian) * distanceFromPlayer;
            float y = Mathf.Sin(radian) * distanceFromPlayer;
            
            // Update position relative to player
            Vector3 newPosition = playerTransform.position + new Vector3(x, y, 0);
            transform.position = newPosition;
            
            // Make scythe face the rotation direction
            float rotationZ = currentAngle + 270f;  // Changed to a fixed 180-degree offset
            transform.rotation = Quaternion.Euler(0, 0, rotationZ);
        }
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
            
            // Removed the Destroy(gameObject) since we want the scythe to persist
        }
    }
}
