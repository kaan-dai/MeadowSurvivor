using UnityEngine;
using UnityEngine.Tilemaps;

public class BackgroundTileGenerator : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private Sprite[] tileSprites; // Assign Tile0-Tile5 sprites here
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TilemapRenderer tilemapRenderer;
    
    [Header("Generation Settings")]
    [SerializeField] private int mapWidth = 200;
    [SerializeField] private int mapHeight = 200;
    [SerializeField] private Vector2Int centerOffset = Vector2Int.zero;
    [SerializeField] private bool generateOnStart = true;
    
    [Header("Randomization Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float tile0Probability = 0.7f; // 70% chance for Tile0
    
    private Tile[] tiles;
    
    void Start()
    {
        // Find components if not assigned
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();
        if (tilemapRenderer == null)
            tilemapRenderer = GetComponent<TilemapRenderer>();
        
        // Create tiles from sprites
        CreateTilesFromSprites();
        
        // Generate static background once
        if (generateOnStart)
        {
            GenerateBackground();
        }
    }
    
    // No Update method needed - background is static
    
    void CreateTilesFromSprites()
    {
        if (tileSprites == null || tileSprites.Length == 0)
        {
            Debug.LogWarning("No tile sprites assigned to BackgroundTileGenerator!");
            return;
        }
        
        tiles = new Tile[tileSprites.Length];
        
        for (int i = 0; i < tileSprites.Length; i++)
        {
            if (tileSprites[i] != null)
            {
                tiles[i] = ScriptableObject.CreateInstance<Tile>();
                tiles[i].sprite = tileSprites[i];
                tiles[i].name = $"Tile{i}";
            }
        }
    }
    
    public void GenerateBackground()
    {
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogWarning("No tiles available for background generation!");
            return;
        }
        
        Vector3Int startPos = new Vector3Int(
            centerOffset.x - mapWidth / 2,
            centerOffset.y - mapHeight / 2,
            0
        );
        
        GenerateArea(startPos, mapWidth, mapHeight);
    }
    

    
    void GenerateArea(Vector3Int startPos, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int position = new Vector3Int(startPos.x + x, startPos.y + y, 0);
                Tile selectedTile = GetRandomTile();
                
                if (selectedTile != null)
                {
                    tilemap.SetTile(position, selectedTile);
                }
            }
        }
    }
    
    Tile GetRandomTile()
    {
        if (tiles == null || tiles.Length == 0) return null;
        
        float randomValue = Random.Range(0f, 1f);
        
        // Check if we should use Tile0 (most common)
        if (randomValue < tile0Probability && tiles[0] != null)
        {
            return tiles[0];
        }
        
        // Use one of the other tiles (Tile1-Tile5)
        if (tiles.Length > 1)
        {
            int randomIndex = Random.Range(1, tiles.Length);
            return tiles[randomIndex];
        }
        
        // Fallback to Tile0 if no other tiles available
        return tiles[0];
    }
    
    [ContextMenu("Regenerate Background")]
    public void RegenerateBackground()
    {
        if (tilemap != null)
        {
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = new TileBase[bounds.size.x * bounds.size.y * bounds.size.z];
            tilemap.SetTilesBlock(bounds, allTiles); // Clear all tiles
        }
        
        GenerateBackground();
    }
    
    [ContextMenu("Clear Background")]
    public void ClearBackground()
    {
        if (tilemap != null)
        {
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = new TileBase[bounds.size.x * bounds.size.y * bounds.size.z];
            tilemap.SetTilesBlock(bounds, allTiles);
        }
    }
    
    // Public methods to get map bounds for other systems (like HordeSpawner)
    public Vector2 GetMapBounds()
    {
        return new Vector2(mapWidth, mapHeight);
    }
    
    public Vector2 GetMapCenter()
    {
        return new Vector2(centerOffset.x, centerOffset.y);
    }
    
    public bool IsPositionWithinMap(Vector2 worldPosition)
    {
        float halfWidth = mapWidth * 0.5f;
        float halfHeight = mapHeight * 0.5f;
        Vector2 center = GetMapCenter();
        
        return worldPosition.x >= center.x - halfWidth && 
               worldPosition.x <= center.x + halfWidth && 
               worldPosition.y >= center.y - halfHeight && 
               worldPosition.y <= center.y + halfHeight;
    }
    
    public Bounds GetWorldBounds()
    {
        Vector2 center = GetMapCenter();
        Vector2 size = GetMapBounds();
        return new Bounds(center, size);
    }
}