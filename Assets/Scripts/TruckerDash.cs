using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Truck dashboard POV overlay.
/// - Renders the Truck_Dash image fullscreen as a UI overlay
/// - Places the steering wheel at the bottom center
/// - Rotates the wheel when the player changes lanes
/// 
/// Setup:
/// 1. Attach to UI_Manager (or any persistent GameObject)
/// 2. Assign dashSprite (Truck_Dash.png) in Inspector
/// 3. Assign wheelSprite (steering_wheel.png) in Inspector
/// 4. Assign playerController in Inspector (or it auto-finds)
/// </summary>
public class TruckerDash : MonoBehaviour
{
    [Header("Sprites (assign in Inspector)")]
    [SerializeField] private Sprite dashSprite;
    [SerializeField] private Sprite wheelSprite;

    [Header("Steering Wheel Settings")]
    [SerializeField] private float wheelRotationAngle = 35f;  // degrees to rotate on lane change
    [SerializeField] private float wheelRotationSpeed = 4f;   // lerp speed back to center
    [SerializeField] private Vector2 wheelSize        = new Vector2(500f, 500f);
    [SerializeField] private Vector2 wheelPosition    = new Vector2(-500f, 250f); // offset from bottom center

    [Header("References")]
    [SerializeField] private TruckPlayerController playerController;

    private Canvas        hudCanvas;
    private Image         dashImage;
    private Image         wheelImage;
    private RectTransform wheelRT;

    private float currentWheelAngle = 0f;
    private float targetWheelAngle  = 0f;
    private int   lastLane          = 1;

    void Awake()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<TruckPlayerController>();

        EnsureCanvas();
        BuildDashOverlay();
        BuildSteeringWheel();
    }

    void Update()
    {
        HandleWheelRotation();
    }

    // ── Steering Wheel ────────────────────────────────────────────────────────

    void HandleWheelRotation()
    {
        if (wheelRT == null) return;

        // If not changing lanes, return wheel to center
        if (playerController != null && !playerController.IsChangingLanes())
            targetWheelAngle = 0f;

        // Smoothly rotate toward target (set by OnLaneChange)
        currentWheelAngle = Mathf.Lerp(currentWheelAngle, targetWheelAngle, Time.deltaTime * wheelRotationSpeed);
        wheelRT.localRotation = Quaternion.Euler(0f, 0f, currentWheelAngle);
    }

    /// <summary>
    /// Call this from TruckPlayerController when a lane change starts.
    /// direction: -1 = left, 1 = right
    /// </summary>
    public void OnLaneChange(int direction)
    {
        targetWheelAngle = direction * wheelRotationAngle;
    }

    // ── UI Build ──────────────────────────────────────────────────────────────

    void EnsureCanvas()
    {
        // Try to find an existing canvas on this GameObject or children
        hudCanvas = GetComponentInChildren<Canvas>();

        if (hudCanvas == null)
            hudCanvas = FindFirstObjectByType<Canvas>();

        if (hudCanvas == null)
        {
            GameObject canvasGO = new GameObject("HUD_Canvas");
            canvasGO.transform.SetParent(transform);
            hudCanvas = canvasGO.AddComponent<Canvas>();
            hudCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasGO.AddComponent<GraphicRaycaster>();
        }
    }

    void BuildDashOverlay()
    {
        // Dash sits at the BOTTOM of the canvas draw order (behind other UI)
        GameObject dashGO = new GameObject("TruckDash");
        dashGO.transform.SetParent(hudCanvas.transform, false);

        // Push to back of sibling order so other UI renders on top
        dashGO.transform.SetAsFirstSibling();

        RectTransform rt = dashGO.AddComponent<RectTransform>();
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;

        dashImage               = dashGO.AddComponent<Image>();
        dashImage.raycastTarget = false; // Don't block clicks
        dashImage.preserveAspect = false; // Stretch to fill screen

        if (dashSprite != null)
        {
            dashImage.sprite = dashSprite;
            dashImage.type   = Image.Type.Simple;
            dashImage.color  = Color.white;
        }
        else
        {
            // Placeholder tint if no sprite assigned
            dashImage.color = new Color(0.15f, 0.12f, 0.1f, 0.85f);
            Debug.LogWarning("TruckerDash: No dash sprite assigned! Assign Truck_Dash.png in Inspector.");
        }
    }

    void BuildSteeringWheel()
    {
        GameObject wheelGO = new GameObject("SteeringWheel");
        wheelGO.transform.SetParent(hudCanvas.transform, false);

        // Wheel renders above dash but below other HUD elements
        wheelGO.transform.SetSiblingIndex(1);

        wheelRT = wheelGO.AddComponent<RectTransform>();
        wheelRT.anchorMin        = new Vector2(0.5f, 0f); // Bottom center
        wheelRT.anchorMax        = new Vector2(0.5f, 0f);
        wheelRT.pivot            = new Vector2(0.5f, 0.5f);
        wheelRT.anchoredPosition = wheelPosition;
        wheelRT.sizeDelta        = wheelSize;

        wheelImage               = wheelGO.AddComponent<Image>();
        wheelImage.raycastTarget = false;
        wheelImage.preserveAspect = true;
        wheelImage.color = new Color(1f, 1f, 1f, 0f); // transparent until sprite loads

        if (wheelSprite != null)
        {
            wheelImage.sprite = wheelSprite;
            wheelImage.color  = Color.white;
        }
        else
        {
            wheelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            Debug.LogWarning("TruckerDash: No wheel sprite assigned! Assign steering_wheel.png in Inspector.");
        }
    }
}