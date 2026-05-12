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
    [SerializeField] private float eatDuration = 2f;
    [SerializeField] private float drinkDuration = 1.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip hornSound;
    [SerializeField] private AudioClip eatSound;
    [SerializeField] private AudioClip drinkSound;
    [SerializeField] private AudioClip burpSound;

    [Header("UI & Effects")]
    [SerializeField] private TruckerUI truckerUI;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ActivityParticles activityParticles;

    private AudioSource hornAudioSource;
    
    // Internal state
    private bool isEating = false;
    private bool isDrinking = false;
    private float activityTimer = 0f;
    private bool isChangingLanes = false;
    
    // Stats
    private int hotdogsEaten = 0;
    private int bigGulpsDrank = 0;
    private int roadBeersDrank = 0;
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
        if (cameraController == null) cameraController = FindFirstObjectByType<CameraController>();
        if (activityParticles == null) activityParticles = GetComponentInChildren<ActivityParticles>();

        Debug.Log("Truck driver ready! Controls: A/D or Arrow Keys = Change Lanes, H = Hotdog, G = Big Gulp, B = Road Beer, Space = Horn");
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
        // Can't do anything while eating, drinking, or changing lanes
        if (isEating || isDrinking || isChangingLanes)
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

        cameraController?.TriggerLaneTilt(direction);

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
 
    void EatHotdog()
    {
        if (hotdogsRemaining <= 0)
        {
            Debug.Log("Out of hotdogs! You should've packed more.");
            return;
        }
        
        if (isEating || isDrinking)
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
    }
 
    void DrinkBigGulp()
    {
        if (bigGulpsRemaining <= 0)
        {
            Debug.Log("Out of Big Gulps! Gotta stop at the next gas station.");
            return;
        }
        
        if (isEating || isDrinking)
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
    }
 
    void DrinkRoadBeer()
    {
        if (roadBeersRemaining <= 0)
        {
            Debug.Log("Out of road beers! That's probably for the best.");
            return;
        }
        
        if (isEating || isDrinking)
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
        cameraController?.TriggerRoadBeerEffect();
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
        if (isEating || isDrinking)
        {
            activityTimer -= Time.deltaTime;
            
            if (activityTimer <= 0)
            {
                // Finish eating/drinking
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
    public bool IsChangingLanes() => isChangingLanes;
    public int GetHotdogsRemaining() => hotdogsRemaining;
    public int GetBigGulpsRemaining() => bigGulpsRemaining;
    public int GetRoadBeersRemaining() => roadBeersRemaining;
 
    // Restock at gas stations
    public void Restock()
    {
        hotdogsRemaining = 10;
        bigGulpsRemaining = 5;
        roadBeersRemaining = 6;
        
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