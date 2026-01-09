using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCard : MonoBehaviour
{
    [Header("UI References")]
    public Image levelPreviewImage;
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI priceText;
    public Button playButton; // This button handles both unlocking and playing
    public Image buttonIcon; // Optional: Assign a child icon image for better alignment
    public GameObject lockOverlay;
    
    private LevelData currentLevelData;
    private LevelSelectionManager selectionManager;
    
    public void Initialize(LevelData levelData, LevelSelectionManager manager)
    {
        currentLevelData = levelData;
        selectionManager = manager;
        
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (currentLevelData == null) return;
        
        // Update level name
        if (levelNameText != null)
        {
            levelNameText.text = currentLevelData.levelName;
        }
        
        // Update preview image
        if (levelPreviewImage != null && currentLevelData.levelPreviewImage != null)
        {
            levelPreviewImage.sprite = currentLevelData.levelPreviewImage;
        }
        
        // Check unlock status
        bool isUnlocked = currentLevelData.isFree || (selectionManager != null && selectionManager.IsLevelUnlocked(currentLevelData));
        
        // Update price text visibility
        if (priceText != null)
        {
            priceText.gameObject.SetActive(!isUnlocked);
            priceText.text = "Price: " + currentLevelData.price;
        }

        // Handle lock overlay
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(!isUnlocked); // Keep overlay if locked, or hide if bought
        }
        
        // Update Button Appearance (Sprite swapping)
        if (playButton != null)
        {
            // Use dedicated buttonIcon if assigned, otherwise fallback to button's own Image
            Image spriteTarget = (buttonIcon != null) ? buttonIcon : playButton.GetComponent<Image>();
            
            if (spriteTarget != null)
            {
                // Swap sprite based on unlock status, using sprites from LevelData
                spriteTarget.sprite = isUnlocked ? currentLevelData.playSprite : currentLevelData.buySprite;
            }
            
            // Button is always active, but logic changes in OnPlayButtonClick
            playButton.gameObject.SetActive(true);
            playButton.interactable = true;
        }
    }
    
    public void OnPlayButtonClick()
    {
        if (currentLevelData == null) return;

        bool isUnlocked = currentLevelData.isFree || (selectionManager != null && selectionManager.IsLevelUnlocked(currentLevelData));

        if (!isUnlocked)
        {
            // Request Purchase via Popup
            if (selectionManager != null)
            {
                selectionManager.RequestPurchase(currentLevelData);
            }
        }
        else
        {
            // Try to Load Level
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
            
            if (selectionManager != null)
            {
                selectionManager.LoadLevel(currentLevelData);
            }
        }
    }
}
