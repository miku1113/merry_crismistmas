using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelSelectionManager : MonoBehaviour
{
    [Header("Level Data")]
    public List<LevelData> levels = new List<LevelData>();
    
    [Header("UI References")]
    public LevelCard levelCard;
    public Button leftButton;
    public Button rightButton;
    public Button backButton;
    public TMPro.TextMeshProUGUI coinsText;
    public TMPro.TextMeshProUGUI keysText;
    
    [Header("Purchase Popup")]
    public GameObject purchasePopup;
    public TMPro.TextMeshProUGUI purchaseText;
    public TMPro.TextMeshProUGUI errorText;
    public Button confirmPurchaseButton; // The "Yes" button in the popup
    public CanvasGroup backgroundUIGroup;
    
    private int currentLevelIndex = 0;
    private LevelData levelPendingPurchase;
    
    // Click cooldown to prevent double-clicks
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.3f; // 300ms between clicks
    
    void Start()
    {
        // Validate setup
        if (levels.Count == 0)
        {
            Debug.LogError("No levels assigned to LevelSelectionManager!");
            return;
        }
        
        if (levelCard == null)
        {
            Debug.LogError("LevelCard reference not assigned!");
            return;
        }
        
        // Setup button listeners
        if (leftButton != null)
        {
            leftButton.onClick.AddListener(OnLeftButtonClick);
        }
        
        if (rightButton != null)
        {
            rightButton.onClick.AddListener(OnRightButtonClick);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }
        
        // Hide purchase popup on start
        if (purchasePopup != null)
        {
            purchasePopup.SetActive(false);
        }
        
        // Show first level
        ShowLevel(0);
        UpdateCurrencyUI();
    }
    
    private void UpdateCurrencyUI()
    {
        if (coinsText != null) coinsText.text = "Coins: " + PlayerPrefs.GetInt("TotalCoins", 0);
        if (keysText != null) keysText.text = "Keys: " + PlayerPrefs.GetInt("TotalKeys", 0);
    }
    
    
    public void RequestPurchase(LevelData levelData)
    {
        if (levelData == null) return;
        
        levelPendingPurchase = levelData;
        
        // Disable background interaction
        if (backgroundUIGroup != null)
        {
            backgroundUIGroup.interactable = false;
            backgroundUIGroup.blocksRaycasts = false;
        }

        if (purchasePopup != null)
        {
            purchasePopup.SetActive(true);
            
            // Show purchase question, hide error text
            if (purchaseText != null) purchaseText.gameObject.SetActive(true);
            if (errorText != null) errorText.gameObject.SetActive(false);
            
            // Ensure confirm button is active and text is correct for initial request
            if (confirmPurchaseButton != null) confirmPurchaseButton.gameObject.SetActive(true);
            
            if (purchaseText != null)
            {
                purchaseText.text = $"Buy {levelData.levelName} for {levelData.price} coins?";
            }
        }
    }

    public void ConfirmPurchase()
    {
        if (levelPendingPurchase != null)
        {
            int deficit = UnlockLevel(levelPendingPurchase);
            
            if (deficit == 0)
            {
                // Success! Refresh card display
                if (levelCard != null)
                {
                    levelCard.Initialize(levels[currentLevelIndex], this);
                }
                
                UpdateCurrencyUI();
                
                // Re-enable background interaction
                SetBackgroundInteraction(true);
                
                // Hide popup
                if (purchasePopup != null) purchasePopup.SetActive(false);
                levelPendingPurchase = null;
            }
            else
            {
                // Not enough coins - Show Error
                if (errorText != null)
                {
                    errorText.gameObject.SetActive(true);
                    errorText.text = $"You need {deficit} more.";
                }
            }
        }
    }

    public void CancelPurchase()
    {
        // Re-enable background interaction
        SetBackgroundInteraction(true);

        if (purchasePopup != null) purchasePopup.SetActive(false);
        levelPendingPurchase = null;
    }

    private void SetBackgroundInteraction(bool state)
    {
        if (backgroundUIGroup != null)
        {
            backgroundUIGroup.interactable = state;
            backgroundUIGroup.blocksRaycasts = state;
        }
    }

    private void ShowLevel(int index)
    {
        // Clamp index to valid range
        currentLevelIndex = Mathf.Clamp(index, 0, levels.Count - 1);
        
        // Update card display
        if (levelCard != null && currentLevelIndex < levels.Count)
        {
            levelCard.Initialize(levels[currentLevelIndex], this);
        }
        
        // Update navigation button states
        UpdateNavigationButtons();
    }
    
    private void UpdateNavigationButtons()
    {
        Debug.Log($"[LevelSelectionManager] Updating buttons for level index: {currentLevelIndex} / {levels.Count - 1}");
        
        // Disable left button if on first level
        if (leftButton != null)
        {
            bool shouldEnable = currentLevelIndex > 0;
            leftButton.interactable = shouldEnable;
            
            Debug.Log($"[LevelSelectionManager] Left Button - Index: {currentLevelIndex}, Should Enable: {shouldEnable}, Interactable: {leftButton.interactable}");
            
            // Visual feedback - reduce alpha when disabled
            var leftColors = leftButton.colors;
            leftColors.disabledColor = new Color(1f, 1f, 1f, 0.5f);
            leftButton.colors = leftColors;
        }
        
        // Disable right button if on last level
        if (rightButton != null)
        {
            bool shouldEnable = currentLevelIndex < levels.Count - 1;
            rightButton.interactable = shouldEnable;
            
            Debug.Log($"[LevelSelectionManager] Right Button - Index: {currentLevelIndex}, Should Enable: {shouldEnable}, Interactable: {rightButton.interactable}");
            
            // Visual feedback - reduce alpha when disabled
            var rightColors = rightButton.colors;
            rightColors.disabledColor = new Color(1f, 1f, 1f, 0.5f);
            rightButton.colors = rightColors;
        }
    }
    
    public void OnLeftButtonClick()
    {
        Debug.Log($"[LevelSelectionManager] Left button clicked! Current index: {currentLevelIndex}");
        
        // Check cooldown to prevent double-clicks
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            Debug.LogWarning($"[LevelSelectionManager] Click ignored - cooldown active (waited {Time.time - lastClickTime:F3}s)");
            return;
        }
        
        if (currentLevelIndex > 0)
        {
            lastClickTime = Time.time; // Update last click time
            
            // Play button click sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
            
            Debug.Log($"[LevelSelectionManager] Moving from level {currentLevelIndex} to {currentLevelIndex - 1}");
            ShowLevel(currentLevelIndex - 1);
        }
        else
        {
            Debug.LogWarning($"[LevelSelectionManager] Cannot go left - already at first level (index: {currentLevelIndex})");
        }
    }
    
    public void OnRightButtonClick()
    {
        Debug.Log($"[LevelSelectionManager] Right button clicked! Current index: {currentLevelIndex}");
        
        // Check cooldown to prevent double-clicks
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            Debug.LogWarning($"[LevelSelectionManager] Click ignored - cooldown active (waited {Time.time - lastClickTime:F3}s)");
            return;
        }
        
        if (currentLevelIndex < levels.Count - 1)
        {
            lastClickTime = Time.time; // Update last click time
            
            // Play button click sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
            
            Debug.Log($"[LevelSelectionManager] Moving from level {currentLevelIndex} to {currentLevelIndex + 1}");
            ShowLevel(currentLevelIndex + 1);
        }
        else
        {
            Debug.LogWarning($"[LevelSelectionManager] Cannot go right - already at last level (index: {currentLevelIndex})");
        }
    }
    
    public void OnBackButtonClick()
    {
        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        
        // Return to main menu
        SceneManager.LoadScene("MainMenu");
    }
    
    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("Cannot load null level data!");
            return;
        }
        
        // Final safety check before loading
        if (!IsLevelUnlocked(levelData))
        {
            Debug.LogWarning("Level is locked!");
            return;
        }

        // Store selected level data for GameManager to use
        PlayerPrefs.SetString("SelectedLevel", levelData.name);
        PlayerPrefs.SetFloat("LevelDifficulty", levelData.difficultyMultiplier);
        PlayerPrefs.SetFloat("LevelSpeed", levelData.speedMultiplier);
        PlayerPrefs.SetFloat("LevelSpawnRate", levelData.spawnRateMultiplier);
        PlayerPrefs.Save();
        
        Debug.Log($"Loading level: {levelData.levelName} (Scene: {levelData.sceneName})");
        
        // Load the level scene
        SceneManager.LoadScene(levelData.sceneName);
    }

    public bool IsLevelUnlocked(LevelData levelData)
    {
        if (levelData == null) return false;
        if (levelData.isFree) return true;

        // Check PlayerPrefs for "LevelUnlocked_[AssetName]" (more unique than levelName)
        return PlayerPrefs.GetInt("LevelUnlocked_" + levelData.name, 0) == 1;
    }

    /// <summary>
    /// Attempts to unlock a level. Returns 0 if success, or the amount of coins missing.
    /// </summary>
    public int UnlockLevel(LevelData levelData)
    {
        if (levelData == null) return -1;
        if (IsLevelUnlocked(levelData)) return 0;

        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (totalCoins >= levelData.price)
        {
            // Deduct coins
            totalCoins -= levelData.price;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            
            // Mark as unlocked using Asset Name
            PlayerPrefs.SetInt("LevelUnlocked_" + levelData.name, 1);
            PlayerPrefs.Save();

            Debug.Log($"Level {levelData.levelName} unlocked! Remaining coins: {totalCoins}");
            
            // Play success sound if possible
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
            return 0;
        }
        else
        {
            int missing = levelData.price - totalCoins;
            Debug.LogWarning($"Not enough coins to unlock {levelData.levelName}. Need {missing} more.");
            return missing;
        }
    }
}
