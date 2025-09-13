using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI coinsDisplay;
    [SerializeField] private Button closeButton;
    
    [Header("Coin Package Buttons")]
    [SerializeField] private Button coins100Button;
    [SerializeField] private Button coins500Button;
    [SerializeField] private Button coins1000Button;
    [SerializeField] private Button coins2500Button;
    
    [Header("Coin Package Price Labels")]
    [SerializeField] private TextMeshProUGUI coins100PriceText;
    [SerializeField] private TextMeshProUGUI coins500PriceText;
    [SerializeField] private TextMeshProUGUI coins1000PriceText;
    [SerializeField] private TextMeshProUGUI coins2500PriceText;
    
    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    
    [Header("Feedback")]
    [SerializeField] private GameObject purchaseSuccessPanel;
    [SerializeField] private GameObject purchaseFailedPanel;
    [SerializeField] private TextMeshProUGUI successMessageText;
    [SerializeField] private TextMeshProUGUI failedMessageText;
    [SerializeField] private float feedbackDisplayTime = 2f;
    
    [Header("Fallback Prices (when IAP not configured)")]
    [SerializeField] private string fallbackPrice100 = "$0.99";
    [SerializeField] private string fallbackPrice500 = "$4.99";
    [SerializeField] private string fallbackPrice1000 = "$9.99";
    [SerializeField] private string fallbackPrice2500 = "$19.99";
    
    void Start()
    {
        SetupUI();
        SetupEvents();
        
        // Hide shop initially
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
    
    void SetupUI()
    {
        // Setup button listeners
        if (coins100Button != null)
            coins100Button.onClick.AddListener(() => PurchaseCoins100());
        if (coins500Button != null)
            coins500Button.onClick.AddListener(() => PurchaseCoins500());
        if (coins1000Button != null)
            coins1000Button.onClick.AddListener(() => PurchaseCoins1000());
        if (coins2500Button != null)
            coins2500Button.onClick.AddListener(() => PurchaseCoins2500());
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
        
        // Hide feedback panels
        if (purchaseSuccessPanel != null)
            purchaseSuccessPanel.SetActive(false);
        if (purchaseFailedPanel != null)
            purchaseFailedPanel.SetActive(false);
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    
    void SetupEvents()
    {
        // Subscribe to currency changes
        CurrencyManager.OnCoinsChanged += UpdateCoinsDisplay;
        
        // Subscribe to IAP events
        IAPManager.OnStoreInitialized += UpdatePriceLabels;
        IAPManager.OnPurchaseSucceeded += OnPurchaseSuccess;
        IAPManager.OnStorePurchaseFailed += OnPurchaseFailed;
        
        // Update initial display
        if (CurrencyManager.Instance != null)
            UpdateCoinsDisplay(CurrencyManager.Instance.GetCoins());
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        CurrencyManager.OnCoinsChanged -= UpdateCoinsDisplay;
        IAPManager.OnStoreInitialized -= UpdatePriceLabels;
        IAPManager.OnPurchaseSucceeded -= OnPurchaseSuccess;
        IAPManager.OnStorePurchaseFailed -= OnPurchaseFailed;
    }
    
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            UpdatePriceLabels();
            
            // Pause game if needed
            Time.timeScale = 0f;
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            
            // Resume game
            Time.timeScale = 1f;
        }
    }
    
    private void UpdateCoinsDisplay(int coins)
    {
        if (coinsDisplay != null)
            coinsDisplay.text = coins.ToString("#,0");
    }
    
    private void UpdatePriceLabels()
    {
        if (IAPManager.Instance == null)
        {
            // IAP not available, use fallback prices
            SetFallbackPrices();
            return;
        }
        
        // Try to get prices from IAP, but use fallbacks if they're test prices
        string price100 = GetPriceOrFallback("com.yourgame.coins100", fallbackPrice100);
        string price500 = GetPriceOrFallback("com.yourgame.coins500", fallbackPrice500);
        string price1000 = GetPriceOrFallback("com.yourgame.coins1000", fallbackPrice1000);
        string price2500 = GetPriceOrFallback("com.yourgame.coins2500", fallbackPrice2500);
        
        if (coins100PriceText != null) coins100PriceText.text = price100;
        if (coins500PriceText != null) coins500PriceText.text = price500;
        if (coins1000PriceText != null) coins1000PriceText.text = price1000;
        if (coins2500PriceText != null) coins2500PriceText.text = price2500;
    }
    
    private string GetPriceOrFallback(string productId, string fallbackPrice)
    {
        string iapPrice = IAPManager.Instance.GetProductPrice(productId);
        
        // If IAP returns test prices or "N/A", use fallback
        if (iapPrice == "N/A" || iapPrice == "$0.01" || iapPrice == "¥1" || string.IsNullOrEmpty(iapPrice))
        {
            return fallbackPrice;
        }
        
        return iapPrice;
    }
    
    private void SetFallbackPrices()
    {
        if (coins100PriceText != null) coins100PriceText.text = fallbackPrice100;
        if (coins500PriceText != null) coins500PriceText.text = fallbackPrice500;
        if (coins1000PriceText != null) coins1000PriceText.text = fallbackPrice1000;
        if (coins2500PriceText != null) coins2500PriceText.text = fallbackPrice2500;
    }
    
    // Purchase Methods
    private void PurchaseCoins100()
    {
        ShowLoading("Processing purchase...");
        IAPManager.Instance?.BuyCoins100();
    }
    
    private void PurchaseCoins500()
    {
        ShowLoading("Processing purchase...");
        IAPManager.Instance?.BuyCoins500();
    }
    
    private void PurchaseCoins1000()
    {
        ShowLoading("Processing purchase...");
        IAPManager.Instance?.BuyCoins1000();
    }
    
    private void PurchaseCoins2500()
    {
        ShowLoading("Processing purchase...");
        IAPManager.Instance?.BuyCoins2500();
    }
    
    private void ShowLoading(string message)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (loadingText != null)
                loadingText.text = message;
        }
    }
    
    private void HideLoading()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    
    private void OnPurchaseSuccess(string productId)
    {
        HideLoading();
        
        // Determine coin amount based on product ID
        string message = "Purchase Successful!";
        if (productId.Contains("coins100")) message = "100 Coins Added!";
        else if (productId.Contains("coins500")) message = "500 Coins Added!";
        else if (productId.Contains("coins1000")) message = "1000 Coins Added!";
        else if (productId.Contains("coins2500")) message = "2500 Coins Added!";
        
        ShowFeedback(purchaseSuccessPanel, successMessageText, message);
    }
    
    private void OnPurchaseFailed(string productId)
    {
        HideLoading();
        ShowFeedback(purchaseFailedPanel, failedMessageText, "Purchase Failed! Please try again.");
    }
    
    private void ShowFeedback(GameObject panel, TextMeshProUGUI textComponent, string message)
    {
        if (panel != null)
        {
            if (textComponent != null)
                textComponent.text = message;
            
            panel.SetActive(true);
            StartCoroutine(HideFeedbackAfterDelay(panel));
        }
    }
    
    private IEnumerator HideFeedbackAfterDelay(GameObject panel)
    {
        yield return new WaitForSecondsRealtime(feedbackDisplayTime);
        if (panel != null)
            panel.SetActive(false);
    }
} 