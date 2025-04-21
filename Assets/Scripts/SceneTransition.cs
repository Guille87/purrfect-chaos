using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    private static SceneTransition instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void Start()
    {
        // Al iniciar la escena, hacer un fade-in
        if (fadePanel != null)
            StartCoroutine(FadeIn());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 1f;
            StartCoroutine(FadeIn());
        }
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeIn()
    {
        fadePanel.blocksRaycasts = true;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            fadePanel.alpha = 1f - (timer / fadeDuration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        Time.timeScale = 0f;

        fadePanel.blocksRaycasts = true;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            fadePanel.alpha = timer / fadeDuration;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        fadePanel.alpha = 1f;
        yield return null;

        SceneManager.LoadScene(sceneName);
    }
}
