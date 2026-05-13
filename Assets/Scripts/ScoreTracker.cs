// using UnityEngine;
// using UnityEngine.UI;

// /// <summary>
// /// Tracks score - distance traveled and time survived
// /// </summary>
// public class ScoreTracker : MonoBehaviour
// {
//     [Header("Score Settings")]
//     [SerializeField] private float pointsPerSecond = 10f;
//     [SerializeField] private float distanceMultiplier = 1f;
    
//     [Header("UI")]
//     [SerializeField] private Text scoreText;
//     [SerializeField] private Text distanceText;
//     [SerializeField] private Text timeText;
    
//     private float score = 0f;
//     private float distanceTraveled = 0f;
//     private float timeSurvived = 0f;
//     private Vector3 lastPosition;
//     private bool isPlaying = true;

//     void Start()
//     {
//         lastPosition = transform.position;
//     }

//     void Update()
//     {
//         if (!isPlaying)
//             return;
        
//         // Track time
//         timeSurvived += Time.deltaTime;
        
//         // Track distance
//         float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);
//         distanceTraveled += distanceThisFrame;
//         lastPosition = transform.position;
        
//         // Calculate score
//         score = (timeSurvived * pointsPerSecond) + (distanceTraveled * distanceMultiplier);
        
//         // Update UI
//         UpdateUI();
//     }

//     void UpdateUI()
//     {
//         if (scoreText != null)
//         {
//             scoreText.text = $"Score: {Mathf.FloorToInt(score)}";
//         }
        
//         if (distanceText != null)
//         {
//             distanceText.text = $"Distance: {Mathf.FloorToInt(distanceTraveled)}m";
//         }
        
//         if (timeText != null)
//         {
//             int minutes = Mathf.FloorToInt(timeSurvived / 60f);
//             int seconds = Mathf.FloorToInt(timeSurvived % 60f);
//             timeText.text = $"Time: {minutes:00}:{seconds:00}";
//         }
//     }

//     public void StopTracking()
//     {
//         isPlaying = false;
//     }

//     // Getters
//     public float GetScore() => score;
//     public float GetDistance() => distanceTraveled;
//     public float GetTime() => timeSurvived;
// }

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tracks score - distance traveled (simulated by road speed) and time survived.
/// Auto-creates UI if not assigned. Attach to the truck GameObject.
/// </summary>
public class ScoreTracker : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private float pointsPerSecond = 10f;
    [SerializeField] private float distancePerSecond = 20f; // Simulated speed (match road/traffic speed)
    [SerializeField] private float distanceMultiplier = 0.5f;

    [Header("Activity Bonuses")]
    public int hotdogBonus = 300;
    public int bigGulpBonus = 200;
    public int roadBeerBonus = 500;
    public int hornBonus = 50;
    public int smokingBonus = 400;


    [Header("UI (auto-created if null)")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject scorePanel;
    
    private Canvas hudCanvas;
    private float score = 0f;
    private float distanceTraveled = 0f;
    private float timeSurvived = 0f;
    private bool isPlaying = true;
    private Coroutine flashCoroutine;

    void Awake()
    {
        // Auto-build score UI if not assigned
        if (scorePanel == null || scoreText == null || distanceText == null || timeText == null)
        {
            BuildScoreUI();
        }
    }

    void Update()
    {
        if (!isPlaying)
            return;
        
        // Track time
        timeSurvived += Time.deltaTime;
        
        // Simulate distance traveled (since truck is stationary, road moves)
        // This represents how far the truck "would have" traveled
        distanceTraveled += distancePerSecond * Time.deltaTime;
        
        // Accumulate score incrementally; AddBonus() adds directly to score so each bonus counts exactly once
        score += (pointsPerSecond + distancePerSecond * distanceMultiplier) * Time.deltaTime;
        
        // Update UI
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE\n<size=42><b>{Mathf.FloorToInt(score)}</b></size>";
        }
        
        if (distanceText != null)
        {
            float miles = distanceTraveled * 0.000621371f; // meters to miles
            distanceText.text = $"MILES\n<size=42><b>{miles:F1}</b></size>";
        }
        
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeSurvived / 60f);
            int seconds = Mathf.FloorToInt(timeSurvived % 60f);
            timeText.text = $"TIME\n<size=42><b>{minutes:00}:{seconds:00}</b></size>";
        }
    }

    public void StopTracking()
    {
        isPlaying = false;
    }

    public void StartTracking()
    {
        isPlaying = true;
    }

    // Getters
    public float GetScore() => score;
    public float GetDistance() => distanceTraveled;
    public float GetTime() => timeSurvived;
    public int GetScoreInt() => Mathf.FloorToInt(score);
    public int GetDistanceInt() => Mathf.FloorToInt(distanceTraveled);

    public void AddBonus(int points)
    {
        score += points;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashScoreText());
        StartCoroutine(SpawnBonusPopup(points));
    }

    private System.Collections.IEnumerator FlashScoreText()
    {
        if (scoreText == null) yield break;
        scoreText.color = Color.yellow;
        yield return new WaitForSeconds(0.5f);
        scoreText.color = new Color(1f, 0.95f, 0.7f);
    }

    private System.Collections.IEnumerator SpawnBonusPopup(int points)
    {
        Canvas canvas = hudCanvas != null ? hudCanvas : FindFirstObjectByType<Canvas>();
        if (canvas == null) yield break;

        GameObject popupGO = new GameObject("BonusPopup");
        popupGO.transform.SetParent(canvas.transform, false);

        TextMeshProUGUI popup = popupGO.AddComponent<TextMeshProUGUI>();
        popup.text = $"+{points} pts";
        popup.fontSize = 52f;
        popup.fontStyle = FontStyles.Bold;
        popup.alignment = TextAlignmentOptions.Center;
        popup.color = new Color(1f, 0.95f, 0.7f);

        RectTransform rt = popupGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300f, 70f);
        rt.anchoredPosition = new Vector2(0f, 0f);

        Vector2 startPos = rt.anchoredPosition;
        float duration = 1f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.anchoredPosition = startPos + new Vector2(0f, 40f * t);
            popup.color = new Color(1f, 0.95f, 0.7f, 1f - t);
            yield return null;
        }

        Destroy(popupGO);
    }


    // ── UI Auto-Build ─────────────────────────────────────────────────────────

    void BuildScoreUI()
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

        // Create score panel (top-right corner)
        scorePanel = new GameObject("ScorePanel");
        scorePanel.transform.SetParent(hudCanvas.transform, false);

        RectTransform panelRT = scorePanel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1f, 1f); // Top-right
        panelRT.anchorMax = new Vector2(1f, 1f);
        panelRT.pivot     = new Vector2(1f, 1f);
        panelRT.anchoredPosition = new Vector2(-20f, -20f); // 20px padding from corner
        panelRT.sizeDelta = new Vector2(220f, 310f);

        // Semi-transparent background
        UnityEngine.UI.Image panelBg = scorePanel.AddComponent<UnityEngine.UI.Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.6f);

        // Create three stat displays spaced further apart so label + number dont overlap
        CreateStatText(scorePanel.transform, "ScoreText",    new Vector2(0f,  95f), out scoreText);
        CreateStatText(scorePanel.transform, "DistanceText", new Vector2(0f,   0f), out distanceText);
        CreateStatText(scorePanel.transform, "TimeText",     new Vector2(0f, -95f), out timeText);

        Debug.Log("ScoreTracker: Auto-created score UI in top-right corner");
    }

    void CreateStatText(Transform parent, string name, Vector2 yOffset, out TextMeshProUGUI textComponent)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);

        RectTransform rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = yOffset;
        rt.sizeDelta = new Vector2(200f, 80f);

        textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.fontSize  = 24f;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color     = new Color(1f, 0.95f, 0.7f); // Warm yellow-white
        textComponent.text      = "---";
    }
}