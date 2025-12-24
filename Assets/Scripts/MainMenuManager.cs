using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI highScoreText;
    public GameObject settingsPanel;

    void Start()
    {
        // Load High Score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore;
        }

        // Ensure settings panel is closed
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        PlayClickSound();
        // Assuming your gameplay scene is named "SampleScene"
        // Update this name if your scene name is different
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenSettings()
    {
        PlayClickSound();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
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

    // Volume Control for BGM
    public void ToggleVolume()
    {
        PlayClickSound();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleBGM();
        }
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
