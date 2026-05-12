using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Truck controller with video playback for activities
/// Lane changing + eating/drinking/smoking with video overlays
/// </summary>
public class TruckPlayerController : MonoBehaviour
{
    [Header("Lane Changing")]
    [SerializeField] private float[] laneXPositions = new float[] { -4.66f, 0f, 4.66f };
    [SerializeField] private float laneChangeDuration = 2.5f;
    private int currentLane = 1;
    private int targetLane = 1;
    private bool isChangingLanes = false;
    private float laneChangeProgress = 1f;
    private float startX;
    private float targetX;
    
    [Header("Trucker Activities")]
    [SerializeField] private int hotdogsRemaining = 10;
    [SerializeField] private int bigGulpsRemaining = 5;
    [SerializeField] private int roadBeersRemaining = 6;
    [SerializeField] private int cigarettesRemaining = 20;
    
    [Header("References")]
    [SerializeField] private TruckerVideoOverlay videoOverlay;
    
    private bool isBusy = false; // Eating, drinking, or smoking
    
    void Start()
    {
        // Set starting position
        Vector3 pos = transform.position;
        pos.x = laneXPositions[currentLane];
        transform.position = pos;
        
        // Find video overlay if not assigned
        if (videoOverlay == null)
        {
            videoOverlay = FindFirstObjectByType<TruckerVideoOverlay>();
        }
        
        Debug.Log("=== TRUCK DRIVER READY ===");
        Debug.Log("Controls:");
        Debug.Log("  A/D or Arrows = Change Lanes");
        Debug.Log("  H = Hotdog");
        Debug.Log("  G = Big Gulp");
        Debug.Log("  B = Road Beer");
        Debug.Log("  C = Cigarette");
        Debug.Log("  Space = Horn");
    }
    
    void Update()
    {
        HandleInput();
        
        if (isChangingLanes)
        {
            UpdateLaneChange();
        }
    }
    
    void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Lane changing (only when not busy)
        if (!isChangingLanes && !isBusy)
        {
            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                ChangeLane(-1);
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                ChangeLane(1);
            }
        }
        
        // Activities (only when not busy and not changing lanes)
        if (!isBusy && !isChangingLanes)
        {
            if (keyboard.hKey.wasPressedThisFrame)
            {
                EatHotdog();
            }
            else if (keyboard.gKey.wasPressedThisFrame)
            {
                DrinkBigGulp();
            }
            else if (keyboard.bKey.wasPressedThisFrame)
            {
                DrinkRoadBeer();
            }
            else if (keyboard.cKey.wasPressedThisFrame)
            {
                SmokeCigarette();
            }
        }
        
        // Horn (can always honk)
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            HonkHorn();
        }
    }
    
    void ChangeLane(int direction)
    {
        int newLane = currentLane + direction;
        
        if (newLane < 0 || newLane >= laneXPositions.Length)
        {
            Debug.Log("❌ Can't change lanes - at edge!");
            return;
        }
        
        targetLane = newLane;
        isChangingLanes = true;
        laneChangeProgress = 0f;
        startX = transform.position.x;
        targetX = laneXPositions[targetLane];
        
        string[] names = { "LEFT", "CENTER", "RIGHT" };
        Debug.Log($"🚗 Changing to {names[targetLane]} lane");
    }
    
    void UpdateLaneChange()
    {
        laneChangeProgress += Time.deltaTime / laneChangeDuration;
        
        if (laneChangeProgress >= 1f)
        {
            laneChangeProgress = 1f;
            isChangingLanes = false;
            currentLane = targetLane;
            
            Vector3 finalPos = transform.position;
            finalPos.x = laneXPositions[targetLane];
            transform.position = finalPos;
            
            Debug.Log($"✅ Lane change complete!");
            return;
        }
        
        float t = Mathf.SmoothStep(0f, 1f, laneChangeProgress);
        float newX = Mathf.Lerp(startX, targetX, t);
        
        Vector3 newPos = transform.position;
        newPos.x = newX;
        transform.position = newPos;
    }
    
    void EatHotdog()
    {
        if (hotdogsRemaining <= 0)
        {
            Debug.Log("❌ Out of hotdogs!");
            return;
        }
        
        hotdogsRemaining--;
        Debug.Log($"🌭 Eating hotdog... ({hotdogsRemaining} left)");
        
        // Play video
        if (videoOverlay != null)
        {
            videoOverlay.PlayHotdogVideo();
            StartCoroutine(ActivityCooldown(2f)); // 2 seconds busy
        }
    }
    
    void DrinkBigGulp()
    {
        if (bigGulpsRemaining <= 0)
        {
            Debug.Log("❌ Out of Big Gulps!");
            return;
        }
        
        bigGulpsRemaining--;
        Debug.Log($"🥤 Drinking Big Gulp... ({bigGulpsRemaining} left)");
        
        // Play video
        if (videoOverlay != null)
        {
            videoOverlay.PlayBigGulpVideo();
            StartCoroutine(ActivityCooldown(1.5f)); // 1.5 seconds busy
        }
    }
    
    void DrinkRoadBeer()
    {
        if (roadBeersRemaining <= 0)
        {
            Debug.Log("❌ Out of road beers!");
            return;
        }
        
        roadBeersRemaining--;
        Debug.Log($"🍺 Drinking road beer... ({roadBeersRemaining} left)");
        
        // Play video
        if (videoOverlay != null)
        {
            videoOverlay.PlayBeerVideo();
            StartCoroutine(ActivityCooldown(1.5f)); // 1.5 seconds busy
        }
    }
    
    void SmokeCigarette()
    {
        if (cigarettesRemaining <= 0)
        {
            Debug.Log("❌ Out of cigarettes!");
            return;
        }
        
        cigarettesRemaining--;
        Debug.Log($"🚬 Smoking cigarette... ({cigarettesRemaining} left)");
        
        // Play video
        if (videoOverlay != null)
        {
            videoOverlay.PlaySmokingVideo();
            StartCoroutine(ActivityCooldown(3f)); // 3 seconds busy
        }
    }
    
    void HonkHorn()
    {
        Debug.Log("📯 HONK HONK!");
    }
    
    System.Collections.IEnumerator ActivityCooldown(float duration)
    {
        isBusy = true;
        yield return new WaitForSeconds(duration);
        isBusy = false;
        Debug.Log("✅ Activity finished!");
    }
    
    // Public getters
    public int GetHotdogsRemaining() => hotdogsRemaining;
    public int GetBigGulpsRemaining() => bigGulpsRemaining;
    public int GetRoadBeersRemaining() => roadBeersRemaining;
    public int GetCigarettesRemaining() => cigarettesRemaining;
}