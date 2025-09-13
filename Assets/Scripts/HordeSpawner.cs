using UnityEngine;
using System.Collections.Generic;

public class HordeSpawner : MonoBehaviour
{
    [Header("Horde Settings")]
    [SerializeField] private GameObject[] enemyPrefabs; // Array of enemy prefabs (Enemy1, Enemy2, Enemy3, Enemy4)
    [SerializeField] private int hordeSize = 30; // Number of enemies per horde
    [SerializeField] private float waveInterval = 60f; // Time between waves in seconds (1 minute default)
    
    [Header("Spawn Distance")]
    [SerializeField] private float minDistanceFromPlayer = 30f; // Minimum distance from player
    [SerializeField] private float maxDistanceFromPlayer = 100f; // Maximum distance from player
    
    [Header("Horde Clustering")]
    [SerializeField] private float hordeClusterRadius = 8f; // Radius within which the horde clusters
    [SerializeField] private float enemySpacing = 0.8f; // Distance between enemies in the horde
    
    [Header("Map Bounds")]
    [SerializeField] private BackgroundTileGenerator backgroundGenerator; // Reference to get map bounds
    [SerializeField] private float mapWidth = 200f; // Fallback if no background generator
    [SerializeField] private float mapHeight = 200f; // Fallback if no background generator
    [SerializeField] private Vector2 mapCenter = Vector2.zero; // Map center position
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool drawGizmos = true;
    
    private Transform player;
    private float nextWaveTime;
    private int currentWave = 0;
    private List<Vector2> spawnPositions = new List<Vector2>();
    
    void Start()
    {
        // Find the player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("No player found with 'Player' tag!");
        }
        
        // Validate enemy prefabs
        if (enemyPrefabs == null || enemyPrefabs.Length < 4)
        {
            Debug.LogWarning("HordeSpawner needs at least 4 enemy prefabs (Enemy1, Enemy2, Enemy3, Enemy4)!");
        }
        
        // Get map bounds from BackgroundTileGenerator if available
        if (backgroundGenerator != null)
        {
            Vector2 bounds = backgroundGenerator.GetMapBounds();
            mapWidth = bounds.x;
            mapHeight = bounds.y;
            mapCenter = backgroundGenerator.GetMapCenter();
            
            if (showDebugInfo)
            {
                Debug.Log($"Using BackgroundTileGenerator map bounds: {mapWidth}x{mapHeight}, center: {mapCenter}");
            }
        }
        
        // Set the first wave time
        nextWaveTime = Time.time + waveInterval;
        
        if (showDebugInfo)
        {
            Debug.Log($"HordeSpawner initialized. First wave in {waveInterval} seconds.");
        }
    }
    
    void Update()
    {
        // Check if it's time for the next wave
        if (Time.time >= nextWaveTime)
        {
            SpawnHorde();
            nextWaveTime = Time.time + waveInterval;
        }
    }
    
    void SpawnHorde()
    {
        if (player == null || enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        
        currentWave++;
        
        // Get current minute from GameTimer (same as EnemySpawner)
        int currentMinute = 0;
        if (GameTimer.Instance != null && GameTimer.Instance.IsRunning())
        {
            currentMinute = GameTimer.Instance.GetCurrentMinute();
        }
        
        // Select enemy type for this horde based on current minute probabilities
        GameObject selectedEnemyPrefab = SelectEnemyTypeForHorde(currentMinute);
        
        if (selectedEnemyPrefab == null)
        {
            Debug.LogWarning("No enemy prefab selected for horde!");
            return;
        }
        
        // Generate valid spawn positions
        List<Vector2> validPositions = GenerateValidSpawnPositions();
        
        if (validPositions.Count == 0)
        {
            Debug.LogWarning("No valid spawn positions found for horde!");
            return;
        }
        
        // Spawn the horde
        int enemiesSpawned = 0;
        foreach (Vector2 position in validPositions)
        {
            if (enemiesSpawned >= hordeSize) break;
            
            Instantiate(selectedEnemyPrefab, position, Quaternion.identity);
            enemiesSpawned++;
        }
        
        if (showDebugInfo)
        {
            string enemyName = selectedEnemyPrefab.name;
            Debug.Log($"Wave {currentWave}: Spawned horde of {enemiesSpawned} {enemyName} enemies at minute {currentMinute}");
        }
    }
    
    GameObject SelectEnemyTypeForHorde(int minute)
    {
        // Use the same probability system as EnemySpawner but select one type for entire horde
        float randomValue = Random.Range(0f, 100f);
        
        switch (minute)
        {
            case 0:
                // Minute 0: 100% Enemy1
                return GetEnemyPrefab(0);
                
            case 1:
                // Minute 1: 50% Enemy1, 50% Enemy2
                if (randomValue < 50f) return GetEnemyPrefab(0);
                else return GetEnemyPrefab(1);
                
            case 2:
                // Minute 2: 20% Enemy1, 80% Enemy2
                if (randomValue < 20f) return GetEnemyPrefab(0);
                else return GetEnemyPrefab(1);
                
            case 3:
                // Minute 3: 10% Enemy1, 40% Enemy2, 50% Enemy3
                if (randomValue < 10f) return GetEnemyPrefab(0);
                else if (randomValue < 50f) return GetEnemyPrefab(1);
                else return GetEnemyPrefab(2);
                
            case 4:
                // Minute 4: 5% Enemy1, 25% Enemy2, 70% Enemy3
                if (randomValue < 5f) return GetEnemyPrefab(0);
                else if (randomValue < 30f) return GetEnemyPrefab(1);
                else return GetEnemyPrefab(2);
                
            case 5:
                // Minute 5: 0% Enemy1, 15% Enemy2, 85% Enemy3
                if (randomValue < 15f) return GetEnemyPrefab(1);
                else return GetEnemyPrefab(2);
                
            case 6:
                // Minute 6: 0% Enemy1, 5% Enemy2, 50% Enemy3, 45% Enemy4
                if (randomValue < 5f) return GetEnemyPrefab(1);
                else if (randomValue < 55f) return GetEnemyPrefab(2);
                else return GetEnemyPrefab(3);
                
            case 7:
                // Minute 7: 0% Enemy1, 0% Enemy2, 35% Enemy3, 65% Enemy4
                if (randomValue < 35f) return GetEnemyPrefab(2);
                else return GetEnemyPrefab(3);
                
            case 8:
                // Minute 8: 0% Enemy1, 0% Enemy2, 15% Enemy3, 85% Enemy4
                if (randomValue < 15f) return GetEnemyPrefab(2);
                else return GetEnemyPrefab(3);
                
            default:
                // Minute 9+: 100% Enemy4
                return GetEnemyPrefab(3);
        }
    }
    
    GameObject GetEnemyPrefab(int index)
    {
        if (index >= 0 && index < enemyPrefabs.Length)
        {
            return enemyPrefabs[index];
        }
        
        Debug.LogWarning($"Enemy index {index} is out of bounds. Using Enemy1 as fallback.");
        return enemyPrefabs.Length > 0 ? enemyPrefabs[0] : null;
    }
    
    List<Vector2> GenerateValidSpawnPositions()
    {
        List<Vector2> validPositions = new List<Vector2>();
        spawnPositions.Clear();
        
        // Step 1: Find a valid central location for the horde
        Vector2 hordeCenter = FindValidHordeCenter();
        
        if (hordeCenter == Vector2.zero)
        {
            Debug.LogWarning("Could not find a valid horde center position!");
            spawnPositions = validPositions;
            return validPositions;
        }
        
        // Step 2: Generate clustered positions around the horde center
        validPositions = GenerateClusteredPositions(hordeCenter);
        
        spawnPositions = validPositions; // Store for gizmo drawing
        return validPositions;
    }
    
    Vector2 FindValidHordeCenter()
    {
        int maxAttempts = 50;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Generate random position within distance range from player
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            Vector2 candidateCenter = (Vector2)player.position + randomDirection * randomDistance;
            
            // Check if the entire horde cluster would fit within map bounds
            if (IsHordeAreaValid(candidateCenter))
            {
                return candidateCenter;
            }
        }
        
        Debug.LogWarning("Could not find valid horde center after maximum attempts!");
        return Vector2.zero;
    }
    
    bool IsHordeAreaValid(Vector2 center)
    {
        // Check if a circle around the center (with horde cluster radius) fits within map bounds
        Vector2[] edgePoints = {
            center + Vector2.up * hordeClusterRadius,
            center + Vector2.down * hordeClusterRadius,
            center + Vector2.left * hordeClusterRadius,
            center + Vector2.right * hordeClusterRadius
        };
        
        foreach (Vector2 point in edgePoints)
        {
            if (!IsWithinMapBounds(point))
            {
                return false;
            }
        }
        
        return true;
    }
    
    List<Vector2> GenerateClusteredPositions(Vector2 center)
    {
        List<Vector2> positions = new List<Vector2>();
        
        // Add center position first
        positions.Add(center);
        
        // Generate positions in a spiral pattern around the center
        float currentRadius = enemySpacing;
        int currentRingEnemies = 6; // Start with 6 enemies in first ring
        int placedEnemies = 1; // Center enemy already placed
        
        while (placedEnemies < hordeSize && currentRadius <= hordeClusterRadius)
        {
            for (int i = 0; i < currentRingEnemies && placedEnemies < hordeSize; i++)
            {
                float angle = (2f * Mathf.PI * i) / currentRingEnemies;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * currentRadius;
                Vector2 position = center + offset;
                
                // Add some randomization to make it look more natural
                Vector2 randomOffset = Random.insideUnitCircle * (enemySpacing * 0.3f);
                position += randomOffset;
                
                // Ensure position is still within bounds
                if (IsWithinMapBounds(position))
                {
                    positions.Add(position);
                    placedEnemies++;
                }
            }
            
            // Move to next ring
            currentRadius += enemySpacing;
            currentRingEnemies += 6; // Each ring has 6 more positions than the previous
        }
        
        if (showDebugInfo && placedEnemies < hordeSize)
        {
            Debug.Log($"Generated {placedEnemies} clustered positions out of {hordeSize} requested around center {center}");
        }
        
        return positions;
    }
    
    bool IsWithinMapBounds(Vector2 position)
    {
        // Use BackgroundTileGenerator's method if available, otherwise use fallback
        if (backgroundGenerator != null)
        {
            return backgroundGenerator.IsPositionWithinMap(position);
        }
        
        // Fallback calculation
        float halfWidth = mapWidth * 0.5f;
        float halfHeight = mapHeight * 0.5f;
        
        return position.x >= mapCenter.x - halfWidth && 
               position.x <= mapCenter.x + halfWidth && 
               position.y >= mapCenter.y - halfHeight && 
               position.y <= mapCenter.y + halfHeight;
    }
    

    
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        
        // Draw map bounds
        Gizmos.color = Color.yellow;
        Vector3 mapSize = new Vector3(mapWidth, mapHeight, 0);
        Gizmos.DrawWireCube(mapCenter, mapSize);
        
        // Draw player distance ranges
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer);
        }
        
        // Draw last spawn positions and horde center
        if (spawnPositions != null && spawnPositions.Count > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (Vector2 pos in spawnPositions)
            {
                Gizmos.DrawWireSphere(pos, 0.5f);
            }
            
            // Draw horde center (first position) and cluster radius
            if (spawnPositions.Count > 0)
            {
                Vector2 hordeCenter = spawnPositions[0];
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(hordeCenter, 1.5f); // Larger sphere for center
                
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(hordeCenter, hordeClusterRadius); // Show cluster area
            }
        }
    }
    
    // Public methods for external control
    public void TriggerHordeNow()
    {
        SpawnHorde();
    }
    
    public void SetWaveInterval(float newInterval)
    {
        waveInterval = newInterval;
        if (showDebugInfo)
        {
            Debug.Log($"Wave interval changed to {newInterval} seconds");
        }
    }
} 