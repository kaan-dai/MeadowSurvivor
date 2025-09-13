using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameItemShop : MonoBehaviour
{
    [Header("Item Prices")]
    [SerializeField] private int healthPackPrice = 50;
    [SerializeField] private int speedBoostPrice = 100;
    [SerializeField] private int bombPrice = 150;
    [SerializeField] private int weaponUpgradePrice = 200;
    
    [Header("UI Elements")]
    [SerializeField] private Button healthPackButton;
    [SerializeField] private Button speedBoostButton;
    [SerializeField] private Button bombButton;
    [SerializeField] private Button weaponUpgradeButton;
    
    [Header("Price Display")]
    [SerializeField] private TextMeshProUGUI healthPackPriceText;
    [SerializeField] private TextMeshProUGUI speedBoostPriceText;
    [SerializeField] private TextMeshProUGUI bombPriceText;
    [SerializeField] private TextMeshProUGUI weaponUpgradePriceText;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject healthPackPrefab;
    [SerializeField] private GameObject speedBoostPrefab;
    [SerializeField] private GameObject bombPrefab;
    
    void Start()
    {
        SetupUI();
        UpdatePriceDisplays();
        
        // Subscribe to coin changes to update button states
        CurrencyManager.OnCoinsChanged += OnCoinsChanged;
        OnCoinsChanged(CurrencyManager.Instance?.GetCoins() ?? 0);
    }
    
    void OnDestroy()
    {
        CurrencyManager.OnCoinsChanged -= OnCoinsChanged;
    }
    
    void SetupUI()
    {
        if (healthPackButton != null)
            healthPackButton.onClick.AddListener(BuyHealthPack);
        if (speedBoostButton != null)
            speedBoostButton.onClick.AddListener(BuySpeedBoost);
        if (bombButton != null)
            bombButton.onClick.AddListener(BuyBomb);
        if (weaponUpgradeButton != null)
            weaponUpgradeButton.onClick.AddListener(BuyWeaponUpgrade);
    }
    
    void UpdatePriceDisplays()
    {
        if (healthPackPriceText != null)
            healthPackPriceText.text = $"{healthPackPrice} coins";
        if (speedBoostPriceText != null)
            speedBoostPriceText.text = $"{speedBoostPrice} coins";
        if (bombPriceText != null)
            bombPriceText.text = $"{bombPrice} coins";
        if (weaponUpgradePriceText != null)
            weaponUpgradePriceText.text = $"{weaponUpgradePrice} coins";
    }
    
    void OnCoinsChanged(int newCoinAmount)
    {
        // Enable/disable buttons based on affordability
        if (healthPackButton != null)
            healthPackButton.interactable = newCoinAmount >= healthPackPrice;
        if (speedBoostButton != null)
            speedBoostButton.interactable = newCoinAmount >= speedBoostPrice;
        if (bombButton != null)
            bombButton.interactable = newCoinAmount >= bombPrice;
        if (weaponUpgradeButton != null)
            weaponUpgradeButton.interactable = newCoinAmount >= weaponUpgradePrice;
    }
    
    public void BuyHealthPack()
    {
        if (CurrencyManager.Instance?.SpendCoins(healthPackPrice) == true)
        {
            SpawnItemNearPlayer(healthPackPrefab);
            Debug.Log("Purchased Health Pack!");
        }
    }
    
    public void BuySpeedBoost()
    {
        if (CurrencyManager.Instance?.SpendCoins(speedBoostPrice) == true)
        {
            SpawnItemNearPlayer(speedBoostPrefab);
            Debug.Log("Purchased Speed Boost!");
        }
    }
    
    public void BuyBomb()
    {
        if (CurrencyManager.Instance?.SpendCoins(bombPrice) == true)
        {
            SpawnItemNearPlayer(bombPrefab);
            Debug.Log("Purchased Bomb!");
        }
    }
    
    public void BuyWeaponUpgrade()
    {
        if (CurrencyManager.Instance?.SpendCoins(weaponUpgradePrice) == true)
        {
            // Give the player experience to trigger a level up and weapon upgrade
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                player.GainExperience(player.experienceToNextLevel);
                Debug.Log("Purchased Weapon Upgrade!");
            }
        }
    }
    
    private void SpawnItemNearPlayer(GameObject itemPrefab)
    {
        if (itemPrefab == null) return;
        
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            Vector3 spawnPosition = player.transform.position + Vector3.right * 2f;
            Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        }
    }
} 