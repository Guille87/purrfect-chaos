using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;

            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

            musicSource.volume = musicVolume;
            sfxSource.volume = sfxVolume;
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
            instance.sfxSource.volume = volume;
    }

    public static void SetMusicVolume(float volume)
    {
        if (instance != null)
            instance.musicSource.volume = volume;
    }
}
