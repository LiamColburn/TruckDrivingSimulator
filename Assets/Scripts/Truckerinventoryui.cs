using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Displays trucker inventory (hotdogs, drinks, beers, cigarettes) in a clean HUD panel.
/// Shows sprite icons next to counts. Auto-creates UI if not assigned.
/// </summary>
public class TruckerInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TruckPlayerController playerController;
    
    [Header("Icons (assign sprites in Inspector)")]
    [SerializeField] private Sprite hotdogIcon;
    [SerializeField] private Sprite bigGulpIcon;
    [SerializeField] private Sprite roadBeerIcon;
    [SerializeField] private Sprite cigaretteIcon;

    [Header("UI Elements (auto-created if null)")]
    [SerializeField] private GameObject inventoryPanel;

    // Internal references
    private TextMeshProUGUI hotdogText;
    private TextMeshProUGUI bigGulpText;
    private TextMeshProUGUI roadBeerText;
    private TextMeshProUGUI cigaretteText;

    private Image hotdogImage;
    private Image bigGulpImage;
    private Image roadBeerImage;
    private Image cigaretteImage;

    private Canvas hudCanvas;

    void Awake()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<TruckPlayerController>();

        if (inventoryPanel == null)
            BuildInventoryUI();
    }

    void Update()
    {
        if (playerController == null) return;
        UpdateInventoryDisplay();
    }

    void UpdateInventoryDisplay()
    {
        UpdateItem(hotdogText,    hotdogImage,    playerController.GetHotdogsRemaining(),     new Color(1f, 0.9f, 0.1f));
        UpdateItem(bigGulpText,   bigGulpImage,   playerController.GetBigGulpsRemaining(),    new Color(0.2f, 0.85f, 1f));
        UpdateItem(roadBeerText,  roadBeerImage,  playerController.GetRoadBeersRemaining(),   new Color(1f, 0.85f, 0.25f));
        UpdateItem(cigaretteText, cigaretteImage, playerController.GetCigarettesRemaining(),  new Color(0.9f, 0.85f, 0.75f));
    }

    void UpdateItem(TextMeshProUGUI text, Image icon, int count, Color activeColor)
    {
        if (text != null)
        {
            text.text  = $"x{count}";
            text.color = count > 0 ? activeColor : new Color(0.4f, 0.4f, 0.4f);
        }

        if (icon != null)
        {
            // Gray out icon when empty
            icon.color = count > 0 ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.6f);
        }
    }

    // ── UI Auto-Build ─────────────────────────────────────────────────────────

    void BuildInventoryUI()
    {
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

        // Main panel — top-left
        inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(hudCanvas.transform, false);

        RectTransform panelRT = inventoryPanel.AddComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0f, 1f);
        panelRT.anchorMax        = new Vector2(0f, 1f);
        panelRT.pivot            = new Vector2(0f, 1f);
        panelRT.anchoredPosition = new Vector2(20f, -20f);
        panelRT.sizeDelta        = new Vector2(160f, 260f);

        Image panelBg = inventoryPanel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.6f);

        // Title
        CreateTitle(inventoryPanel.transform);

        // Each row: icon + count text
        float yStart  = 65f;
        float yStep   = -50f;

        CreateRow(inventoryPanel.transform, "Hotdog",    hotdogIcon,    yStart + yStep * 0, out hotdogImage,    out hotdogText);
        CreateRow(inventoryPanel.transform, "BigGulp",   bigGulpIcon,   yStart + yStep * 1, out bigGulpImage,   out bigGulpText);
        CreateRow(inventoryPanel.transform, "RoadBeer",  roadBeerIcon,  yStart + yStep * 2, out roadBeerImage,  out roadBeerText);
        CreateRow(inventoryPanel.transform, "Cigarette", cigaretteIcon, yStart + yStep * 3, out cigaretteImage, out cigaretteText);
    }

    void CreateTitle(Transform parent)
    {
        GameObject go = new GameObject("Title");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 1f);
        rt.anchorMax        = new Vector2(0.5f, 1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -8f);
        rt.sizeDelta        = new Vector2(140f, 30f);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = "SUPPLIES";
        tmp.fontSize  = 18f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = new Color(0.8f, 0.8f, 0.8f);
    }

    void CreateRow(Transform parent, string name, Sprite icon,
                   float yPos, out Image iconImage, out TextMeshProUGUI countText)
    {
        // Row container
        GameObject row = new GameObject(name + "Row");
        row.transform.SetParent(parent, false);

        RectTransform rowRT = row.AddComponent<RectTransform>();
        rowRT.anchorMin        = new Vector2(0.5f, 0.5f);
        rowRT.anchorMax        = new Vector2(0.5f, 0.5f);
        rowRT.pivot            = new Vector2(0.5f, 0.5f);
        rowRT.anchoredPosition = new Vector2(0f, yPos);
        rowRT.sizeDelta        = new Vector2(140f, 44f);

        // Icon (left side)
        GameObject iconGO = new GameObject(name + "Icon");
        iconGO.transform.SetParent(row.transform, false);

        RectTransform iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin        = new Vector2(0f, 0.5f);
        iconRT.anchorMax        = new Vector2(0f, 0.5f);
        iconRT.pivot            = new Vector2(0f, 0.5f);
        iconRT.anchoredPosition = new Vector2(8f, 0f);
        iconRT.sizeDelta        = new Vector2(36f, 36f);

        iconImage               = iconGO.AddComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget  = false;

        if (icon != null)
            iconImage.sprite = icon;
        else
            iconImage.color = new Color(1f, 1f, 1f, 0f); // invisible if no sprite

        // Count text (right of icon)
        GameObject textGO = new GameObject(name + "Text");
        textGO.transform.SetParent(row.transform, false);

        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin        = new Vector2(0f, 0.5f);
        textRT.anchorMax        = new Vector2(1f, 0.5f);
        textRT.pivot            = new Vector2(0f, 0.5f);
        textRT.anchoredPosition = new Vector2(52f, 0f);
        textRT.sizeDelta        = new Vector2(0f, 36f);

        countText           = textGO.AddComponent<TextMeshProUGUI>();
        countText.fontSize  = 26f;
        countText.fontStyle = FontStyles.Bold;
        countText.alignment = TextAlignmentOptions.Left;
        countText.color     = Color.white;
        countText.text      = "x0";
    }
}