using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shared "trucker" visual theme. All menu scripts build their UI through here.
/// </summary>
public static class UITheme
{
    // ── Colour palette ────────────────────────────────────────────────────────
    public static readonly Color BgDark   = new Color(0.07f, 0.07f, 0.09f, 0.97f);
    public static readonly Color Gold     = new Color(1.00f, 0.78f, 0.08f, 1.00f);
    public static readonly Color Chrome   = new Color(0.58f, 0.60f, 0.66f, 1.00f);
    public static readonly Color OffWhite = new Color(0.95f, 0.95f, 0.90f, 1.00f);
    public static readonly Color Silver   = new Color(0.52f, 0.53f, 0.58f, 1.00f);
    public static readonly Color Green    = new Color(0.10f, 0.46f, 0.12f, 1.00f);
    public static readonly Color Red      = new Color(0.60f, 0.08f, 0.07f, 1.00f);
    public static readonly Color Blue     = new Color(0.10f, 0.28f, 0.58f, 1.00f);
    public static readonly Color DarkGray = new Color(0.20f, 0.20f, 0.24f, 1.00f);

    // ── Canvas factory ────────────────────────────────────────────────────────
    public static Canvas BuildCanvas(string name, int sortOrder)
    {
        EnsureEventSystem();

        var go = new GameObject(name);
        var c  = go.AddComponent<Canvas>();
        c.renderMode   = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortOrder;
        var s = go.AddComponent<CanvasScaler>();
        s.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920f, 1080f);
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    // Ensures an EventSystem exists with the correct input module for the
    // active input backend. Uses reflection — no hard package dependency.
    public static void EnsureEventSystem()
    {
        // Detect whether the New Input System package is installed
        var newModuleType = System.Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

        var existing = Object.FindFirstObjectByType<EventSystem>();
        if (existing != null)
        {
            // If New Input System is present but the EventSystem only has the
            // legacy StandaloneInputModule, swap it out — otherwise pointer
            // events never reach the UI buttons.
            if (newModuleType != null && existing.GetComponent(newModuleType) == null)
            {
                var legacy = existing.GetComponent<StandaloneInputModule>();
                if (legacy != null) legacy.enabled = false;
                existing.gameObject.AddComponent(newModuleType);
            }
            return;
        }

        // No EventSystem at all — create one from scratch
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        if (newModuleType != null)
            esGO.AddComponent(newModuleType);
        else
            esGO.AddComponent<StandaloneInputModule>();
    }

    // ── Full-screen panel: dark bg + gold top/bottom strips ───────────────────
    public static GameObject BuildPanel(Transform parent, Color? bgTint = null)
    {
        var panel = new GameObject("Panel");
        panel.transform.SetParent(parent, false);
        StretchToFill(panel.AddComponent<RectTransform>());
        var bg = panel.AddComponent<Image>();
        bg.color         = bgTint ?? BgDark;
        bg.raycastTarget = false;

        EdgeBar(panel.transform, top: true,  Gold, 8f);
        EdgeBar(panel.transform, top: false, Gold, 8f);
        return panel;
    }

    // ── Title badge: dark rect with coloured border strips + text ────────────
    public static void AddTitleBadge(Transform parent, string line1, string line2,
        Vector2 anchor, Color textColor, Color? barColor = null)
    {
        Color bar = barColor ?? Gold;
        bool twoLines = !string.IsNullOrEmpty(line2);

        var badge = MakeRect(parent, "TitleBadge", anchor,
            new Vector2(820f, twoLines ? 178f : 112f), Vector2.zero,
            new Color(0f, 0f, 0f, 0.50f), false);

        EdgeBar(badge.transform, top: true,  bar, 5f);
        EdgeBar(badge.transform, top: false, bar, 5f);

        float y1 = twoLines ? 0.66f : 0.50f;
        AddLabel(badge.transform, "L1", line1,
            new Vector2(0.5f, y1), new Vector2(790f, 84f), 68f, textColor);

        if (twoLines)
            AddLabel(badge.transform, "L2", line2,
                new Vector2(0.5f, 0.24f), new Vector2(790f, 58f), 42f, textColor);
    }

    // ── Thin gold divider line ────────────────────────────────────────────────
    public static void AddDivider(Transform parent, Vector2 anchor, float width = 500f)
    {
        var go = new GameObject("Divider");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta         = new Vector2(width, 3f);
        rt.anchoredPosition  = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color         = Gold;
        img.raycastTarget = false;
    }

    // ── Styled button: drop shadow + border ring + flat interactive fill ────────
    // All three are siblings in parent — Button lives on the fill GO directly,
    // same structure as the original working buttons, just with added decoration.
    public static Button AddButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 size, Color fillColor,
        UnityEngine.Events.UnityAction onClick,
        Color? frameColor = null)
    {
        Color border = frameColor ?? Gold;
        Vector2 innerSize = size - new Vector2(8f, 8f); // inset 4 px each side

        // 1. Shadow — rendered first so it sits behind everything
        MakeRect(parent, name + "_Shd",
            anchor, size + new Vector2(6f, 6f), new Vector2(5f, -5f),
            new Color(0f, 0f, 0f, 0.60f), false);

        // 2. Border ring — slightly larger than fill, purely decorative
        MakeRect(parent, name + "_Border",
            anchor, size, Vector2.zero, border, false);

        // 3. Fill — the interactive button surface (same level as border sibling)
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = anchor;
        rt.sizeDelta        = innerSize;
        rt.anchoredPosition = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color         = fillColor;
        img.raycastTarget = true;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.navigation    = new Navigation { mode = Navigation.Mode.None };

        var cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(0.80f, 0.80f, 0.80f);
        cb.pressedColor     = new Color(0.58f, 0.58f, 0.58f);
        cb.selectedColor    = Color.white;
        cb.fadeDuration     = 0.07f;
        btn.colors = cb;

        btn.onClick.AddListener(onClick);

        // Label inside the fill GO — raycastTarget off so it never intercepts clicks
        AddLabel(go.transform, name + "_Lbl", label,
            new Vector2(0.5f, 0.5f), innerSize * 0.88f, 34f, OffWhite);

        return btn;
    }

    // ── Sprite image button (truck PNGs) ─────────────────────────────────────
    // resourcePath is relative to a Resources folder, e.g. "UI/Buttons/btn_play"
    public static Button AddImageButton(Transform parent, string name,
        string resourcePath, Vector2 anchor, Vector2 size,
        UnityEngine.Events.UnityAction onClick)
    {
        var sprite = LoadSpriteTransparent(resourcePath);

        // Drop shadow
        MakeRect(parent, name + "_Shd",
            anchor, size + new Vector2(8f, 8f), new Vector2(6f, -6f),
            new Color(0f, 0f, 0f, 0.40f), false);

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = anchor;
        rt.sizeDelta        = size;
        rt.anchoredPosition = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.raycastTarget  = true;
        if (sprite != null)
        {
            img.sprite         = sprite;
            img.preserveAspect = true;
            img.color          = Color.white;
        }
        else
        {
            // Fallback flat button if sprite file isn't found yet
            img.color = new Color(0.3f, 0.3f, 0.35f);
            AddLabel(go.transform, name + "_Lbl", name,
                new Vector2(0.5f, 0.5f), size * 0.88f, 32f, OffWhite);
            Debug.LogWarning($"UITheme: sprite not found at Resources/{resourcePath}");
        }

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.navigation    = new Navigation { mode = Navigation.Mode.None };

        var cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
        cb.pressedColor     = new Color(0.65f, 0.65f, 0.65f);
        cb.selectedColor    = Color.white;
        cb.fadeDuration     = 0.07f;
        btn.colors          = cb;

        btn.onClick.AddListener(onClick);
        return btn;
    }

    // ── TMP label ─────────────────────────────────────────────────────────────
    public static TextMeshProUGUI AddLabel(Transform parent, string name, string text,
        Vector2 anchor, Vector2 size, float fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta        = size;
        rt.anchoredPosition = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = text;
        tmp.fontSize      = fontSize;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.color         = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    // ── Utilities ─────────────────────────────────────────────────────────────
    public static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // Loads a sprite and makes near-white pixels transparent so button PNGs
    // with white backgrounds display correctly without needing external editing.
    // Requires Read/Write enabled on the texture import settings.
    static Sprite LoadSpriteTransparent(string resourcePath, float threshold = 0.85f)
    {
        var source = Resources.Load<Texture2D>(resourcePath);
        if (source == null) return null;

        try
        {
            var tex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            tex.SetPixels(source.GetPixels()); // throws if Read/Write not enabled

            var pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                if (c.r > threshold && c.g > threshold && c.b > threshold)
                    pixels[i] = Color.clear;
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f);
        }
        catch
        {
            Debug.LogWarning($"UITheme: '{resourcePath}' needs Read/Write enabled for background removal. Using sprite as-is.");
            return Resources.Load<Sprite>(resourcePath);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    static void EdgeBar(Transform parent, bool top, Color color, float height)
    {
        var go = new GameObject(top ? "TopBar" : "BotBar");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        if (top)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(0f, -height);
            rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1f, 0f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = new Vector2(0f, height);
        }
        var img = go.AddComponent<Image>();
        img.color         = color;
        img.raycastTarget = false;
    }

    static GameObject MakeRect(Transform parent, string name,
        Vector2 anchor, Vector2 size, Vector2 offset, Color color, bool interactive)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = anchor;
        rt.sizeDelta        = size;
        rt.anchoredPosition = offset;
        var img = go.AddComponent<Image>();
        img.color         = color;
        img.raycastTarget = interactive;
        return go;
    }
}
