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
    private const string SFX_MUTE_KEY = "SFXMute";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

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

    public void ToggleSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.mute = !sfxSource.mute;
            PlayerPrefs.SetInt(SFX_MUTE_KEY, sfxSource.mute ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = volume;
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
            PlayerPrefs.Save();
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
            PlayerPrefs.Save();
        }
    }

    public bool IsBGMMuted()
    {
        return bgmSource != null && bgmSource.mute;
    }

    public bool IsSFXMuted()
    {
        return sfxSource != null && sfxSource.mute;
    }

    public float GetBGMVolume()
    {
        return bgmSource != null ? bgmSource.volume : 1f;
    }

    public float GetSFXVolume()
    {
        return sfxSource != null ? sfxSource.volume : 1f;
    }

    private void LoadSettings()
    {
        if (bgmSource != null)
        {
            bgmSource.mute = PlayerPrefs.GetInt(BGM_MUTE_KEY, 0) == 1;
            bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        }
        
        if (sfxSource != null)
        {
            sfxSource.mute = PlayerPrefs.GetInt(SFX_MUTE_KEY, 0) == 1;
            sfxSource.volume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        }
    }
}
