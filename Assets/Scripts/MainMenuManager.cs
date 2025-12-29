using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI coinsText; 
    public GameObject settingsPanel;

    [Header("Settings UI")]
    public UnityEngine.UI.Slider bgmVolumeSlider;
    public UnityEngine.UI.Slider sfxVolumeSlider;
    public UnityEngine.UI.Slider sensitivitySlider;
    public UnityEngine.UI.Toggle bgmMuteToggle;
    public UnityEngine.UI.Toggle sfxMuteToggle;
    public UnityEngine.UI.Toggle snowfallToggle;
    public TMP_Dropdown difficultyDropdown;

    void Start()
    {
        UpdateMainMenuUI();

        // Ensure settings panel is closed
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        LoadUISettings();
    }

    void UpdateMainMenuUI()
    {
        // Load High Score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore;
        }

        // Load Total Coins
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (coinsText != null)
        {
            coinsText.text = "Coins: " + totalCoins;
        }
    }

    void LoadUISettings()
    {
        if (AudioManager.Instance != null)
        {
            if (bgmVolumeSlider != null) bgmVolumeSlider.value = AudioManager.Instance.GetBGMVolume();
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
            if (bgmMuteToggle != null) bgmMuteToggle.isOn = !AudioManager.Instance.IsBGMMuted();
            if (sfxMuteToggle != null) sfxMuteToggle.isOn = !AudioManager.Instance.IsSFXMuted();
        }

        if (sensitivitySlider != null) sensitivitySlider.value = PlayerPrefs.GetFloat("JoystickSensitivity", 1f);
        if (snowfallToggle != null) snowfallToggle.isOn = PlayerPrefs.GetInt("SnowfallEnabled", 1) == 1;
        if (difficultyDropdown != null) difficultyDropdown.value = PlayerPrefs.GetInt("GameDifficulty", 1);
    }

    public void PlayGame()
    {
        PlayClickSound();
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenSettings()
    {
        PlayClickSound();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            LoadUISettings(); // Refresh UI when opening
        }
    }

    public void CloseSettings()
    {
        PlayClickSound();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    // --- Settings Change Handlers ---

    public void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
    }

    public void OnBGMMuteToggled(bool isOn)
    {
        // Our Toggle logic in AudioManager is a simple flip, 
        // so we check if current state matches desired 'isOn'
        if (AudioManager.Instance != null && AudioManager.Instance.IsBGMMuted() == isOn)
        {
            AudioManager.Instance.ToggleBGM();
        }
    }

    public void OnSFXMuteToggled(bool isOn)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.IsSFXMuted() == isOn)
        {
            AudioManager.Instance.ToggleSFX();
        }
    }

    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("JoystickSensitivity", value);
        PlayerPrefs.Save();
    }

    public void OnDifficultyChanged(int index)
    {
        PlayerPrefs.SetInt("GameDifficulty", index);
        PlayerPrefs.Save();
    }

    public void OnSnowfallToggled(bool isOn)
    {
        PlayerPrefs.SetInt("SnowfallEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ResetGameData()
    {
        PlayClickSound();
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.DeleteKey("TotalCoins");
        PlayerPrefs.Save();
        UpdateMainMenuUI();
        Debug.Log("Game Data Reset!");
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
