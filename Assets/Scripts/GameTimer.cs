using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Display")]
    [SerializeField] private TextMeshProUGUI timerText; // Assign a UI Text component to display timer
    
    [Header("Timer Settings")]
    public bool startAutomatically = true; // Whether to start timer when scene loads
    
    private float startTime;
    private bool isRunning = false;
    
    // Static instance for easy access from other scripts
    public static GameTimer Instance { get; private set; }

    // Auto-create GameTimer if none exists
    public static GameTimer GetOrCreateInstance()
    {
        if (Instance == null)
        {
            // Look for existing GameTimer in scene
            Instance = FindFirstObjectByType<GameTimer>();
            
            if (Instance == null)
            {
                // Create new GameTimer GameObject
                GameObject gameTimerGO = new GameObject("GameTimer");
                Instance = gameTimerGO.AddComponent<GameTimer>();
                DontDestroyOnLoad(gameTimerGO);
            }
        }
        return Instance;
    }
    
    void Awake()
    {
        // Singleton pattern - only one GameTimer should exist
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
        // Initialize currency manager if not already done
        if (CurrencyManager.Instance == null)
        {
            GameObject currencyManagerGO = new GameObject("CurrencyManager");
            currencyManagerGO.AddComponent<CurrencyManager>();
        }
        
        // Initialize IAP manager if not already done
        if (IAPManager.Instance == null)
        {
            GameObject iapManagerGO = new GameObject("IAPManager");
            iapManagerGO.AddComponent<IAPManager>();
        }
        
        if (startAutomatically)
        {
            StartTimer();
        }
    }
    
    void Update()
    {
        if (isRunning)
        {
            UpdateTimerDisplay();
        }
    }
    
    public void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
        Debug.Log("Game Timer Started!");
    }
    
    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("Game Timer Stopped!");
    }
    
    public void ResetTimer()
    {
        startTime = Time.time;
        Debug.Log("Game Timer Reset!");
    }
    
    public float GetElapsedTime()
    {
        if (!isRunning) return 0f;
        return Time.time - startTime;
    }
    
    public int GetCurrentMinute()
    {
        float elapsedTime = GetElapsedTime();
        return Mathf.FloorToInt(elapsedTime / 60f); // Return 0-based minutes (0, 1, 2, 3...)
    }
    
    public float GetElapsedSeconds()
    {
        float elapsedTime = GetElapsedTime();
        return elapsedTime % 60f; // Seconds within the current minute
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            float elapsedTime = GetElapsedTime();
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    // Public method to get formatted time string
    public string GetFormattedTime()
    {
        float elapsedTime = GetElapsedTime();
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    public bool IsRunning()
    {
        return isRunning;
    }
} 