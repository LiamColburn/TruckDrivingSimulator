using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;

    public static bool IsShowing { get; private set; }

    // Set to true before reloading the scene to bypass the main menu on next load
    public static bool SkipOnNextLoad { get; set; }

    IEnumerator Start()
    {
        Debug.Log("MainMenuUI: Start() running");
        EnsureMenuPanel();
        yield return null; // let canvas + EventSystem finish initialising

        if (SkipOnNextLoad)
        {
            SkipOnNextLoad = false;
            IsShowing = false;
            yield break;
        }

        ShowMainMenu();
        Debug.Log("MainMenuUI: showing. IsShowing=" + IsShowing);
    }

    void Update()
    {
        if (!IsShowing) return;

        bool pressed = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            pressed = true;
#endif
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
            pressed = true;
#endif
        if (pressed)
        {
            Debug.Log("MainMenuUI: keyboard triggered play");
            OnPlayButton();
        }
    }

    void ShowMainMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        Time.timeScale = 0f;
        IsShowing = true;
    }

    public void OnPlayButton()
    {
        Debug.Log("MainMenuUI: OnPlayButton called");
        if (menuPanel != null) menuPanel.SetActive(false);
        Time.timeScale = 1f;
        IsShowing = false;
    }

    public void OnQuitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── UI builder ────────────────────────────────────────────────────────────

    void EnsureMenuPanel()
    {
        if (menuPanel != null) return;

        var canvas = UITheme.BuildCanvas("MainMenu_Canvas", 70);

        menuPanel = UITheme.BuildPanel(canvas.transform);

        // Title badge — two lines, amber gold
        UITheme.AddTitleBadge(menuPanel.transform,
            "TRUCK DRIVING", "SIMULATOR",
            new Vector2(0.5f, 0.73f), UITheme.Gold);

        // Tagline
        UITheme.AddLabel(menuPanel.transform, "Tagline",
            "Keep it between the lines...",
            new Vector2(0.5f, 0.58f), new Vector2(700f, 48f), 26f, UITheme.Silver);

        UITheme.AddImageButton(menuPanel.transform, "PlayBtn",
            "UI/Buttons/btn_play",
            new Vector2(0.5f, 0.42f), new Vector2(540f, 180f), OnPlayButton);

        UITheme.AddImageButton(menuPanel.transform, "QuitBtn",
            "UI/Buttons/btn_quit",
            new Vector2(0.5f, 0.25f), new Vector2(540f, 180f), OnQuitButton);

        menuPanel.SetActive(false);
        Debug.Log("MainMenuUI: panel built");
    }
}
