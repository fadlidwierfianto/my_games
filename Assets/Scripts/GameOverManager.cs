using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Animation Settings")]
    public float fadeInDuration = 1f;
    public float delayBeforeShow = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gameOverSound;

    private bool gameOverTriggered = false;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Setup canvas group for fade animation
        if (gameOverPanel != null)
        {
            canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }
        }

        // Setup button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Hide game over panel initially
        HideGameOverPanel();
    }

    private void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }
    }

    public void TriggerGameOver()
    {
        if (gameOverTriggered)
            return;

        gameOverTriggered = true;

        // Pause the game
        Time.timeScale = 0f;

        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        // Show game over UI with delay and animation
        StartCoroutine(ShowGameOverWithDelay());
    }

    private IEnumerator ShowGameOverWithDelay()
    {
        // Wait for specified delay (real time, not affected by timeScale)
        yield return new WaitForSecondsRealtime(delayBeforeShow);

        // Show the panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Fade in animation
            if (canvasGroup != null)
            {
                yield return StartCoroutine(FadeIn());
            }
        }

        // Enable cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        // Reset time scale
        Time.timeScale = 1f;

        // Load main menu scene (ganti "MainMenu" dengan nama scene menu utama Anda)
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        // Reset time scale
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Method untuk reset game over state (jika diperlukan)
    public void ResetGameOver()
    {
        gameOverTriggered = false;
        Time.timeScale = 1f;
        HideGameOverPanel();
    }

    // Property untuk mengecek apakah game over sudah triggered
    public bool IsGameOverTriggered
    {
        get { return gameOverTriggered; }
    }
}
