using UnityEngine;
#if UNITY_ANDROID
// using GooglePlayGames;
// using GooglePlayGames.BasicApi;
#endif
using UnityEngine.SocialPlatforms;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance;

    public bool isAuthenticated = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Auto-login on start
        AuthenticateUser();
    }

    public void AuthenticateUser()
    {
#if UNITY_ANDROID
        // Uncomment when Google Play Games Plugin is installed
        /*
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .RequestServerAuthCode(false)
            .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        */
#endif

        Social.localUser.Authenticate((bool success) =>
        {
            isAuthenticated = success;
            if (success)
            {
                Debug.Log("Cloud Login Successful!");
                // Load data on successful login
                LoadFromCloud(); 
            }
            else
            {
                Debug.LogWarning("Cloud Login Failed.");
            }
        });
    }

    public void ReportScore(int score)
    {
        if (!isAuthenticated) return;

        // Post score to Leaderboard
        Social.ReportScore(score, "YOUR_LEADERBOARD_ID", (bool success) =>
        {
            if (success) Debug.Log("Score posted to Cloud Leaderboard");
        });
    }

    public void SaveGameData(int highScore, int coins, int keys)
    {
        if (!isAuthenticated) return;

        // Implementation differs by platform.
        // For simple data, Unity's Cloud Save or a custom implementation using Saved Games API (Android) / iCloud (iOS) is needed.
        // Below is a placeholder for Saved Games logic.
        
        Debug.Log($"[CloudSave] Saving Data: Score={highScore}, Coins={coins}, Keys={keys}");

        // Example flow for Android Saved Games:
        // OpenSavedGame("MySaveSlot", dataBytes, ...);
    }

    public void LoadFromCloud()
    {
        if (!isAuthenticated) return;

        Debug.Log("[CloudSave] Requesting Data Load...");
        // Example flow:
        // OpenSavedGame("MySaveSlot", ...);
        // OnSuccess: Parse bytes -> Update PlayerPrefs -> GameManager.Instance.UpdateUI();
    }
}
