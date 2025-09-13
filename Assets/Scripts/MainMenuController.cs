using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button storeButton;
    [SerializeField] private Button quitButton;
    
    [Header("References")]
    [SerializeField] private SceneController sceneController;
    [SerializeField] private ShopManager shopManager;
    
    void Start()
    {
        SetupButtons();
    }
    
    void SetupButtons()
    {
        // Connect existing buttons
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);
        if (storeButton != null)
            storeButton.onClick.AddListener(OpenStore);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    public void StartGame()
    {
        if (sceneController != null)
            sceneController.LoadSampleScene();
        else
            Debug.LogError("SceneController not assigned!");
    }
    
    public void OpenOptions()
    {
        // Add your options functionality here
        Debug.Log("Options clicked - implement options menu");
    }
    
    public void OpenStore()
    {
        if (shopManager != null)
            shopManager.OpenShop();
        else
            Debug.LogError("ShopManager not assigned!");
    }
    
    public void QuitGame()
    {
        if (sceneController != null)
            sceneController.QuitGame();
        else
            Debug.LogError("SceneController not assigned!");
    }
} 