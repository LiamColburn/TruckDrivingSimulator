using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Activity HUD: toast messages, screen flash, road beer vignette.
/// Auto-builds its Canvas and panels in Awake if not pre-wired in the Inspector.
/// Attach to any persistent GameObject (e.g. a dedicated "UI_Manager" object).
/// </summary>
public class TruckerUI : MonoBehaviour
{
    [Header("Activity Toast (auto-created if null)")]
    [SerializeField] private GameObject activityPanel;
    [SerializeField] private TextMeshProUGUI activityText;

    [Header("Overlays (auto-created if null)")]
    [SerializeField] private Image flashOverlay;
    [SerializeField] private Image vignetteOverlay;

    [Header("Optional Icon Sprites")]
    [SerializeField] private Sprite hotdogSprite;
    [SerializeField] private Sprite bigGulpSprite;
    [SerializeField] private Sprite roadBeerSprite;
    [SerializeField] private Sprite honkSprite;

    private Canvas hudCanvas;
    private Coroutine activityCoroutine;
    private Coroutine flashCoroutine;
    private Coroutine vignetteCoroutine;

    void Awake()
    {
        EnsureCanvas();
        if (activityPanel == null) BuildActivityPanel();
        if (flashOverlay == null)  BuildFullScreenOverlay("FlashOverlay",    new Color(1f, 1f, 1f, 0f), out flashOverlay);
        if (vignetteOverlay == null) BuildFullScreenOverlay("RoadBeerVignette", new Color(0.8f, 0.35f, 0f, 0f), out vignetteOverlay);
    }

    // ── public API ────────────────────────────────────────────────────────────

    public void ShowEating(float duration)
    {
        ShowToast("Munchin' a hotdog!", hotdogSprite, duration, new Color(1f, 0.85f, 0.1f));
        Flash(new Color(1f, 0.8f, 0f, 0.18f));
    }

    public void ShowDrinking(float duration)
    {
        ShowToast("Sippin' that Big Gulp!", bigGulpSprite, duration, new Color(0.2f, 0.9f, 1f));
        Flash(new Color(0f, 0.8f, 1f, 0.15f));
    }

    public void ShowRoadBeer(float duration)
    {
        ShowToast("Crackin' a cold one!", roadBeerSprite, duration, new Color(1f, 0.85f, 0.25f));
        Flash(new Color(1f, 0.6f, 0f, 0.25f));
        TriggerVignette(duration + 5f);
    }

    public void ShowHonk()
    {
        ShowToast("HONK HONK!", honkSprite, 0.75f, Color.white);
        Flash(new Color(1f, 1f, 1f, 0.22f));
    }

    // ── internal helpers ──────────────────────────────────────────────────────

    void ShowToast(string message, Sprite icon, float duration, Color tint)
    {
        if (activityCoroutine != null) StopCoroutine(activityCoroutine);

        if (activityText != null)
        {
            activityText.text  = message;
            activityText.color = tint;
        }

        if (activityPanel != null) activityPanel.SetActive(true);
        activityCoroutine = StartCoroutine(HideToastAfter(duration));
    }

    IEnumerator HideToastAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activityPanel != null) activityPanel.SetActive(false);
    }

    void Flash(Color color, float duration = 0.18f)
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(DoFlash(color, duration));
    }

    IEnumerator DoFlash(Color color, float duration)
    {
        if (flashOverlay == null) yield break;
        flashOverlay.color = color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color c = color;
            c.a = Mathf.Lerp(color.a, 0f, elapsed / duration);
            flashOverlay.color = c;
            yield return null;
        }
        flashOverlay.color = new Color(color.r, color.g, color.b, 0f);
    }

    void TriggerVignette(float duration)
    {
        if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
        vignetteCoroutine = StartCoroutine(DoVignette(duration));
    }

    IEnumerator DoVignette(float duration)
    {
        if (vignetteOverlay == null) yield break;
        const float maxAlpha  = 0.2f;
        const float pulsePeriod = 1.1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fadeOut = 1f - Mathf.Clamp01(elapsed / duration);
            float pulse   = (Mathf.Sin(elapsed * (Mathf.PI * 2f / pulsePeriod)) + 1f) * 0.5f;
            vignetteOverlay.color = new Color(0.8f, 0.35f, 0f, pulse * maxAlpha * fadeOut);
            yield return null;
        }
        vignetteOverlay.color = new Color(0.8f, 0.35f, 0f, 0f);
    }

    // ── auto-build helpers ────────────────────────────────────────────────────

    void EnsureCanvas()
    {
        hudCanvas = GetComponentInChildren<Canvas>();
        if (hudCanvas != null) return;

        GameObject go = new GameObject("HUD_Canvas");
        go.transform.SetParent(transform);
        hudCanvas = go.AddComponent<Canvas>();
        hudCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 10;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        go.AddComponent<GraphicRaycaster>();
    }

    void BuildActivityPanel()
    {
        GameObject panel = new GameObject("ActivityPanel");
        panel.transform.SetParent(hudCanvas.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin       = new Vector2(0.5f, 0f);
        rt.anchorMax       = new Vector2(0.5f, 0f);
        rt.pivot           = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 90f);
        rt.sizeDelta       = new Vector2(480f, 70f);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.62f);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(panel.transform, false);

        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin  = Vector2.zero;
        textRT.anchorMax  = Vector2.one;
        textRT.offsetMin  = new Vector2(12f, 4f);
        textRT.offsetMax  = new Vector2(-12f, -4f);

        activityText                  = textGO.AddComponent<TextMeshProUGUI>();
        activityText.fontSize         = 30f;
        activityText.fontStyle        = FontStyles.Bold;
        activityText.alignment        = TextAlignmentOptions.Center;
        activityText.color            = Color.white;

        activityPanel = panel;
        panel.SetActive(false);
    }

    void BuildFullScreenOverlay(string goName, Color initialColor, out Image result)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(hudCanvas.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;

        result                 = go.AddComponent<Image>();
        result.color           = initialColor;
        result.raycastTarget   = false;
    }
}
