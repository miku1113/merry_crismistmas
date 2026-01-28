using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdsManager Instance;

    [Header("Setup")]
    [SerializeField] string _androidGameId = "6013200";
#if UNITY_IOS
    [SerializeField] string _iOSGameId = "6013201";
#endif
    [SerializeField] bool _testMode = false;

    [Header("Rewarded Ad Unit IDs")]
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
#if UNITY_IOS
    [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
#endif

    private string _gameId;
    private string _adUnitId;
    private bool _isInitializing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializeAds();
    }

    public void InitializeAds()
    {
#if UNITY_IOS
        _gameId = _iOSGameId;
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
        _adUnitId = _androidAdUnitId;
#elif UNITY_EDITOR
        _gameId = _androidGameId; // Default to Android for Editor
        _adUnitId = _androidAdUnitId;
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported && !_isInitializing)
        {
            _isInitializing = true;
            
            // Set child-directed metadata for Families Policy compliance
            MetaData privacyMetaData = new MetaData("privacy");
            privacyMetaData.Set("mode", "child-directed");
            Advertisement.SetMetaData(privacyMetaData);

            Advertisement.Initialize(_gameId, _testMode, this);
        }
        else if (Advertisement.isInitialized)
        {
            // If already initialized (e.g., from a previous scene), load ad immediately
            Debug.Log("Unity Ads already initialized. Loading Ad...");
            LoadAd();
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void LoadAd()
    {
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
    }

    public void ShowRewardedAd()
    {
        Debug.Log("Showing Ad: " + _adUnitId);
        Advertisement.Show(_adUnitId, this);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            // Reward the user
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ContinueGame();
            }
            
            // Load another ad
            LoadAd();
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}
