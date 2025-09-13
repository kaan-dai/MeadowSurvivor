using UnityEngine;

public class Magnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    [SerializeField] private float magnetStrength = 15f; // New move speed for ExBalls
    [SerializeField] private float magnetRange = 50f; // New detection range for ExBalls
    [SerializeField] private float magnetDuration = 5f; // How long the magnet effect lasts
    [SerializeField] private float lifetime = 30f; // How long the magnet stays before disappearing
    
    [Header("Visual Effects")]
    [SerializeField] private float collectFlashDuration = 0.2f;
    [SerializeField] private Color collectFlashColor = Color.magenta;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    void Start()
    {
        // Get sprite renderer and store original color
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Destroy after lifetime to prevent clutter
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // Activate magnet effect
                StartCoroutine(MagnetEffect());
                
                // Visual feedback
                StartCoroutine(CollectEffect(player));
                
                Debug.Log($"Magnet collected! All ExBalls are now attracted to the player for {magnetDuration} seconds.");
                
                // Destroy the magnet
                Destroy(gameObject);
            }
        }
    }
    
    private System.Collections.IEnumerator MagnetEffect()
    {
        // Find all ExBall objects in the scene
        ExBall10[] exBalls10 = FindObjectsOfType<ExBall10>();
        ExBall25[] exBalls25 = FindObjectsOfType<ExBall25>();
        ExBall50[] exBalls50 = FindObjectsOfType<ExBall50>();
        
        // Store original values to restore later
        System.Collections.Generic.List<ExBallData> originalData = new System.Collections.Generic.List<ExBallData>();
        
        // Modify ExBall10 objects
        foreach (ExBall10 exBall in exBalls10)
        {
            if (exBall != null)
            {
                originalData.Add(new ExBallData(exBall.gameObject, exBall.moveSpeed, exBall.detectionRange));
                exBall.moveSpeed = magnetStrength;
                exBall.detectionRange = magnetRange;
            }
        }
        
        // Modify ExBall25 objects
        foreach (ExBall25 exBall in exBalls25)
        {
            if (exBall != null)
            {
                originalData.Add(new ExBallData(exBall.gameObject, exBall.moveSpeed, exBall.detectionRange));
                exBall.moveSpeed = magnetStrength;
                exBall.detectionRange = magnetRange;
            }
        }
        
        // Modify ExBall50 objects
        foreach (ExBall50 exBall in exBalls50)
        {
            if (exBall != null)
            {
                originalData.Add(new ExBallData(exBall.gameObject, exBall.moveSpeed, exBall.detectionRange));
                exBall.moveSpeed = magnetStrength;
                exBall.detectionRange = magnetRange;
            }
        }
        
        int totalExBalls = exBalls10.Length + exBalls25.Length + exBalls50.Length;
        Debug.Log($"Magnet effect activated on {totalExBalls} ExBalls!");
        
        // Wait for the magnet duration
        yield return new WaitForSeconds(magnetDuration);
        
        // Restore original values for any ExBalls that still exist
        foreach (ExBallData data in originalData)
        {
            if (data.gameObject != null) // Check if ExBall still exists (not collected)
            {
                ExBall10 exBall10 = data.gameObject.GetComponent<ExBall10>();
                ExBall25 exBall25 = data.gameObject.GetComponent<ExBall25>();
                ExBall50 exBall50 = data.gameObject.GetComponent<ExBall50>();
                
                if (exBall10 != null)
                {
                    exBall10.moveSpeed = data.originalMoveSpeed;
                    exBall10.detectionRange = data.originalDetectionRange;
                }
                else if (exBall25 != null)
                {
                    exBall25.moveSpeed = data.originalMoveSpeed;
                    exBall25.detectionRange = data.originalDetectionRange;
                }
                else if (exBall50 != null)
                {
                    exBall50.moveSpeed = data.originalMoveSpeed;
                    exBall50.detectionRange = data.originalDetectionRange;
                }
            }
        }
        
        Debug.Log("Magnet effect ended.");
    }
    
    private System.Collections.IEnumerator CollectEffect(Player player)
    {
        // Flash the player magenta briefly to show magnet effect
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            Color playerOriginalColor = playerSprite.color;
            playerSprite.color = collectFlashColor;
            
            yield return new WaitForSeconds(collectFlashDuration);
            
            playerSprite.color = playerOriginalColor;
        }
    }
    
    // Helper class to store original ExBall data
    private class ExBallData
    {
        public GameObject gameObject;
        public float originalMoveSpeed;
        public float originalDetectionRange;
        
        public ExBallData(GameObject obj, float moveSpeed, float detectionRange)
        {
            this.gameObject = obj;
            this.originalMoveSpeed = moveSpeed;
            this.originalDetectionRange = detectionRange;
        }
    }
} 