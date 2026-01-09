using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsPanel;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI keysText;

    [Header("Settings Sliders")]
    public UnityEngine.UI.Slider bgmSlider;
    public UnityEngine.UI.Slider sfxSlider;
    public UnityEngine.UI.Slider sensitivitySlider;

    void Start()
    {
        // Hide settings panel on start
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Update high score and coins display
        UpdateUI();
    }

    void UpdateUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0);
        }
        if (coinsText != null)
        {
            coinsText.text = "Coins: " + PlayerPrefs.GetInt("TotalCoins", 0);
        }
        if (keysText != null)
        {
            keysText.text = "Keys: " + PlayerPrefs.GetInt("TotalKeys", 0);
        }
    }

    // Button Functions
    public void StartGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        // Load Level Selection scene instead of directly loading gameplay
        SceneManager.LoadScene("LevelSelection");
    }

    public void OpenSettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            LoadSettings();
        }
    }

    public void CloseSettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        Application.Quit();
    }

    // Settings Functions
    void LoadSettings()
    {
        if (bgmSlider != null)
        {
            // Load saved value
            bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
            // Add listener to ensure it triggers callback
            bgmSlider.onValueChanged.RemoveAllListeners(); // Clear any duplicates
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
            sensitivitySlider.onValueChanged.RemoveAllListeners();
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    public void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        PlayerPrefs.Save();
    }
}
