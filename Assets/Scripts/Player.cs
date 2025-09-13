using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dragSensitivity = 1f;
    public bool smoothMovement = true;
    
    [Header("Wall Collision")]
    [SerializeField] private LayerMask wallLayerMask = -1; // Layer mask for walls (default: all layers)
    
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject fireballPrefab;  // Add FireBall prefab reference
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private bool useGun = true;  // Toggle for gun/bullet shooting
    [SerializeField] private bool useFireball = true;  // Toggle for fireball
    private float nextFireballTime;  // Separate cooldown for fireball
    
    [Header("Weapon Settings")]
    [SerializeField] private GameObject pf_scythe;  // Assign the Scythe prefab in inspector
    [SerializeField] private GameObject pf_sword;   // Assign the Sword prefab in inspector
    [SerializeField] private GameObject pf_spear;   // Add spear prefab reference
    
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    [SerializeField] private bool invincible = false; // Godmode toggle - no damage when checked

    [Header("Health Bar References")]
    [SerializeField] private Canvas healthBarCanvas; // Assign your canvas in inspector
    [SerializeField] private Image healthBarFill; // Assign your red fill image in inspector
    
    [Header("Flash Effect Settings")]
    [SerializeField] private float flashDuration = 0.1f; // Duration of red flash
    [SerializeField] private Color flashColor = Color.red; // Color to flash
    
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int currentExperience = 0;
    public int experienceToNextLevel = 100;
    private float levelExpMultiplier = 1.25f; // 25% increase per level
    
    private Camera mainCamera;
    private Mouse mouse;
    private Touchscreen touchscreen;
    private SpriteRenderer spriteRenderer;
    private Color originalColor; // Store original sprite color
    private Animator animator; // Animation controller
    private Collider2D playerCollider; // Player's collider for wall detection
    
    // Drag/swipe tracking variables
    private bool isDragging = false;
    private Vector3 startPressPosition;
    private Vector3 currentDragDirection = Vector3.zero;
    private bool wasPressed = false;
    
    private Vector3 targetPosition;
    private float nextFireTime;
    
    private UIManager uiManager;

    [Header("Weapon Management")]
    private Dictionary<WeaponData.WeaponType, WeaponData> weapons;
    private List<GameObject> activeWeaponObjects;

    [Header("Enemy Collision Detection")]
    private Dictionary<Collider2D, EnemyDamageData> enemiesInContact; // Track enemies currently colliding with player
    private Dictionary<Collider2D, Coroutine> damageCoroutines; // Track damage coroutines for each enemy
    
    // Helper class to store enemy damage data
    private class EnemyDamageData
    {
        public int damage;
        public MonoBehaviour enemyComponent;
        
        public EnemyDamageData(int damage, MonoBehaviour enemyComponent)
        {
            this.damage = damage;
            this.enemyComponent = enemyComponent;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Initialize level and experience
        currentLevel = 1;
        currentExperience = 0;
        experienceToNextLevel = 100;
        
        // Get reference to the main camera
        mainCamera = Camera.main;
        
        // If no main camera is found, try to find any camera
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        // Initialize target position to current position
        targetPosition = transform.position;
        
        // Get input devices
        mouse = Mouse.current;
        touchscreen = Touchscreen.current;
        
        // Get sprite renderer and store original color
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Get animator component
        animator = GetComponent<Animator>();

        // Get player collider for wall collision detection
        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogWarning("Player needs a Collider2D component for wall collision detection!");
        }

        // Initialize health bar 
        UpdateHealthBar();

        // Initialize weapons dictionary and list
        weapons = new Dictionary<WeaponData.WeaponType, WeaponData>();
        activeWeaponObjects = new List<GameObject>();

        // Initialize enemy collision tracking
        enemiesInContact = new Dictionary<Collider2D, EnemyDamageData>();
        damageCoroutines = new Dictionary<Collider2D, Coroutine>();

        // Initialize weapon data
        InitializeWeapons();

        // Start with only sword
        ActivateWeapon(WeaponData.WeaponType.Sword);

        // Get reference to UIManager
        uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager == null)
            Debug.LogWarning("UIManager not found in scene!");
    }

    private void InitializeWeapons()
    {
        // Initialize all possible weapons (but don't activate them yet)
        weapons = new Dictionary<WeaponData.WeaponType, WeaponData>();
        
        // Add each weapon type with proper initialization
        if (pf_sword != null)
            weapons.Add(WeaponData.WeaponType.Sword, new WeaponData(WeaponData.WeaponType.Sword, pf_sword));
            
        if (pf_spear != null)
            weapons.Add(WeaponData.WeaponType.Spear, new WeaponData(WeaponData.WeaponType.Spear, pf_spear));
            
        if (pf_scythe != null)
            weapons.Add(WeaponData.WeaponType.Scythe, new WeaponData(WeaponData.WeaponType.Scythe, pf_scythe));
            
        if (fireballPrefab != null)
            weapons.Add(WeaponData.WeaponType.Fireball, new WeaponData(WeaponData.WeaponType.Fireball, fireballPrefab));
            
        if (bulletPrefab != null)
            weapons.Add(WeaponData.WeaponType.Bullet, new WeaponData(WeaponData.WeaponType.Bullet, bulletPrefab));

        Debug.Log($"Initialized {weapons.Count} weapons");
    }

    public void ActivateWeapon(WeaponData.WeaponType type)
    {
        if (!weapons.ContainsKey(type)) return;

        WeaponData weapon = weapons[type];
        if (!weapon.isActive && weapon.prefab != null)
        {
            weapon.isActive = true;
            
            // Handle projectile weapons differently
            if (type == WeaponData.WeaponType.Fireball)
            {
                useFireball = true;
                Debug.Log($"Activated Fireball weapon");
            }
            else if (type == WeaponData.WeaponType.Bullet)
            {
                useGun = true;
                Debug.Log($"Activated Bullet weapon");
            }
            else
            {
                // For melee weapons (sword, spear, scythe)
                GameObject weaponObj = Instantiate(weapon.prefab, transform.position, Quaternion.identity);
                weaponObj.transform.SetParent(transform);

                // Set initial level for spear
                if (type == WeaponData.WeaponType.Spear)
                {
                    Spear spearComponent = weaponObj.GetComponent<Spear>();
                    if (spearComponent != null)
                    {
                        weapon.upgradeLevel = 0; // Start at 0, first upgrade will make it 1
                        spearComponent.level = 1;
                        spearComponent.attackDirection = 0f; // Initial direction will be updated by followPlayerDirection
                        spearComponent.followPlayerDirection = true; // Make initial spear follow player direction
                        Debug.Log($"Activated spear with player direction following enabled");
                    }
                }

                activeWeaponObjects.Add(weaponObj);
                Debug.Log($"Activated melee weapon: {type}");
            }
        }
    }

    public void UpgradeWeapon(WeaponData.WeaponType type)
    {
        if (!weapons.ContainsKey(type)) return;

        WeaponData weapon = weapons[type];
        
        // If weapon is not active, activate it first
        if (!weapon.isActive)
        {
            ActivateWeapon(type);
            return;
        }

        // Increase base damage and upgrade level
        weapon.UpgradeDamage();

        // Handle projectile weapons
        if (type == WeaponData.WeaponType.Fireball)
        {
            // Spawn fireball at player position to check current settings
            GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
            if (fireball.TryGetComponent<FireBall>(out var fireballComponent))
            {
                fireballComponent.damage = Mathf.RoundToInt(weapon.damage);
                fireballComponent.scale = 1f + (weapon.upgradeLevel * 0.05f); // Increase size by 0.1 each upgrade
                fireballComponent.lifetime = 2f + (weapon.upgradeLevel * 0.25f); // Increase lifetime by 0.5 seconds each upgrade
                Debug.Log($"Upgraded Fireball - Damage: {fireballComponent.damage}, Scale: {fireballComponent.scale:F2}, Lifetime: {fireballComponent.lifetime:F1}s");
            }
            Destroy(fireball); // Destroy the test fireball
            return;
        }
        else if (type == WeaponData.WeaponType.Bullet)
        {
            int totalBullets = 1 + ((weapon.upgradeLevel / 3) * 2);
            Debug.Log($"Upgraded Bullet - Damage: {weapon.damage}, Pierce Count: {weapon.upgradeLevel + 1}, " +
                     $"Speed: {5f + (weapon.upgradeLevel * 1f):F1}, Fire Rate: {Mathf.Max(0.75f * (1f - weapon.upgradeLevel * 0.04f), 0.2f):F2}s, " +
                     $"Number of Bullets: {totalBullets}");
            return;
        }

        // Handle melee weapons
        foreach (GameObject weaponObj in activeWeaponObjects.ToArray())
        {
            if (weaponObj != null)
            {
                switch (type)
                {
                    case WeaponData.WeaponType.Scythe:
                        Scythe scythe = weaponObj.GetComponent<Scythe>();
                        if (scythe != null)
                        {
                            scythe.damage = Mathf.RoundToInt(weapon.damage);
                            scythe.rotationSpeed *= 1.1f;
                            scythe.distanceFromPlayer *= 1;
                            weaponObj.transform.localScale *= 1.05f; // Increase scale by 5% each upgrade, same as fireball
                            //Debug.Log($"Updated Scythe - Damage: {scythe.damage}, Speed: {scythe.rotationSpeed:F1}, Distance: {scythe.distanceFromPlayer:F2}, Scale: {weaponObj.transform.localScale.x:F2}");
                        }
                        break;
                        
                    case WeaponData.WeaponType.Sword:
                        Sword sword = weaponObj.GetComponent<Sword>();
                        if (sword != null)
                        {
                            sword.damage = Mathf.RoundToInt(weapon.damage);
                            weaponObj.transform.localScale *= 1.1f;
                            sword.slashCooldown *= 0.95f;
                            Debug.Log($"Updated Sword - Damage: {sword.damage}, Scale: {weaponObj.transform.localScale.x:F2}, Cooldown: {sword.slashCooldown:F2}");
                        }
                        break;
                        
                    case WeaponData.WeaponType.Spear:
                        // Create a new list for non-spear weapons
                        List<GameObject> nonSpearWeapons = new List<GameObject>();
                        List<GameObject> currentSpears = new List<GameObject>();

                        // Separate spears from other weapons
                        foreach (var obj in activeWeaponObjects)
                        {
                            if (obj != null)
                            {
                                if (obj.GetComponent<Spear>() != null)
                                {
                                    currentSpears.Add(obj);
                                }
                                else
                                {
                                    nonSpearWeapons.Add(obj);
                                }
                            }
                        }

                        // Clear the active weapons list and add back non-spear weapons
                        activeWeaponObjects = new List<GameObject>(nonSpearWeapons);
                        
                        // Destroy old spears
                        foreach (var spear in currentSpears)
                        {
                            if (spear != null)
                            {
                                Destroy(spear);
                            }
                        }

                        // Create new spears based on upgrade level
                        float[] directions = GetSpearDirections(weapon.upgradeLevel);
                        Debug.Log($"Creating {directions.Length} spears for upgrade level {weapon.upgradeLevel}");
                        
                        foreach (float direction in directions)
                        {
                            GameObject newSpear = Instantiate(weapon.prefab, transform.position, Quaternion.identity);
                            newSpear.transform.SetParent(transform);
                            
                            Spear spearComponent = newSpear.GetComponent<Spear>();
                            if (spearComponent != null)
                            {
                                spearComponent.damage = Mathf.RoundToInt(weapon.damage);
                                spearComponent.level = weapon.upgradeLevel;
                                spearComponent.attackDirection = direction;
                                spearComponent.stabDuration *= 0.95f;
                                spearComponent.stabDistance *= 1.05f;
                                
                                // Set followPlayerDirection only for level 0 (initial spear)
                                spearComponent.followPlayerDirection = (weapon.upgradeLevel == 0);
                                
                                Debug.Log($"Created spear with direction: {direction} at upgrade level {weapon.upgradeLevel}, following player: {spearComponent.followPlayerDirection}");
                            }
                            
                            activeWeaponObjects.Add(newSpear);
                        }
                        
                        Debug.Log($"Updated Spear - Upgrade Level: {weapon.upgradeLevel}, Number of Spears: {directions.Length}, Damage: {weapon.damage}");
                        break;
                }
            }
        }
    }

    private float[] GetSpearDirections(int level)
    {
        Debug.Log($"Getting spear directions for level {level}");
        float[] directions;
        
        switch (level)
        {
            case 0:
                directions = new float[] { 0f }; // Initial activation: Right only
                break;
            case 1:
                directions = new float[] { 0f, 180f }; // First upgrade: Right and left
                break;
            case 2:
                directions = new float[] { 0f, 180f, 90f }; // Second upgrade: Right, left, and up
                break;
            case 3:
                directions = new float[] { 0f, 180f, 90f, 270f }; // Third upgrade: Right, left, up, and down
                break;
            default:
                directions = new float[] { 0f, 180f, 90f, 270f }; // Max level
                break;
        }
        
        Debug.Log($"Returning {directions.Length} directions: {string.Join(", ", directions)}");
        return directions;
    }

    public List<WeaponData.WeaponType> GetRandomUpgradeOptions(int count)
    {
        List<WeaponData.WeaponType> allWeapons = new List<WeaponData.WeaponType>(weapons.Keys);
        List<WeaponData.WeaponType> options = new List<WeaponData.WeaponType>();
        
        // No max level restrictions - allow all weapons to be upgraded indefinitely
        Debug.Log($"Available weapons for upgrade: {string.Join(", ", allWeapons)}");
        
        while (options.Count < count && allWeapons.Count > 0)
        {
            int index = Random.Range(0, allWeapons.Count);
            options.Add(allWeapons[index]);
            allWeapons.RemoveAt(index);
        }
        
        Debug.Log($"Selected upgrade options: {string.Join(", ", options)}");
        return options;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    // UpdateSwordPosition() function is removed as per the edit hint

    void HandleMovement()
    {
        // If game is paused, don't process movement
        if (Time.timeScale == 0f) return;

        Vector3 screenPosition = Vector3.zero;
        bool isPressed = false;
        
        // Check for mouse input (for editor/PC testing)
        if (mouse != null)
        {
            Vector2 mousePos = mouse.position.ReadValue();
            screenPosition = new Vector3(mousePos.x, mousePos.y, 0);
            isPressed = mouse.leftButton.isPressed;
        }
        // Check for touch input (for mobile)
        else if (touchscreen != null)
        {
            Vector2 touchPos = touchscreen.primaryTouch.position.ReadValue();
            screenPosition = new Vector3(touchPos.x, touchPos.y, 0);
            isPressed = touchscreen.primaryTouch.press.isPressed;
        }
        
        // Handle drag start
        if (isPressed && !wasPressed)
        {
            isDragging = true;
            startPressPosition = screenPosition;
            currentDragDirection = Vector3.zero;
        }
        // Handle drag in progress
        else if (isPressed && wasPressed && isDragging)
        {
            // Calculate drag direction in screen space
            Vector3 dragVector = screenPosition - startPressPosition;
            
            // Convert drag direction to world space direction (normalize to get direction only)
            if (dragVector.magnitude > 10f) // Minimum drag threshold to avoid jitter
            {
                // Convert to world space direction
                Vector3 worldDragStart = mainCamera.ScreenToWorldPoint(startPressPosition);
                Vector3 worldDragCurrent = mainCamera.ScreenToWorldPoint(screenPosition);
                
                // Calculate direction (normalized)
                currentDragDirection = (worldDragCurrent - worldDragStart).normalized;
                currentDragDirection.z = 0; // Keep it 2D
                
                // Flip sprite based on movement direction
                if (spriteRenderer != null)
                {
                    if (currentDragDirection.x < 0)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else if (currentDragDirection.x > 0)
                    {
                        spriteRenderer.flipX = false;
                    }
                }
                
                // Calculate movement
                Vector3 movement = currentDragDirection * moveSpeed * dragSensitivity * Time.deltaTime;
                
                // Check for wall collision before applying movement
                Vector3 newPosition = transform.position + movement;
                if (!IsCollidingWithWall(newPosition))
                {
                    // Safe to move
                    transform.position = newPosition;
                }
                else
                {
                    // Try moving in individual axes to allow sliding along walls
                    Vector3 horizontalMovement = new Vector3(movement.x, 0, 0);
                    Vector3 verticalMovement = new Vector3(0, movement.y, 0);
                    
                    // Try horizontal movement
                    if (!IsCollidingWithWall(transform.position + horizontalMovement))
                    {
                        transform.position += horizontalMovement;
                    }
                    // Try vertical movement
                    else if (!IsCollidingWithWall(transform.position + verticalMovement))
                    {
                        transform.position += verticalMovement;
                    }
                    // If both fail, don't move
                }
                
                // Update animation based on movement
                bool isMoving = movement.magnitude > 0.01f;
                if (animator != null)
                {
                    animator.SetBool("isRunning", isMoving);
                }
            }
        }
        // Handle drag end
        else if (!isPressed && wasPressed)
        {
            isDragging = false;
            currentDragDirection = Vector3.zero;
            
            // Stop running animation when drag ends
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
            }
        }
        
        // Update previous press state
        wasPressed = isPressed;
    }
    
    private bool IsCollidingWithWall(Vector3 position)
    {
        if (playerCollider == null) return false;
        
        // Calculate the bounds at the new position
        Bounds playerBounds = playerCollider.bounds;
        Vector3 offset = position - transform.position;
        playerBounds.center += offset;
        
        // Check for overlaps with walls using the calculated bounds
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(wallLayerMask);
        contactFilter.useTriggers = true; // Include both triggers and solid colliders
        
        Collider2D[] results = new Collider2D[10]; // Array to store collision results
        int hitCount = Physics2D.OverlapBox(playerBounds.center, playerBounds.size, 0f, contactFilter, results);
        
        // Check if any of the overlapping colliders are walls
        for (int i = 0; i < hitCount; i++)
        {
            if (results[i] != null && results[i].CompareTag("Wall"))
            {
                return true;
            }
        }
        
        return false;
    }

    void HandleShooting()
    {
        // If game is paused, don't process shooting
        if (Time.timeScale == 0f) return;

        // Get the weapon data for projectiles
        WeaponData bulletWeapon = weapons.ContainsKey(WeaponData.WeaponType.Bullet) ? weapons[WeaponData.WeaponType.Bullet] : null;
        WeaponData fireballWeapon = weapons.ContainsKey(WeaponData.WeaponType.Fireball) ? weapons[WeaponData.WeaponType.Fireball] : null;

        // Handle bullet shooting
        if (useGun && bulletWeapon != null && bulletWeapon.isActive && Time.time >= nextFireTime)
        {
            // Calculate number of bullets based on upgrade level (increase by 2 every 3 levels)
            int extraBullets = (bulletWeapon.upgradeLevel / 3) * 2; // Integer division by 3 gives us the number of "3 level" increments
            int totalBullets = 1 + extraBullets;
            
            // Calculate starting angle for spread
            float totalSpread = 30f * (totalBullets - 1); // 30 degrees between each bullet
            float startAngle = -totalSpread / 2f; // Start from negative half of total spread
            
            // Find closest enemy for center bullet direction
            Vector3 centerDirection = Vector3.up; // Default direction
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies.Length > 0)
            {
                float closestDistance = Mathf.Infinity;
                Transform closestEnemy = null;
                
                foreach (GameObject enemy in enemies)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy.transform;
                    }
                }
                
                if (closestEnemy != null)
                {
                    centerDirection = (closestEnemy.position - transform.position).normalized;
                }
            }
            
            // Spawn bullets with spread
            for (int i = 0; i < totalBullets; i++)
            {
                // Calculate rotation for this bullet
                float angle = startAngle + (30f * i); // 30 degrees between each bullet
                Vector3 bulletDirection = RotateVector(centerDirection, angle);
                
                // Spawn bullet
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                if (bullet.TryGetComponent<Bullet>(out var bulletComponent))
                {
                    bulletComponent.damage = Mathf.RoundToInt(bulletWeapon.damage);
                    bulletComponent.maxPierceCount = bulletWeapon.upgradeLevel + 1;
                    bulletComponent.speed = 5f + (bulletWeapon.upgradeLevel * 1f);
                    bulletComponent.SetInitialDirectionManually(bulletDirection); // We'll add this method to Bullet.cs
                }
            }
            
            // Update next fire time with improved fire rate based on level
            float baseFireRate = 0.75f;
            float fireRateReduction = bulletWeapon.upgradeLevel * 0.04f;
            float currentFireRate = Mathf.Max(baseFireRate * (1f - fireRateReduction), 0.2f);
            nextFireTime = Time.time + currentFireRate;
        }

        // Handle fireball shooting
        if (useFireball && fireballWeapon != null && fireballWeapon.isActive && Time.time >= nextFireballTime)
        {
            // Spawn fireball at player position
            GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
            if (fireball.TryGetComponent<FireBall>(out var fireballComponent))
            {
                fireballComponent.damage = Mathf.RoundToInt(fireballWeapon.damage);
                fireballComponent.scale = 1f + (fireballWeapon.upgradeLevel * 0.1f); // Apply current scale
                fireballComponent.lifetime = 2f + (fireballWeapon.upgradeLevel * 0.5f); // Apply current lifetime
            }
            
            // Update next fireball time (using same fire rate)
            nextFireballTime = Time.time + fireRate;
        }
    }
    
    public void TakeDamage(int damage)
    {
        // Check if player is invincible
        if (invincible)
        {
            //Debug.Log($"Player is invincible! Blocked {damage} damage.");
            return; // Exit early, no damage taken
        }
        
        currentHealth -= damage;
        
        // Flash red when taking damage
        FlashRed();
        
        // Update health bar
        UpdateHealthBar(); // Add this line
        
        // Print to console
        //Debug.Log("Player took " + damage + " damage! Current health: " + currentHealth);
        
        // Check if player is dead
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateHealthBar(); // Add this line
            Debug.Log("Player has died!");
            // Destroy the player GameObject
            Destroy(gameObject);
        }
    }
    
    private void FlashRed()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashCoroutine());
        }
    }
    
    private System.Collections.IEnumerator FlashCoroutine()
    {
        // Change to flash color
        spriteRenderer.color = flashColor;
        
        // Use WaitForSecondsRealtime instead of WaitForSeconds to work during pause
        yield return new WaitForSecondsRealtime(flashDuration);
        
        // Return to original color
        spriteRenderer.color = originalColor;
    }
    
    public void GainExperience(int experiencePoints)
    {
        currentExperience += experiencePoints;
        
        // Check for level up
        while (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }
        
        // Print to console
        //Debug.Log($"Player gained {experiencePoints} experience! Level: {currentLevel}, Experience: {currentExperience}/{experienceToNextLevel}");
    }

    private void LevelUp()
    {
        // Subtract experience needed for this level
        currentExperience -= experienceToNextLevel;
        
        // Increase level
        currentLevel++;
        
        // Calculate new experience requirement (25% more than last level)
        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * levelExpMultiplier);
        
        Debug.Log($"Level Up! Now level {currentLevel}! Next level requires {experienceToNextLevel} experience.");

        // Show level up popup
        if (uiManager != null)
            uiManager.ShowLevelUpPopup();
    }
    
    // Public methods for invincibility control
    public void SetInvincible(bool isInvincible)
    {
        invincible = isInvincible;
        Debug.Log($"Player invincibility set to: {invincible}");
    }
    
    public bool IsInvincible()
    {
        return invincible;
    }

    // Public method for healing the player (used by HealthPack)
    public void Heal(int healAmount)
    {
        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthBar();
        
        int actualHeal = currentHealth - oldHealth;
        if (actualHeal > 0)
        {
            Debug.Log($"Player healed for {actualHeal} health. Current health: {currentHealth}/{maxHealth}");
        }
    }

    // Public method to fully restore health (used by HealthPack)
    public void FullHeal()
    {
        int healAmount = maxHealth - currentHealth;
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        if (healAmount > 0)
        {
            Debug.Log($"Player fully healed for {healAmount} health. Current health: {currentHealth}/{maxHealth}");
        }
        else
        {
            Debug.Log("Player was already at full health.");
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = healthPercentage;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Get enemy damage value from the appropriate enemy component
            int enemyDamage = 0;
            MonoBehaviour enemyComponent = null;
            
            // Try to get damage from each enemy type
            Enemy1 enemy1 = other.GetComponent<Enemy1>();
            Enemy2 enemy2 = other.GetComponent<Enemy2>();
            Enemy3 enemy3 = other.GetComponent<Enemy3>();
            Enemy4 enemy4 = other.GetComponent<Enemy4>();
            
            if (enemy1 != null)
            {
                enemyDamage = enemy1.damage;
                enemyComponent = enemy1;
            }
            else if (enemy2 != null)
            {
                enemyDamage = enemy2.damage;
                enemyComponent = enemy2;
            }
            else if (enemy3 != null)
            {
                enemyDamage = enemy3.damage;
                enemyComponent = enemy3;
            }
            else if (enemy4 != null)
            {
                enemyDamage = enemy4.damage;
                enemyComponent = enemy4;
            }
            
            // If we found a valid enemy, start damage
            if (enemyComponent != null && enemyDamage > 0)
            {
                // Store enemy data
                EnemyDamageData damageData = new EnemyDamageData(enemyDamage, enemyComponent);
                enemiesInContact[other] = damageData;
                
                // Start continuous damage coroutine for this enemy
                Coroutine damageCoroutine = StartCoroutine(ContinuousDamageFromEnemy(other, damageData));
                damageCoroutines[other] = damageCoroutine;
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Stop damage from this enemy
            if (damageCoroutines.ContainsKey(other))
            {
                StopCoroutine(damageCoroutines[other]);
                damageCoroutines.Remove(other);
            }
            
            // Remove enemy from contact list
            if (enemiesInContact.ContainsKey(other))
            {
                enemiesInContact.Remove(other);
            }
        }
    }
    
    private System.Collections.IEnumerator ContinuousDamageFromEnemy(Collider2D enemyCollider, EnemyDamageData damageData)
    {
        while (enemiesInContact.ContainsKey(enemyCollider) && damageData.enemyComponent != null)
        {
            // Deal damage to the player
            TakeDamage(damageData.damage);
            
            // Wait for 0.5 seconds before next damage (same as original system)
            yield return new WaitForSeconds(0.5f);
        }
        
        // Clean up when coroutine ends
        if (damageCoroutines.ContainsKey(enemyCollider))
        {
            damageCoroutines.Remove(enemyCollider);
        }
        if (enemiesInContact.ContainsKey(enemyCollider))
        {
            enemiesInContact.Remove(enemyCollider);
        }
    }

    // Helper method to rotate a vector by an angle
    private Vector3 RotateVector(Vector3 vector, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        
        return new Vector3(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos,
            0f
        );
    }
}
