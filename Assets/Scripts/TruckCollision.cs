using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip crashSound;
    
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    
    private AudioSource audioSource;
    private bool hasCrashed = false;
    private int crashCount = 0;
    private TrafficSpawner trafficSpawner;
    private GameObject explosionFallback;
 
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
        // Check if we hit a vehicle (using your "Vehicle" tag)
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
        
        // Play crash sound
        if (crashSound != null && audioSource != null)
            audioSource.PlayOneShot(crashSound);
        AudioManager.Instance?.PlayCrash();

        // Spawn explosion at midpoint between truck and hit vehicle
        GameObject effectPrefab = explosionEffect != null ? explosionEffect : GetOrCreateExplosionFallback();
        if (effectPrefab != null)
        {
            Vector3 explosionPos = (transform.position + hitVehicle.transform.position) / 2f;
            GameObject instance = Instantiate(effectPrefab, explosionPos, Quaternion.identity);
            instance.SetActive(true); // needed when using the inactive fallback template
        }
        
        // Remove the hit vehicle from traffic system
        if (trafficSpawner != null)
        {
            trafficSpawner.RemoveVehicle(hitVehicle);
        }
        
        // Destroy the vehicle we hit
        Destroy(hitVehicle);
        
        // Disable player control
        TruckPlayerController playerController = GetComponent<TruckPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Stop score tracking
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
            // Just respawn after delay
            Invoke("Respawn", respawnDelay);
        }
    }
 
    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Pause the game
        Time.timeScale = 0f;
        
        Debug.Log("GAME OVER! Press R to restart");
    }
 
    void Respawn()
    {
        // Reset truck position to center lane
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;
        
        // Re-enable player control
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
        // Press R to restart after game over - FIXED: Uses New Input System
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
 
    // Public methods for UI buttons
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
 
    // Getters
    public int GetCrashCount() => crashCount;
    public bool HasCrashed() => hasCrashed;

    // ── Game Over panel ───────────────────────────────────────────────────────

    void EnsureGameOverPanel()
    {
        if (gameOverPanel != null) return;

        // EventSystem is required for button clicks
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Root canvas
        GameObject canvasGO = new GameObject("GameOver_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Full-screen dark panel
        gameOverPanel = new GameObject("GameOver_Panel");
        gameOverPanel.transform.SetParent(canvas.transform, false);
        StretchToFill(gameOverPanel.AddComponent<RectTransform>());
        Image bg = gameOverPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.78f);

        // "GAME OVER" title
        AddTMPLabel(gameOverPanel.transform, "Title", "GAME OVER",
            new Vector2(0.5f, 0.62f), new Vector2(700f, 130f),
            80f, new Color(0.95f, 0.15f, 0.15f));

        // Restart button
        AddButton(gameOverPanel.transform, "RestartBtn", "RESTART",
            new Vector2(0.5f, 0.42f), new Vector2(320f, 72f),
            new Color(0.15f, 0.75f, 0.15f), OnRestartButton);

        // Quit button
        AddButton(gameOverPanel.transform, "QuitBtn", "QUIT",
            new Vector2(0.5f, 0.28f), new Vector2(320f, 72f),
            new Color(0.75f, 0.15f, 0.15f), OnQuitButton);
    }

    static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
    }

    static void AddTMPLabel(Transform parent, string name, string text,
        Vector2 anchor, Vector2 size, float fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = color;
    }

    void AddButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 size, Color bgColor,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        AddTMPLabel(go.transform, "Label", label,
            new Vector2(0.5f, 0.5f), size * 0.9f, 36f, Color.white);
    }

    // ── Explosion fallback ────────────────────────────────────────────────────

    GameObject GetOrCreateExplosionFallback()
    {
        if (explosionFallback != null) return explosionFallback;

        explosionFallback = new GameObject("ExplosionFallback");
        explosionFallback.SetActive(false);

        ParticleSystem ps = explosionFallback.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(4f, 12f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.25f, 0.7f);
        main.startColor    = new ParticleSystem.MinMaxGradient(Color.red, new Color(1f, 0.6f, 0f));
        main.maxParticles  = 60;
        main.stopAction    = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 1.2f;

        return explosionFallback;
    }
}