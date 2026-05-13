// using UnityEngine;
// using UnityEngine.InputSystem;

// /// <summary>
// /// Truck controller with video playback for activities
// /// Lane changing + eating/drinking/smoking with video overlays
// /// </summary>
// public class TruckPlayerController : MonoBehaviour
// {
//     [Header("Lane Changing")]
//     [SerializeField] private float[] laneXPositions = new float[] { -4.66f, 0f, 4.66f };
//     [SerializeField] private float laneChangeDuration = 2.5f;
//     private int currentLane = 1;
//     private int targetLane = 1;
//     private bool isChangingLanes = false;
//     private float laneChangeProgress = 1f;
//     private float startX;
//     private float targetX;
    
//     [Header("Trucker Activities")]
//     [SerializeField] private int hotdogsRemaining = 10;
//     [SerializeField] private int bigGulpsRemaining = 5;
//     [SerializeField] private int roadBeersRemaining = 6;
//     [SerializeField] private int cigarettesRemaining = 20;
    
//     [Header("References")]
//     [SerializeField] private TruckerVideoOverlay videoOverlay;
    
//     private bool isBusy = false; // Eating, drinking, or smoking
    
//     void Start()
//     {
//         // Set starting position
//         Vector3 pos = transform.position;
//         pos.x = laneXPositions[currentLane];
//         transform.position = pos;
        
//         // Find video overlay if not assigned
//         if (videoOverlay == null)
//         {
//             videoOverlay = FindFirstObjectByType<TruckerVideoOverlay>();
//         }
        
//         Debug.Log("=== TRUCK DRIVER READY ===");
//         Debug.Log("Controls:");
//         Debug.Log("  A/D or Arrows = Change Lanes");
//         Debug.Log("  H = Hotdog");
//         Debug.Log("  G = Big Gulp");
//         Debug.Log("  B = Road Beer");
//         Debug.Log("  C = Cigarette");
//         Debug.Log("  Space = Horn");
//     }
    
//     void Update()
//     {
//         HandleInput();
        
//         if (isChangingLanes)
//         {
//             UpdateLaneChange();
//         }
//     }
    
//     void HandleInput()
//     {
//         var keyboard = Keyboard.current;
//         if (keyboard == null) return;
        
//         // Lane changing (only when not busy)
//         if (!isChangingLanes && !isBusy)
//         {
//             if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
//             {
//                 ChangeLane(-1);
//             }
//             else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
//             {
//                 ChangeLane(1);
//             }
//         }
        
//         // Activities (only when not busy and not changing lanes)
//         if (!isBusy && !isChangingLanes)
//         {
//             if (keyboard.hKey.wasPressedThisFrame)
//             {
//                 EatHotdog();
//             }
//             else if (keyboard.gKey.wasPressedThisFrame)
//             {
//                 DrinkBigGulp();
//             }
//             else if (keyboard.bKey.wasPressedThisFrame)
//             {
//                 DrinkRoadBeer();
//             }
//             else if (keyboard.cKey.wasPressedThisFrame)
//             {
//                 SmokeCigarette();
//             }
//         }
        
//         // Horn (can always honk)
//         if (keyboard.spaceKey.wasPressedThisFrame)
//         {
//             HonkHorn();
//         }
//     }
    
//     void ChangeLane(int direction)
//     {
//         int newLane = currentLane + direction;
        
//         if (newLane < 0 || newLane >= laneXPositions.Length)
//         {
//             Debug.Log("❌ Can't change lanes - at edge!");
//             return;
//         }
        
//         targetLane = newLane;
//         isChangingLanes = true;
//         laneChangeProgress = 0f;
//         startX = transform.position.x;
//         targetX = laneXPositions[targetLane];
        
//         string[] names = { "LEFT", "CENTER", "RIGHT" };
//         Debug.Log($"🚗 Changing to {names[targetLane]} lane");
//     }
    
//     void UpdateLaneChange()
//     {
//         laneChangeProgress += Time.deltaTime / laneChangeDuration;
        
//         if (laneChangeProgress >= 1f)
//         {
//             laneChangeProgress = 1f;
//             isChangingLanes = false;
//             currentLane = targetLane;
            
//             Vector3 finalPos = transform.position;
//             finalPos.x = laneXPositions[targetLane];
//             transform.position = finalPos;
            
//             Debug.Log($"✅ Lane change complete!");
//             return;
//         }
        
//         float t = Mathf.SmoothStep(0f, 1f, laneChangeProgress);
//         float newX = Mathf.Lerp(startX, targetX, t);
        
//         Vector3 newPos = transform.position;
//         newPos.x = newX;
//         transform.position = newPos;
//     }
    
//     void EatHotdog()
//     {
//         if (hotdogsRemaining <= 0)
//         {
//             Debug.Log("❌ Out of hotdogs!");
//             return;
//         }
        
//         hotdogsRemaining--;
//         Debug.Log($"🌭 Eating hotdog... ({hotdogsRemaining} left)");
        
//         // Play video
//         if (videoOverlay != null)
//         {
//             videoOverlay.PlayHotdogVideo();
//             StartCoroutine(ActivityCooldown(2f)); // 2 seconds busy
//         }
//     }
    
//     void DrinkBigGulp()
//     {
//         if (bigGulpsRemaining <= 0)
//         {
//             Debug.Log("❌ Out of Big Gulps!");
//             return;
//         }
        
//         bigGulpsRemaining--;
//         Debug.Log($"🥤 Drinking Big Gulp... ({bigGulpsRemaining} left)");
        
//         // Play video
//         if (videoOverlay != null)
//         {
//             videoOverlay.PlayBigGulpVideo();
//             StartCoroutine(ActivityCooldown(1.5f)); // 1.5 seconds busy
//         }
//     }
    
//     void DrinkRoadBeer()
//     {
//         if (roadBeersRemaining <= 0)
//         {
//             Debug.Log("❌ Out of road beers!");
//             return;
//         }
        
//         roadBeersRemaining--;
//         Debug.Log($"🍺 Drinking road beer... ({roadBeersRemaining} left)");
        
//         // Play video
//         if (videoOverlay != null)
//         {
//             videoOverlay.PlayBeerVideo();
//             StartCoroutine(ActivityCooldown(1.5f)); // 1.5 seconds busy
//         }
//     }
    
//     void SmokeCigarette()
//     {
//         if (cigarettesRemaining <= 0)
//         {
//             Debug.Log("❌ Out of cigarettes!");
//             return;
//         }
        
//         cigarettesRemaining--;
//         Debug.Log($"🚬 Smoking cigarette... ({cigarettesRemaining} left)");
        
//         // Play video
//         if (videoOverlay != null)
//         {
//             videoOverlay.PlaySmokingVideo();
//             StartCoroutine(ActivityCooldown(3f)); // 3 seconds busy
//         }
//     }
    
//     void HonkHorn()
//     {
//         Debug.Log("📯 HONK HONK!");
//     }
    
//     System.Collections.IEnumerator ActivityCooldown(float duration)
//     {
//         isBusy = true;
//         yield return new WaitForSeconds(duration);
//         isBusy = false;
//         Debug.Log("✅ Activity finished!");
//     }
    
//     // Public getters
//     public int GetHotdogsRemaining() => hotdogsRemaining;
//     public int GetBigGulpsRemaining() => bigGulpsRemaining;
//     public int GetRoadBeersRemaining() => roadBeersRemaining;
//     public int GetCigarettesRemaining() => cigarettesRemaining;
// }

using UnityEngine;

/// <summary>
/// Truck player controller - Change lanes, eat hotdogs, drink Big Gulps, road beers, road cigs, and honk
/// Now with smooth 2-3 second lane transitions and proper input blocking
/// </summary>
public class TruckPlayerController : MonoBehaviour
{
    [Header("Lane Changing")]
    [SerializeField] private float[] laneXPositions = new float[] { -4.66f, 0f, 4.66f }; // Must match TrafficSpawner!
    [SerializeField] private float laneChangeDuration = 2.5f; // 2-3 seconds
    private int currentLane = 1; // 0 = left, 1 = center, 2 = right
    private int targetLane = 1;
    private float laneChangeProgress = 1f; // 1 = finished, 0 = just started
    private Vector3 laneChangeStartPos;
    private Vector3 laneChangeTargetPos;
    
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 0f; // Set to 0 - we're stationary, world moves
    [SerializeField] private bool autoForward = false; // Truck stays still, traffic moves backward
    
    [Header("Trucker Activities")]
    [SerializeField] private int hotdogsRemaining = 10;
    [SerializeField] private int bigGulpsRemaining = 5;
    [SerializeField] private int roadBeersRemaining = 6;
    [SerializeField] private int cigarettesRemaining = 20;
    [SerializeField] private float eatDuration = 2f;
    [SerializeField] private float drinkDuration = 1.5f;
    [SerializeField] private float smokeDuration = 3f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip hornSound;
    [SerializeField] private AudioClip eatSound;
    [SerializeField] private AudioClip drinkSound;
    [SerializeField] private AudioClip burpSound;

    [Header("UI & Effects")]
    [SerializeField] private TruckerUI truckerUI;
    [SerializeField] private ActivityParticles activityParticles;
    [SerializeField] private TruckerDash dashOverlay;
    [SerializeField] private TruckerVideoOverlay videoOverlay;

    private AudioSource hornAudioSource;
    
    // Internal state
    private bool isEating = false;
    private bool isDrinking = false;
    private bool isSmoking = false;
    private float activityTimer = 0f;
    private bool isChangingLanes = false;
    
    // Stats
    private int hotdogsEaten = 0;
    private int bigGulpsDrank = 0;
    private int roadBeersDrank = 0;
    private int cigarettesSmoked = 0;
    private int hornsHonked = 0;
    
    void Start()
    {
        // Initialize lane position
        transform.position = new Vector3(laneXPositions[currentLane], transform.position.y, transform.position.z);
        
        // Setup audio if not assigned
        if (hornAudioSource == null)
        {
            hornAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (truckerUI        == null) truckerUI        = FindFirstObjectByType<TruckerUI>();
        if (activityParticles == null) activityParticles = GetComponentInChildren<ActivityParticles>();
        if (dashOverlay == null) dashOverlay = FindFirstObjectByType<TruckerDash>();
        if (videoOverlay == null) videoOverlay = FindFirstObjectByType<TruckerVideoOverlay>();

        Debug.Log("Truck driver ready! Controls: A/D or Arrow Keys = Change Lanes, H = Hotdog, G = Big Gulp, B = Road Beer, C = Cigarette, Space = Horn");
    }
 
    void Update()
    {
        HandleInput();
        HandleLaneMovement();
        HandleForwardMovement();
        HandleActivities();
    }
 
    void HandleInput()
    {
        // Can't do anything while eating, drinking, smoking, or changing lanes
        if (isEating || isDrinking || isSmoking || isChangingLanes)
            return;
        
        // Lane changing - A/D or Arrow Keys
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1); // Left
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1); // Right
        }
        
        // Trucker activities
        if (Input.GetKeyDown(KeyCode.H))
        {
            EatHotdog();
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            DrinkBigGulp();
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            DrinkRoadBeer();
        } 
        else if (Input.GetKeyDown(KeyCode.C))
        {
            SmokeCigarette();
        }
        
        // Horn (can honk anytime - one hand is always free)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HonkHorn();
        }
    }
 
    void ChangeLane(int direction)
    {
        int newLane = targetLane + direction;
        
        // Check bounds
        if (newLane < 0 || newLane >= laneXPositions.Length)
        {
            Debug.Log("Can't change lanes - already at edge!");
            return;
        }
        
        // Start lane change
        targetLane = newLane;
        isChangingLanes = true;
        laneChangeProgress = 0f;
        
        laneChangeStartPos = transform.position;
        laneChangeTargetPos = new Vector3(laneXPositions[targetLane], transform.position.y, transform.position.z);

        dashOverlay?.OnLaneChange(direction);

        string[] laneNames = new string[] { "left", "center", "right" };
        Debug.Log($"Changing to {laneNames[targetLane]} lane...");
    }
 
    void HandleLaneMovement()
    {
        if (!isChangingLanes)
            return;
        
        // Increment progress (0 to 1 over laneChangeDuration)
        laneChangeProgress += Time.deltaTime / laneChangeDuration;
        
        if (laneChangeProgress >= 1f)
        {
            // Lane change complete
            laneChangeProgress = 1f;
            isChangingLanes = false;
            currentLane = targetLane;
            Debug.Log("Lane change complete!");
        }
        
        // Smooth interpolation (ease in/out)
        float t = Mathf.SmoothStep(0f, 1f, laneChangeProgress);
        
        // Update position (only X changes, keep Y and Z)
        Vector3 newPos = Vector3.Lerp(laneChangeStartPos, laneChangeTargetPos, t);
        transform.position = new Vector3(newPos.x, transform.position.y, transform.position.z);
    }
 
    void HandleForwardMovement()
    {
        if (!autoForward)
            return;
        
        // Move forward automatically (if enabled)
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }

    void SmokeCigarette()
    {
        if (cigarettesRemaining <= 0)
        {
            Debug.Log("Out of cigarettes! Time to quit.");
            return;
        }
        
        if (isEating || isDrinking || isSmoking)
        {
            Debug.Log("Already got your hands full!");
            return;
        }
        
        cigarettesRemaining--;
        cigarettesSmoked++;
        isSmoking = true;
        activityTimer = smokeDuration;
        
        Debug.Log($"Lightin' up a cigarette... ({cigarettesRemaining} left)");

        truckerUI?.ShowSmoking(smokeDuration);
        activityParticles?.SpawnSmokePuff();
        AudioManager.Instance?.PlaySmoke();
        videoOverlay?.PlaySmokingVideo();
    }
 
    void EatHotdog()
    {
        if (hotdogsRemaining <= 0)
        {
            Debug.Log("Out of hotdogs! You should've packed more.");
            return;
        }
        
        if (isEating || isDrinking || isSmoking)
        {
            Debug.Log("Already got your hands full!");
            return;
        }
        
        hotdogsRemaining--;
        hotdogsEaten++;
        isEating = true;
        activityTimer = eatDuration;
        
        Debug.Log($"Munchin' on a hotdog... ({hotdogsRemaining} left)");

        PlaySound(eatSound);
        truckerUI?.ShowEating(eatDuration);
        activityParticles?.SpawnEatBurst();
        AudioManager.Instance?.PlayEat();
        videoOverlay?.PlayHotdogVideo();
    }
 
    void DrinkBigGulp()
    {
        if (bigGulpsRemaining <= 0)
        {
            Debug.Log("Out of Big Gulps! Gotta stop at the next gas station.");
            return;
        }
        
        if (isEating || isDrinking || isSmoking)
        {
            Debug.Log("Already got your hands full!");
            return;
        }
        
        bigGulpsRemaining--;
        bigGulpsDrank++;
        isDrinking = true;
        activityTimer = drinkDuration;
        
        Debug.Log($"Sippin' on a Big Gulp... ({bigGulpsRemaining} left)");

        PlaySound(drinkSound);
        truckerUI?.ShowDrinking(drinkDuration);
        activityParticles?.SpawnDrinkBurst();
        AudioManager.Instance?.PlayDrink();
        videoOverlay?.PlayBigGulpVideo();
    }
 
    void DrinkRoadBeer()
    {
        if (roadBeersRemaining <= 0)
        {
            Debug.Log("Out of road beers! That's probably for the best.");
            return;
        }
        
        if (isEating || isDrinking || isSmoking)
        {
            Debug.Log("Already got your hands full!");
            return;
        }
        
        roadBeersRemaining--;
        roadBeersDrank++;
        isDrinking = true;
        activityTimer = drinkDuration;
        
        Debug.Log($"Crackin' a cold one... ({roadBeersRemaining} left)");

        PlaySound(drinkSound);
        truckerUI?.ShowRoadBeer(drinkDuration);
        activityParticles?.SpawnDrinkBurst();
        videoOverlay?.PlayBeerVideo();
        AudioManager.Instance?.PlayDrink();

        // Road beers make you drive slightly wonky (optional effect)
        StartCoroutine(RoadBeerEffect());
    }
 
    void HonkHorn()
    {
        hornsHonked++;

        Debug.Log("HONK HONK! 🚚");

        if (hornSound != null && hornAudioSource != null)
        {
            hornAudioSource.PlayOneShot(hornSound);
        }
        else
        {
            Debug.Log("*HORN SOUND*");
        }

        truckerUI?.ShowHonk();
        activityParticles?.SpawnHonkPuff();
        AudioManager.Instance?.PlayHorn();
    }
 
    void HandleActivities()
    {
        if (isEating || isDrinking || isSmoking)
        {
            activityTimer -= Time.deltaTime;
            
            if (activityTimer <= 0)
            {
                // Finish eating/drinking/smoking
                if (isEating)
                {
                    Debug.Log("Finished that hotdog. Delicious!");
                    isEating = false;
                }
                else if (isDrinking)
                {
                    Debug.Log("*BURP* That hit the spot.");
                    PlaySound(burpSound);
                    AudioManager.Instance?.PlayBurp();
                    isDrinking = false;
                }
                else if (isSmoking)
                {
                    Debug.Log("*Exhales smoke* That's the stuff.");
                    isSmoking = false;
                }
            }
        }
    }
 
    System.Collections.IEnumerator RoadBeerEffect()
    {
        // Slight lane wobble effect after road beer
        float wobbleDuration = 5f;
        float wobbleAmount = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < wobbleDuration)
        {
            elapsed += Time.deltaTime;
            
            // Only wobble if not actively changing lanes
            if (!isChangingLanes)
            {
                // Add slight sine wave wobble to X position
                float wobble = Mathf.Sin(elapsed * 3f) * wobbleAmount;
                Vector3 pos = transform.position;
                pos.x = laneXPositions[currentLane] + wobble;
                transform.position = pos;
            }
            
            yield return null;
        }
    }
 
    void PlaySound(AudioClip clip)
    {
        if (clip != null && hornAudioSource != null)
        {
            hornAudioSource.PlayOneShot(clip);
        }
    }

 
    // Public getters for other scripts
    public int GetCurrentLane() => currentLane;
    public bool IsEating() => isEating;
    public bool IsDrinking() => isDrinking;
    public bool IsSmoking() => isSmoking;
    public bool IsChangingLanes() => isChangingLanes;
    public int GetHotdogsRemaining() => hotdogsRemaining;
    public int GetBigGulpsRemaining() => bigGulpsRemaining;
    public int GetRoadBeersRemaining() => roadBeersRemaining;
    public int GetCigarettesRemaining() => cigarettesRemaining;
 
    // Restock at gas stations
    public void Restock()
    {
        hotdogsRemaining = 10;
        bigGulpsRemaining = 5;
        roadBeersRemaining = 6;
        cigarettesRemaining = 20;
        
        Debug.Log("Restocked at the gas station!");
        //UpdateUI();
    }
 
    void OnDrawGizmos()
    {
        // Visualize lanes in editor
        if (!Application.isPlaying)
            return;
        
        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < laneXPositions.Length; i++)
        {
            float lanePos = laneXPositions[i];
            Vector3 start = new Vector3(lanePos, transform.position.y, transform.position.z - 50f);
            Vector3 end = new Vector3(lanePos, transform.position.y, transform.position.z + 50f);
            
            Gizmos.DrawLine(start, end);
        }
        
        // Highlight current/target lane
        if (isChangingLanes)
        {
            // Show both current and target
            Gizmos.color = Color.red;
            Vector3 targetLaneStart = new Vector3(laneXPositions[targetLane], transform.position.y, transform.position.z - 10f);
            Vector3 targetLaneEnd = new Vector3(laneXPositions[targetLane], transform.position.y, transform.position.z + 10f);
            Gizmos.DrawLine(targetLaneStart, targetLaneEnd);
        }
        else
        {
            Gizmos.color = Color.green;
            Vector3 currentLaneStart = new Vector3(laneXPositions[currentLane], transform.position.y, transform.position.z - 10f);
            Vector3 currentLaneEnd = new Vector3(laneXPositions[currentLane], transform.position.y, transform.position.z + 10f);
            Gizmos.DrawLine(currentLaneStart, currentLaneEnd);
        }
    }
}