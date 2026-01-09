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

    [Header("Floor Spawning")]
    public GameObject floorPrefab;
    public float floorWidth = 20f;
    public float floorY = -5f;
    public int spawnAheadCount = 3;
    private float lastFloorX = -20f;
    private List<GameObject> activeFloors = new List<GameObject>();

    private int score = 0;
    private int coins = 0;
    private int totalKeys = 0;
    private int continueWithKeyCount = 0; // Increments each time user continues with keys in a single run
    private int highScore = 0;
    private bool isGameOver = false;
    private bool isPaused = false;
    
    [Header("Key System UI")]
    public TextMeshProUGUI keyText;
    public TextMeshProUGUI continueKeyCostText;
    public GameObject continueWithKeysButton;

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
        coins = PlayerPrefs.GetInt("TotalCoins", 0);
        totalKeys = PlayerPrefs.GetInt("TotalKeys", 0);
        continueWithKeyCount = 0; // Reset for new run

        UpdateUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Auto-detect Santa if not assigned
        if (santaTransform == null)
        {
            GameObject player = GameObject.Find("Santa") ?? GameObject.Find("SantaClaus") ?? GameObject.Find("Ravan") ?? GameObject.FindWithTag("Player");
            if (player != null) santaTransform = player.transform;
            else Debug.LogWarning("Player Transform not assigned in GameManager and could not be auto-detected!");
        }

        // Initialize first floors
        if (floorPrefab != null)
        {
            for (int i = 0; i < spawnAheadCount + 2; i++)
            {
                SpawnFloor();
            }
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

        // Backup Miss Detection (Queue-based) - Now also checking for Yagnas
        if (santaTransform != null)
        {
            // Check Chimneys
            if (activeChimneys.Count > 0)
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

            // Check Yagnas (Assuming we might want a separate list or tag-based check)
            // For simplicity, we can also let the Yagna script handle its own miss detection as implemented.
            
            // Handle Procedural Floor Spawning
            if (floorPrefab != null && santaTransform.position.x + (spawnAheadCount * floorWidth) > lastFloorX)
            {
                SpawnFloor();
            }

            // Cleanup floors that are far behind
            if (activeFloors.Count > 0 && activeFloors[0].transform.position.x < santaTransform.position.x - floorWidth * 2)
            {
                GameObject oldFloor = activeFloors[0];
                activeFloors.RemoveAt(0);
                Destroy(oldFloor);
            }
        }
    }

    void SpawnFloor()
    {
        lastFloorX += floorWidth;
        GameObject newFloor = Instantiate(floorPrefab, new Vector3(lastFloorX, floorY, 0), Quaternion.identity);
        activeFloors.Add(newFloor);
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

    public void AddCoin(int amount)
    {
        if (isGameOver) return;
        coins += amount;
        PlayerPrefs.SetInt("TotalCoins", coins); // Save continuously
        UpdateUI();
    }

    public void AddKey(int amount)
    {
        if (isGameOver) return;
        totalKeys += amount;
        PlayerPrefs.SetInt("TotalKeys", totalKeys);
        UpdateUI();
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
        
        // Setup Key Continue Cost
        int requiredKeys = continueWithKeyCount + 1;
        if (continueKeyCostText != null)
        {
            continueKeyCostText.text = $"Use {requiredKeys} {(requiredKeys == 1 ? "Key" : "Keys")}";
        }

        // Show/Hide continue with keys button based on balance
        if (continueWithKeysButton != null)
        {
            continueWithKeysButton.SetActive(totalKeys >= requiredKeys);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void OnContinueWithKeysClick()
    {
        PlayClickSound();
        int requiredKeys = continueWithKeyCount + 1;
        
        if (totalKeys >= requiredKeys)
        {
            totalKeys -= requiredKeys;
            PlayerPrefs.SetInt("TotalKeys", totalKeys);
            continueWithKeyCount++; // Increment cost for next time
            
            ContinueGame();
            UpdateUI();
        }
    }

    public void OnContinueButtonClick()
    {
        PlayClickSound();
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardedAd();
        }
        else
        {
            Debug.LogError("AdsManager Instance not found! Continuing game without ad for testing.");
            ContinueGame();
        }
    }

    public void ContinueGame()
    {
        isGameOver = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        Time.timeScale = 1;
        
        // Find and destroy any chimneys currently on screen
        if (Camera.main != null)
        {
            // 1. Clear Chimneys (Houses)
            // Create a temporary copy to avoid modification errors while iterating
            List<Chimney> toCheck = new List<Chimney>(activeChimneys);
            foreach (Chimney chimney in toCheck)
            {
                if (chimney == null) continue;
                Vector3 screenPoint = Camera.main.WorldToViewportPoint(chimney.transform.position);
                
                // If chimney is inside or slightly outside the viewport, destroy it
                if (screenPoint.x >= -0.2f && screenPoint.x <= 1.2f)
                {
                    Destroy(chimney.gameObject);
                }
            }
            
            // 2. Clear Other Obstacles (Clouds, Trees) identified by OffScreenDestroyer
            OffScreenDestroyer[] obstacles = FindObjectsOfType<OffScreenDestroyer>();
            foreach (OffScreenDestroyer obs in obstacles)
            {
                if (obs == null) continue;
                Vector3 screenPoint = Camera.main.WorldToViewportPoint(obs.transform.position);
                
                // If visible (with buffer), destroy
                if (screenPoint.x >= -0.2f && screenPoint.x <= 1.2f && screenPoint.y >= -0.2f && screenPoint.y <= 1.2f)
                {
                    Destroy(obs.gameObject);
                }
            }

            // 3. Explicitly clear Dark Clouds (identified by PeriodicAnimator)
            // This ensures they are removed even if OffScreenDestroyer is missing
            PeriodicAnimator[] darkClouds = FindObjectsOfType<PeriodicAnimator>();
            foreach (PeriodicAnimator dc in darkClouds)
            {
                if (dc == null) continue;
                Vector3 screenPoint = Camera.main.WorldToViewportPoint(dc.transform.position);
                
                if (screenPoint.x >= -0.2f && screenPoint.x <= 1.2f && screenPoint.y >= -0.2f && screenPoint.y <= 1.2f)
                {
                    Destroy(dc.gameObject);
                }
            }
        }
        
        // Reset Santa's state (re-enable controls, clear shock flag)
        if (santaTransform != null)
        {
            SantaController santaCtrl = santaTransform.GetComponent<SantaController>();
            if (santaCtrl != null)
            {
                santaCtrl.ResetState();
            }
        }

        Debug.Log("Game Continued and visible chimneys cleared!");
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
        if (scoreText != null) scoreText.text = "Score: " + score + "\nCoins: " + coins;
        if (keyText != null) keyText.text = "Keys: " + totalKeys;
        if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
    }
}
