using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip buttonClickSound;

    private const string BGM_MUTE_KEY = "BGMMute";

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
            return;
        }

        LoadSettings();
    }

    void Start()
    {
        if (backgroundMusic != null && bgmSource != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void ToggleBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.mute = !bgmSource.mute;
            PlayerPrefs.SetInt(BGM_MUTE_KEY, bgmSource.mute ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool IsBGMMuted()
    {
        return bgmSource != null && bgmSource.mute;
    }

    private void LoadSettings()
    {
        if (bgmSource != null)
        {
            bool isMuted = PlayerPrefs.GetInt(BGM_MUTE_KEY, 0) == 1;
            bgmSource.mute = isMuted;
        }
    }
}
