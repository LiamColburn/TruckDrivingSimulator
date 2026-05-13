using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool isPaused;
    private TruckCollision truckCollision;

    void Start()
    {
        EnsurePausePanel();
        truckCollision = FindFirstObjectByType<TruckCollision>();
    }

    void Update()
    {
        bool escPressed = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape)) escPressed = true;
#endif
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) escPressed = true;
#endif
        if (!escPressed) return;

        if (MainMenuUI.IsShowing) return;
        if (truckCollision != null && truckCollision.HasCrashed()) return;

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OnResumeButton()  => ResumeGame();

    public void OnRestartButton()
    {
        MainMenuUI.SkipOnNextLoad = true;
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ── UI builder ────────────────────────────────────────────────────────────

    void EnsurePausePanel()
    {
        if (pausePanel != null) return;

        // Sorting order 45: above HUD (10), below Game Over (50)
        var canvas = UITheme.BuildCanvas("Pause_Canvas", 45);

        pausePanel = UITheme.BuildPanel(canvas.transform,
            new Color(0.05f, 0.05f, 0.08f, 0.88f));

        // Title badge — white text, gold bars
        UITheme.AddTitleBadge(pausePanel.transform,
            "PAUSED", "Taking a rest stop?",
            new Vector2(0.5f, 0.73f), UITheme.OffWhite);

        UITheme.AddImageButton(pausePanel.transform, "ResumeBtn",
            "UI/Buttons/btn_resume",
            new Vector2(0.5f, 0.58f), new Vector2(540f, 180f), OnResumeButton);

        UITheme.AddImageButton(pausePanel.transform, "RestartBtn",
            "UI/Buttons/btn_restart",
            new Vector2(0.5f, 0.43f), new Vector2(540f, 180f), OnRestartButton);

        UITheme.AddImageButton(pausePanel.transform, "MainMenuBtn",
            "UI/Buttons/btn_mainmenu",
            new Vector2(0.5f, 0.28f), new Vector2(540f, 180f), OnMainMenuButton);

        pausePanel.SetActive(false); // always hidden until PauseGame() is called
    }
}
