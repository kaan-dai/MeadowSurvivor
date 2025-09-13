using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using System;

public class IAPManager : MonoBehaviour, IStoreListener
{
    [Header("Product IDs")]
    [SerializeField] private string coins100ProductId = "com.yourgame.coins100";
    [SerializeField] private string coins500ProductId = "com.yourgame.coins500";
    [SerializeField] private string coins1000ProductId = "com.yourgame.coins1000";
    [SerializeField] private string coins2500ProductId = "com.yourgame.coins2500";
    [SerializeField] private string noAdsProductId = "com.yourgame.remove_ads";
    
    [Header("Coin Packages")]
    [SerializeField] private int coins100Amount = 100;
    [SerializeField] private int coins500Amount = 500;
    [SerializeField] private int coins1000Amount = 1000;
    [SerializeField] private int coins2500Amount = 2500;
    
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;
    
    // Events - renamed to avoid conflicts with IStoreListener interface
    public static event Action OnStoreInitialized;
    public static event Action<string> OnPurchaseSucceeded;
    public static event Action<string> OnStorePurchaseFailed;
    
    // Singleton
    public static IAPManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        InitializePurchasing();
    }
    
    public void InitializePurchasing()
    {
        if (IsInitialized()) return;
        
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        
        // Add consumable products (coins)
        builder.AddProduct(coins100ProductId, ProductType.Consumable);
        builder.AddProduct(coins500ProductId, ProductType.Consumable);
        builder.AddProduct(coins1000ProductId, ProductType.Consumable);
        builder.AddProduct(coins2500ProductId, ProductType.Consumable);
        
        // Add non-consumable product (remove ads)
        builder.AddProduct(noAdsProductId, ProductType.NonConsumable);
        
        UnityPurchasing.Initialize(this, builder);
    }
    
    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }
    
    // Purchase Methods
    public void BuyCoins100()
    {
        BuyProductID(coins100ProductId);
    }
    
    public void BuyCoins500()
    {
        BuyProductID(coins500ProductId);
    }
    
    public void BuyCoins1000()
    {
        BuyProductID(coins1000ProductId);
    }
    
    public void BuyCoins2500()
    {
        BuyProductID(coins2500ProductId);
    }
    
    public void BuyRemoveAds()
    {
        BuyProductID(noAdsProductId);
    }
    
    void BuyProductID(string productId)
    {
        if (!IsInitialized())
        {
            Debug.LogError("IAP not initialized!");
            OnStorePurchaseFailed?.Invoke(productId);
            return;
        }
        
        Product product = storeController.products.WithID(productId);
        
        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"Purchasing product: {product.definition.id}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogError($"Product {productId} not available for purchase");
            OnStorePurchaseFailed?.Invoke(productId);
        }
    }
    
    public string GetProductPrice(string productId)
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);
            if (product != null)
            {
                return product.metadata.localizedPriceString;
            }
        }
        return "N/A";
    }
    
    // IStoreListener Implementation
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAP Successfully Initialized");
        storeController = controller;
        storeExtensionProvider = extensions;
        OnStoreInitialized?.Invoke();
    }
    
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"IAP Initialization Failed: {error}");
    }
    
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP Initialization Failed: {error} - {message}");
    }
    
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log($"Processing purchase: {args.purchasedProduct.definition.id}");
        
        // Validate receipt (important for security)
        if (ValidateReceipt(args.purchasedProduct.receipt))
        {
            // Handle the purchase based on product ID
            HandleSuccessfulPurchase(args.purchasedProduct.definition.id);
            OnPurchaseSucceeded?.Invoke(args.purchasedProduct.definition.id);
        }
        else
        {
            Debug.LogError("Receipt validation failed!");
            OnStorePurchaseFailed?.Invoke(args.purchasedProduct.definition.id);
            return PurchaseProcessingResult.Pending;
        }
        
        return PurchaseProcessingResult.Complete;
    }
    
    private bool ValidateReceipt(string receipt)
    {
        // Basic validation - in production, you should implement proper receipt validation
        // For security, validate receipts on your server
        
        #if UNITY_EDITOR
        return true; // Skip validation in editor
        #endif
        
        // For now, we'll skip receipt validation since GooglePlayTangle and AppleTangle
        // are only generated when you set up receipt validation in Unity IAP
        // To enable receipt validation:
        // 1. Go to Window > Unity IAP > Receipt Validation Obfuscator
        // 2. Follow the setup instructions for your platform
        // 3. Uncomment the code below and remove this return statement
        return true;
        
        /*
        try
        {
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            var result = validator.Validate(receipt);
            return true;
        }
        catch (IAPSecurityException)
        {
            return false;
        }
        */
    }
    
    private void HandleSuccessfulPurchase(string productId)
    {
        switch (productId)
        {
            case var id when id == coins100ProductId:
                CurrencyManager.Instance?.AddCoins(coins100Amount);
                break;
            case var id when id == coins500ProductId:
                CurrencyManager.Instance?.AddCoins(coins500Amount);
                break;
            case var id when id == coins1000ProductId:
                CurrencyManager.Instance?.AddCoins(coins1000Amount);
                break;
            case var id when id == coins2500ProductId:
                CurrencyManager.Instance?.AddCoins(coins2500Amount);
                break;
            case var id when id == noAdsProductId:
                // Handle remove ads
                PlayerPrefs.SetInt("AdsRemoved", 1);
                PlayerPrefs.Save();
                break;
            default:
                Debug.LogWarning($"Unknown product ID: {productId}");
                break;
        }
    }
    
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed for {product.definition.id}: {failureReason}");
        OnStorePurchaseFailed?.Invoke(product.definition.id);
    }
    
    // Utility methods
    public bool HasPurchasedNoAds()
    {
        return PlayerPrefs.GetInt("AdsRemoved", 0) == 1;
    }
    
    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.LogError("IAP not initialized!");
            return;
        }
        
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result) =>
            {
                Debug.Log($"Restore transactions result: {result}");
            });
        }
        else
        {
            Debug.Log("Restore purchases not supported on this platform");
        }
    }
} 