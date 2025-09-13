using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    [Header("Currency Settings")]
    [SerializeField] private int startingCoins = 0;
    
    private int currentCoins;
    
    // Events for UI updates
    public static event Action<int> OnCoinsChanged;
    
    // Singleton instance
    public static CurrencyManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Initialize with starting coins if first time
        if (currentCoins == 0 && !HasSavedCoins())
        {
            currentCoins = startingCoins;
            SaveCoins();
        }
        
        // Notify UI of current coin amount
        OnCoinsChanged?.Invoke(currentCoins);
    }
    
    public int GetCoins()
    {
        return currentCoins;
    }
    
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        
        currentCoins += amount;
        SaveCoins();
        OnCoinsChanged?.Invoke(currentCoins);
        
        Debug.Log($"Added {amount} coins. Total: {currentCoins}");
    }
    
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || currentCoins < amount)
        {
            Debug.Log($"Cannot spend {amount} coins. Current balance: {currentCoins}");
            return false;
        }
        
        currentCoins -= amount;
        SaveCoins();
        OnCoinsChanged?.Invoke(currentCoins);
        
        Debug.Log($"Spent {amount} coins. Remaining: {currentCoins}");
        return true;
    }
    
    public bool CanAfford(int amount)
    {
        return currentCoins >= amount;
    }
    
    private void SaveCoins()
    {
        PlayerPrefs.SetInt("PlayerCoins", currentCoins);
        PlayerPrefs.Save();
    }
    
    private void LoadCoins()
    {
        currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
    }
    
    private bool HasSavedCoins()
    {
        return PlayerPrefs.HasKey("PlayerCoins");
    }
    
    // For testing purposes - remove in production
    [ContextMenu("Add 100 Coins")]
    private void TestAddCoins100()
    {
        AddCoins(100);
    }
    
    [ContextMenu("Spend 50 Coins")]
    private void TestSpendCoins50()
    {
        SpendCoins(50);
    }

    [ContextMenu("Spend 100 Coins")]
    private void TestSpendCoins100()
    {
        SpendCoins(100);
    }

    [ContextMenu("Spend 1000 Coins")]
    private void TestSpendCoins1000()
    {
        SpendCoins(1000);
    }

    [ContextMenu("Spend 10000 Coins")]
    private void TestSpendCoins10000()
    {
        SpendCoins(10000);
    }
} 