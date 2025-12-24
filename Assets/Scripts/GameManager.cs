using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public Transform santaTransform;

    private int score = 0;
    private int highScore = 0;
    private bool isGameOver = false;
    private bool isPaused = false;

    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;

    // List to track all active chimneys in order of appearance
    private List<Chimney> activeChimneys = new List<Chimney>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Auto-detect Santa if not assigned
        if (santaTransform == null)
        {
            GameObject santa = GameObject.Find("Santa") ?? GameObject.Find("SantaClaus") ?? GameObject.FindWithTag("Player");
            if (santa != null) santaTransform = santa.transform;
            else Debug.LogWarning("Santa Transform not assigned in GameManager and could not be auto-detected!");
        }
    }

    void Update()
    {
        if (isGameOver) return;

        // Check for Pause Input
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (isPaused) return;

        // Backup Miss Detection (Queue-based)
        if (santaTransform != null && activeChimneys.Count > 0)
        {
            Chimney oldest = activeChimneys[0];
            if (oldest != null && oldest.transform.position.x < santaTransform.position.x - 15f)
            {
                if (oldest.IsActive())
                {
                    Debug.Log("Game Manager detected Missed House (Backup Check)");
                    GameOver();
                }
                else
                {
                    activeChimneys.RemoveAt(0);
                }
            }
        }
    }

    public void RegisterChimney(Chimney chimney)
    {
        activeChimneys.Add(chimney);
    }

    public void UnregisterChimney(Chimney chimney)
    {
        if (activeChimneys.Contains(chimney))
            activeChimneys.Remove(chimney);
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
        UpdateUI();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        PlayClickSound();
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        PlayClickSound();
        isPaused = !isPaused;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
        
        Time.timeScale = isPaused ? 0 : 1;
    }

    public void ResumeGame()
    {
        PlayClickSound();
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void GoToMainMenu()
    {
        PlayClickSound();
        Time.timeScale = 1;
        // Assuming your main menu scene is named "MainMenu"
        // You might need to change this if your scene name is different
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
    }
}
