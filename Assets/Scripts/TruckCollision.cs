using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Collision detection - hit a vehicle = game over
/// Works with "Vehicle" tag
/// FIXED: Uses New Input System (Keyboard.current) instead of Input.GetKeyDown
/// </summary>
public class TruckCollision : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private bool enableGameOver = true;
    [SerializeField] private float respawnDelay = 2f;

    [Header("Visual Feedback")]
    [SerializeField] private AudioClip crashSound;

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

    private AudioSource audioSource;
    private bool hasCrashed = false;
    private int crashCount = 0;
    private TrafficSpawner trafficSpawner;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        EnsureGameOverPanel();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        trafficSpawner = FindFirstObjectByType<TrafficSpawner>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            if (!hasCrashed)
            {
                Crash(collision.gameObject);
            }
        }
    }

    void Crash(GameObject hitVehicle)
    {
        hasCrashed = true;
        crashCount++;

        Debug.Log($"CRASH! You hit {hitVehicle.name}! Total crashes: {crashCount}");

        if (crashSound != null && audioSource != null)
            audioSource.PlayOneShot(crashSound);
        AudioManager.Instance?.PlayCrash();

        var exp = new GameObject("Explosion");
        exp.transform.position = transform.position + Vector3.up;
        exp.AddComponent<TruckExplosionEffect>();

        if (trafficSpawner != null)
        {
            trafficSpawner.RemoveVehicle(hitVehicle);
        }

        Destroy(hitVehicle);

        TruckPlayerController playerController = GetComponent<TruckPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        ScoreTracker scoreTracker = GetComponent<ScoreTracker>();
        if (scoreTracker != null)
        {
            scoreTracker.StopTracking();
        }

        if (enableGameOver)
        {
            ShowGameOver();
        }
        else
        {
            Invoke("Respawn", respawnDelay);
        }
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Debug.Log("GAME OVER! Press R to restart");
    }

    void Respawn()
    {
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;

        TruckPlayerController playerController = GetComponent<TruckPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        hasCrashed = false;
        Debug.Log("Respawned! Be more careful!");
    }

    void Update()
    {
        if (hasCrashed)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RestartGame();
            }
        }
    }

    public void RestartGame()
    {
        MainMenuUI.SkipOnNextLoad = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnRestartButton()
    {
        RestartGame();
    }

    public void OnQuitButton()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public int GetCrashCount() => crashCount;
    public bool HasCrashed() => hasCrashed;

    // ── Game Over panel ───────────────────────────────────────────────────────

    void EnsureGameOverPanel()
    {
        if (gameOverPanel != null) return;

        var canvas = UITheme.BuildCanvas("GameOver_Canvas", 50);

        gameOverPanel = UITheme.BuildPanel(canvas.transform,
            new Color(0.10f, 0.04f, 0.04f, 0.95f));

        UITheme.AddTitleBadge(gameOverPanel.transform,
            "GAME OVER", "You didn't make it, trucker.",
            new Vector2(0.5f, 0.73f), UITheme.Red, UITheme.Red);

        UITheme.AddImageButton(gameOverPanel.transform, "RestartBtn",
            "UI/Buttons/btn_restart",
            new Vector2(0.5f, 0.42f), new Vector2(540f, 180f), OnRestartButton);

        UITheme.AddImageButton(gameOverPanel.transform, "QuitBtn",
            "UI/Buttons/btn_quit",
            new Vector2(0.5f, 0.25f), new Vector2(540f, 180f), OnQuitButton);
    }
}
