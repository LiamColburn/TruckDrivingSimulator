using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Displays trucker inventory (hotdogs, drinks, beers) in a clean HUD panel.
/// Auto-creates UI if not assigned. Place on a GameObject in the scene.
/// </summary>
public class TruckerInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TruckPlayerController playerController;
    
    [Header("UI Elements (auto-created if null)")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TextMeshProUGUI hotdogText;
    [SerializeField] private TextMeshProUGUI bigGulpText;
    [SerializeField] private TextMeshProUGUI roadBeerText;
    
    [Header("Optional Icons")]
    [SerializeField] private Sprite hotdogIcon;
    [SerializeField] private Sprite bigGulpIcon;
    [SerializeField] private Sprite roadBeerIcon;
    
    private Canvas hudCanvas;

    void Awake()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<TruckPlayerController>();
        }
        
        if (inventoryPanel == null || hotdogText == null || bigGulpText == null || roadBeerText == null)
        {
            BuildInventoryUI();
        }
    }

    void Update()
    {
        if (playerController == null)
            return;
        
        UpdateInventoryDisplay();
    }

    void UpdateInventoryDisplay()
    {
        if (hotdogText != null)
        {
            int count = playerController.GetHotdogsRemaining();
            hotdogText.text = $"🌭 x{count}";
            hotdogText.color = count > 0 ? new Color(1f, 0.9f, 0.1f) : new Color(0.5f, 0.5f, 0.5f);
        }
        
        if (bigGulpText != null)
        {
            int count = playerController.GetBigGulpsRemaining();
            bigGulpText.text = $"🥤 x{count}";
            bigGulpText.color = count > 0 ? new Color(0.2f, 0.85f, 1f) : new Color(0.5f, 0.5f, 0.5f);
        }
        
        if (roadBeerText != null)
        {
            int count = playerController.GetRoadBeersRemaining();
            roadBeerText.text = $"🍺 x{count}";
            roadBeerText.color = count > 0 ? new Color(1f, 0.85f, 0.25f) : new Color(0.5f, 0.5f, 0.5f);
        }
    }

    // ── UI Auto-Build ─────────────────────────────────────────────────────────

    void BuildInventoryUI()
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

        // Create inventory panel (top-left corner)
        inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(hudCanvas.transform, false);

        RectTransform panelRT = inventoryPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0f, 1f); // Top-left
        panelRT.anchorMax = new Vector2(0f, 1f);
        panelRT.pivot     = new Vector2(0f, 1f);
        panelRT.anchoredPosition = new Vector2(20f, -20f); // 20px padding from corner
        panelRT.sizeDelta = new Vector2(220f, 180f);

        // Semi-transparent background
        Image panelBg = inventoryPanel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.6f);

        // Title
        CreateTitle(inventoryPanel.transform);
        
        // Create item displays
        CreateItemText(inventoryPanel.transform, "HotdogText", new Vector2(0f, 25f), out hotdogText);
        CreateItemText(inventoryPanel.transform, "BigGulpText", new Vector2(0f, -15f), out bigGulpText);
        CreateItemText(inventoryPanel.transform, "RoadBeerText", new Vector2(0f, -55f), out roadBeerText);

        Debug.Log("TruckerInventoryUI: Auto-created inventory panel in top-left corner");
    }

    void CreateTitle(Transform parent)
    {
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(parent, false);

        RectTransform rt = titleGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -10f);
        rt.sizeDelta = new Vector2(200f, 35f);

        TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text      = "SUPPLIES";
        title.fontSize  = 20f;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color     = new Color(0.8f, 0.8f, 0.8f);
    }

    void CreateItemText(Transform parent, string name, Vector2 yOffset, out TextMeshProUGUI textComponent)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);

        RectTransform rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = yOffset;
        rt.sizeDelta = new Vector2(180f, 40f);

        textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.fontSize  = 28f;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color     = Color.white;
        textComponent.text      = "--- x0";
    }
}