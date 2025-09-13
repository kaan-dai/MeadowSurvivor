using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;    // Array of different enemy prefabs (Enemy1, Enemy2, Enemy3, Enemy4)
    
    [Header("Spawn Settings")]
    public float spawnInterval = 1f;  // Time between spawns (reduced from 2f to 1f)
    public float spawnRadius = 10f;   // How far from player to spawn
    public int minEnemiesPerSpawn = 1;  // Minimum number of enemies to spawn at once
    public int maxEnemiesPerSpawn = 10; // Maximum number of enemies to spawn at once

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true; // Show current minute and spawn info in console

    private Transform player;
    private float nextSpawnTime;
    private int lastMinute = 0; // Track when minute changes for debug info

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Try to find the player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("No player found with 'Player' tag!");
        }

        // Check if enemy prefabs are assigned
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned to EnemySpawner!");
            return;
        }

        // Validate that we have all 4 enemy types
        if (enemyPrefabs.Length < 4)
        {
            Debug.LogWarning("EnemySpawner needs at least 4 enemy prefabs (Enemy1, Enemy2, Enemy3, Enemy4)!");
        }

        nextSpawnTime = Time.time + spawnInterval;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for minute changes and display debug info
        if (showDebugInfo)
        {
            GameTimer gameTimer = GameTimer.GetOrCreateInstance();
            if (gameTimer != null)
            {
                int currentMinute = gameTimer.GetCurrentMinute();
                if (currentMinute != lastMinute)
                {
                    lastMinute = currentMinute;
                    string timeRange = $"{currentMinute}:00-{currentMinute}:59";
                    Debug.Log($"Entering minute {currentMinute} ({timeRange}) - Spawn probabilities updated!");
                }
            }
        }

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        if (player == null || enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Get current minute from GameTimer
        int currentMinute = 0; // Default to minute 0 if no timer
        GameTimer gameTimer = GameTimer.GetOrCreateInstance();
        if (gameTimer != null && gameTimer.IsRunning())
        {
            currentMinute = gameTimer.GetCurrentMinute();
        }

        // Determine how many enemies to spawn this time
        int enemyCount = Random.Range(minEnemiesPerSpawn, maxEnemiesPerSpawn + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            // Select enemy type based on current minute
            GameObject selectedEnemyPrefab = SelectEnemyBasedOnTime(currentMinute);
            
            if (selectedEnemyPrefab != null)
            {
                // Get random angle
                float randomAngle = Random.Range(0f, 360f);
                
                // Convert angle to direction
                Vector2 spawnDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
                
                // Calculate spawn position
                Vector2 spawnPosition = (Vector2)player.position + (spawnDirection * spawnRadius);

                // Spawn the selected enemy
                Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    GameObject SelectEnemyBasedOnTime(int minute)
    {
        float randomValue = Random.Range(0f, 100f); // Random percentage (0-100)
        
        switch (minute)
        {
            case 0:
                // Minute 0 (0:00-0:59): 100% Enemy1
                return GetEnemyPrefab(0); // Enemy1
                
            case 1:
                // Minute 1 (1:00-1:59): 50% Enemy1, 50% Enemy2
                if (randomValue < 50f) return GetEnemyPrefab(0); // Enemy1
                else return GetEnemyPrefab(1); // Enemy2
                
            case 2:
                // Minute 2 (2:00-2:59): 20% Enemy1, 80% Enemy2
                if (randomValue < 20f) return GetEnemyPrefab(0); // Enemy1
                else return GetEnemyPrefab(1); // Enemy2
                
            case 3:
                // Minute 3 (3:00-3:59): 10% Enemy1, 40% Enemy2, 50% Enemy3
                if (randomValue < 10f) return GetEnemyPrefab(0); // Enemy1
                else if (randomValue < 50f) return GetEnemyPrefab(1); // Enemy2
                else return GetEnemyPrefab(2); // Enemy3
                
            case 4:
                // Minute 4 (4:00-4:59): 5% Enemy1, 25% Enemy2, 70% Enemy3
                if (randomValue < 5f) return GetEnemyPrefab(0); // Enemy1
                else if (randomValue < 30f) return GetEnemyPrefab(1); // Enemy2
                else return GetEnemyPrefab(2); // Enemy3
                
            case 5:
                // Minute 5 (5:00-5:59): 0% Enemy1, 15% Enemy2, 85% Enemy3
                if (randomValue < 15f) return GetEnemyPrefab(1); // Enemy2
                else return GetEnemyPrefab(2); // Enemy3
                
            case 6:
                // Minute 6 (6:00-6:59): 0% Enemy1, 5% Enemy2, 50% Enemy3, 45% Enemy4
                if (randomValue < 5f) return GetEnemyPrefab(1); // Enemy2
                else if (randomValue < 55f) return GetEnemyPrefab(2); // Enemy3
                else return GetEnemyPrefab(3); // Enemy4
                
            case 7:
                // Minute 7 (7:00-7:59): 0% Enemy1, 0% Enemy2, 35% Enemy3, 65% Enemy4
                if (randomValue < 35f) return GetEnemyPrefab(2); // Enemy3
                else return GetEnemyPrefab(3); // Enemy4
                
            case 8:
                // Minute 8 (8:00-8:59): 0% Enemy1, 0% Enemy2, 15% Enemy3, 85% Enemy4
                if (randomValue < 15f) return GetEnemyPrefab(2); // Enemy3
                else return GetEnemyPrefab(3); // Enemy4
                
            default:
                // Minute 9+ (9:00+): 0% Enemy1, 0% Enemy2, 0% Enemy3, 100% Enemy4
                return GetEnemyPrefab(3); // Enemy4
        }
    }

    GameObject GetEnemyPrefab(int index)
    {
        if (index >= 0 && index < enemyPrefabs.Length)
        {
            return enemyPrefabs[index];
        }
        
        // Fallback to first enemy if index is out of bounds
        Debug.LogWarning($"Enemy index {index} is out of bounds. Falling back to Enemy1.");
        return enemyPrefabs.Length > 0 ? enemyPrefabs[0] : null;
    }
}