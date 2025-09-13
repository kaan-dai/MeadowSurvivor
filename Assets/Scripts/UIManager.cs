using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject levelUpPopup;
    [SerializeField] private Button[] upgradeButtons; // Array of 3 upgrade buttons
    [SerializeField] private TextMeshProUGUI[] buttonTexts; // Array of 3 text components for the buttons
    
    [Header("Button Text Settings")]
    [SerializeField] private float buttonTextSize = 36f; // Default text size
    [SerializeField] private Color weaponNameColor = Color.yellow; // Color for weapon name
    [SerializeField] private Color damageTextColor = Color.white; // Color for damage text
    
    private Button continueButton;
    private Player player;
    private Dictionary<Button, WeaponData.WeaponType> buttonWeaponMap = new Dictionary<Button, WeaponData.WeaponType>();

    private void Start()
    {
        // Get reference to the player
        player = FindFirstObjectByType<Player>();
        if (player == null)
            Debug.LogWarning("Player not found in scene!");

        // Hide popup at start
        if (levelUpPopup != null)
        {
            levelUpPopup.SetActive(false);
            
            // Get the continue button from the popup
            continueButton = levelUpPopup.GetComponentInChildren<Button>();
            if (continueButton != null)
                continueButton.onClick.AddListener(CloseLevelUpPopup);
            else
                Debug.LogWarning("Continue button not found in level up popup!");

            // Setup upgrade buttons
            if (upgradeButtons != null && upgradeButtons.Length == 3 && buttonTexts != null && buttonTexts.Length == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    int buttonIndex = i; // Needed for closure
                    if (upgradeButtons[i] != null)
                    {
                        upgradeButtons[i].onClick.AddListener(() => OnUpgradeButtonClicked(buttonIndex));
                        
                        // Configure button text
                        if (buttonTexts[i] != null)
                        {
                            buttonTexts[i].fontSize = buttonTextSize;
                            buttonTexts[i].alignment = TextAlignmentOptions.Center;
                            buttonTexts[i].textWrappingMode = TextWrappingModes.Normal;
                            buttonTexts[i].margin = new Vector4(10, 10, 10, 10); // Add some padding
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Upgrade buttons or texts not properly assigned!");
            }
        }
    }

    public void ShowLevelUpPopup()
    {
        if (levelUpPopup != null && player != null)
        {
            levelUpPopup.SetActive(true);
            Time.timeScale = 0f;

            // Clear previous weapon mappings
            buttonWeaponMap.Clear();

            // Get random weapon options
            List<WeaponData.WeaponType> upgradeOptions = player.GetRandomUpgradeOptions(3);

            // Update button texts and store weapon types
            for (int i = 0; i < upgradeOptions.Count && i < 3; i++)
            {
                WeaponData.WeaponType weaponType = upgradeOptions[i];
                string buttonText = GetUpgradeButtonText(weaponType);
                
                if (buttonTexts[i] != null)
                {
                    buttonTexts[i].text = buttonText;
                    // Force text update and resize if needed
                    buttonTexts[i].ForceMeshUpdate();
                }
                buttonWeaponMap[upgradeButtons[i]] = weaponType;
            }
        }
    }

    private string GetUpgradeButtonText(WeaponData.WeaponType weaponType)
    {
        string weaponName = FormatWeaponName(weaponType.ToString());
        return $"<color=#{ColorUtility.ToHtmlStringRGB(weaponNameColor)}>{weaponName}</color>\n<color=#{ColorUtility.ToHtmlStringRGB(damageTextColor)}>+10% Damage</color>";
    }

    private string FormatWeaponName(string weaponType)
    {
        // Add spaces before capital letters and trim any extra spaces
        string formatted = System.Text.RegularExpressions.Regex.Replace(weaponType, "([a-z])([A-Z])", "$1 $2");
        return formatted.Trim();
    }

    private void OnUpgradeButtonClicked(int buttonIndex)
    {
        if (player != null && buttonIndex < upgradeButtons.Length)
        {
            // Disable all upgrade buttons immediately to prevent multiple clicks
            foreach (Button button in upgradeButtons)
            {
                if (button != null)
                {
                    button.interactable = false;
                }
            }

            Button clickedButton = upgradeButtons[buttonIndex];
            if (buttonWeaponMap.TryGetValue(clickedButton, out WeaponData.WeaponType weaponType))
            {
                Debug.Log($"Upgrading weapon: {weaponType}");
                player.UpgradeWeapon(weaponType);
            }

            // Close the popup immediately
            CloseLevelUpPopup();
        }
    }

    private void CloseLevelUpPopup()
    {
        if (levelUpPopup != null)
        {
            // Re-enable all upgrade buttons for next time
            foreach (Button button in upgradeButtons)
            {
                if (button != null)
                {
                    button.interactable = true;
                }
            }

            levelUpPopup.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
