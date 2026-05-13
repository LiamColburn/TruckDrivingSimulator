using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Displays game controls. Toggle with Tab key or call ShowControls()/HideControls().
/// Auto-creates UI if not assigned.
/// </summary>
public class ControlsUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private float autoHideDelay = 30f; // Hide after 30 seconds
    
    [Header("UI (auto-created if null)")]
    [SerializeField] private GameObject controlsPanel;
    
    private Canvas hudCanvas;
    private float visibleTimer = 0f;
    private bool isVisible = false;

    void Awake()
    {
        if (controlsPanel == null)
        {
            BuildControlsUI();
        }
    }

    void Start()
    {
        if (showOnStart)
        {
            ShowControls();
        }
        else
        {
            HideControls();
        }
    }

    void Update()
    {
        // Toggle with Tab
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleControls();
        }

        // Auto-hide after delay
        if (isVisible && autoHideDelay > 0f)
        {
            visibleTimer += Time.deltaTime;
            if (visibleTimer >= autoHideDelay)
            {
                HideControls();
            }
        }
    }

    public void ShowControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
            isVisible = true;
            visibleTimer = 0f;
        }
    }

    public void HideControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
            isVisible = false;
        }
    }

    public void ToggleControls()
    {
        if (isVisible)
            HideControls();
        else
            ShowControls();
    }

    // ── UI Auto-Build ─────────────────────────────────────────────────────────

    void BuildControlsUI()
    {
        // Find or create canvas
        hudCanvas = FindFirstObjectByType<Canvas>();
        
        if (hudCanvas == null)
        {
            GameObject canvasGO = new GameObject("HUD_Canvas");
            hudCanvas = canvasGO.AddComponent<Canvas>();
            hudCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create controls panel (bottom-left)
        controlsPanel = new GameObject("ControlsPanel");
        controlsPanel.transform.SetParent(hudCanvas.transform, false);

        RectTransform panelRT = controlsPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0f, 0f); // Bottom-left
        panelRT.anchorMax = new Vector2(0f, 0f);
        panelRT.pivot     = new Vector2(0f, 0f);
        panelRT.anchoredPosition = new Vector2(20f, 20f);
        panelRT.sizeDelta = new Vector2(400f, 280f);

        // Background
        Image bg = controlsPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.7f);

        // Title
        AddControlText(controlsPanel.transform, "Title", 
            "<b>CONTROLS</b>", 
            new Vector2(0f, 120f), 
            26f, 
            new Color(1f, 0.9f, 0.4f));

        // Control listings
        float yStart = 80f;
        float ySpacing = -32f;
        
        AddControlText(controlsPanel.transform, "Lane1", 
            "<b>A / ←</b>  Move Left", 
            new Vector2(0f, yStart), 24f);
        
        AddControlText(controlsPanel.transform, "Lane2", 
            "<b>D / →</b>  Move Right", 
            new Vector2(0f, yStart + ySpacing), 24f);
        
        AddControlText(controlsPanel.transform, "Hotdog", 
            "<b>H</b>  Eat Hotdog", 
            new Vector2(0f, yStart + ySpacing * 2), 24f);
        
        AddControlText(controlsPanel.transform, "BigGulp", 
            "<b>G</b>  Drink Big Gulp", 
            new Vector2(0f, yStart + ySpacing * 3), 24f);
        
        AddControlText(controlsPanel.transform, "RoadBeer", 
            "<b>B</b>  Road Beer", 
            new Vector2(0f, yStart + ySpacing * 4), 24f);
        
        AddControlText(controlsPanel.transform, "Horn", 
            "<b>SPACE</b>  Honk Horn", 
            new Vector2(0f, yStart + ySpacing * 5), 24f);
        
        AddControlText(controlsPanel.transform, "Toggle", 
            "<b>TAB</b>  Toggle Controls", 
            new Vector2(0f, yStart + ySpacing * 6), 20f, 
            new Color(0.7f, 0.7f, 0.7f));

        Debug.Log("ControlsUI: Auto-created controls panel");
    }

    void AddControlText(Transform parent, string name, string text, Vector2 pos, float fontSize, Color? color = null)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);

        RectTransform rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(360f, 30f);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color     = color ?? Color.white;
    }
}