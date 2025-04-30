using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioSource uiSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            uiSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            uiSource.ignoreListenerPause = true;

            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

            musicSource.volume = musicVolume;
            sfxSource.volume = sfxVolume;
            uiSource.volume = sfxVolume; // Usar el mismo volumen que SFX para UI
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (instance != null && clip != null)
        {
            instance.sfxSource.PlayOneShot(clip, volume);
        }
    }

    public static void PlayUISound(AudioClip clip, float volume = 1f)
    {
        if (instance != null && clip != null)
        {
            instance.uiSource.PlayOneShot(clip, volume);
        }
    }

    public static void PlayMusic(AudioClip clip, float volume = 0.5f)
    {
        Debug.Log($"Reproduciendo música: {clip.name} a volumen: {volume}");
        if (instance != null && clip != null)
        {
            instance.musicSource.clip = clip;
            instance.musicSource.volume = volume;
            instance.musicSource.Play();
        }
        else
        {
            Debug.LogWarning("No se pudo reproducir música: instancia o clip nulo");
        }
    }

    public static void SetSFXVolume(float volume)
    {
        if (instance != null)
        {
            instance.sfxSource.volume = volume;
            instance.uiSource.volume = volume; // Sincronizar volumen de UI con SFX
        }
    }

    public static void SetMusicVolume(float volume)
    {
        if (instance != null)
            instance.musicSource.volume = volume;
    }

    public static void PauseMusic()
    {
        if (instance != null && instance.musicSource.isPlaying)
        {
            instance.musicSource.Pause();
        }
    }

    public static void ResumeMusic()
    {
        if (instance != null && instance.musicSource.clip != null && !instance.musicSource.isPlaying)
        {
            instance.musicSource.UnPause();
        }
    }

    public static void PlaySoundWithPitch(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (instance != null && clip != null)
        {
            // Crear un AudioSource temporal
            AudioSource tempSource = instance.gameObject.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = volume;
            tempSource.pitch = pitch;
            tempSource.Play();

            // Destruir el AudioSource cuando termine
            Destroy(tempSource, clip.length / pitch);
        }
    }
}
