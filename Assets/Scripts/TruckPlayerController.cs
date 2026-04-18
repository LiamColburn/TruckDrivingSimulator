using UnityEngine;

/// <summary>
/// Truck player controller - Change lanes, eat hotdogs, drink Big Gulps, road beers, and honk
/// Because that's what trucking is all about
/// </summary>
public class TruckPlayerController : MonoBehaviour
{
    [Header("Lane Changing")]
    private float laneWidth = 3.5f;
    private float laneChangeSpeed = 5f;
    private int currentLane = 1; // 0 = left, 1 = center, 2 = right
    private int totalLanes = 3;
    
    [Header("Movement")]
    private float forwardSpeed = 20f;
    private bool autoForward = true;
    
    [Header("Trucker Activities")]
    private int hotdogsRemaining = 10;
    private int bigGulpsRemaining = 5;
    private int roadBeersRemaining = 6;
    private float eatDuration = 2f;
    private float drinkDuration = 1.5f;
    
    [Header("Audio")]
    private AudioSource hornAudioSource;
    private AudioClip hornSound;
    private AudioClip eatSound;
    private AudioClip drinkSound;
    private AudioClip burpSound;
    
    // UI Reference (found at runtime)
    private TruckerUI truckerUI;
    
    // Internal state
    private float targetLanePosition;
    private bool isEating = false;
    private bool isDrinking = false;
    private float activityTimer = 0f;
    
    // Stats
    private int hotdogsEaten = 0;
    private int bigGulpsDrank = 0;
    private int roadBeersDrank = 0;
    private int hornsHonked = 0;
    
    void Start()
    {
        // Initialize lane position
        targetLanePosition = GetLanePosition(currentLane);
        
        // Setup audio if not assigned
        if (hornAudioSource == null)
        {
            hornAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find TruckerUI if not assigned
        if (truckerUI == null)
        {
            truckerUI = FindObjectOfType<TruckerUI>();
        }
        
        // Update UI
        UpdateUI();
        
        Debug.Log("Truck driver ready! Controls: A/D = Change Lanes, H = Hotdog, G = Big Gulp, B = Road Beer, Space = Horn");
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
        // Can't do anything while eating or drinking
        if (isEating || isDrinking)
            return;
        
        // Lane changing
        if (Input.GetKeyDown(KeyCode.A))
        {
            ChangeLane(-1); // Left
        }
        else if (Input.GetKeyDown(KeyCode.D))
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
        
        // Horn (can honk while eating/drinking because one hand is free)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HonkHorn();
        }
    }
 
    void ChangeLane(int direction)
    {
        int newLane = currentLane + direction;
        
        // Check bounds
        if (newLane < 0 || newLane >= totalLanes)
        {
            Debug.Log("Can't change lanes - already at edge!");
            return;
        }
        
        currentLane = newLane;
        targetLanePosition = GetLanePosition(currentLane);
        
        string[] laneNames = new string[] { "left", "center", "right" };
        Debug.Log($"Changing to {laneNames[currentLane]} lane");
    }
 
    float GetLanePosition(int lane)
    {
        // Calculate lateral position based on lane
        // Assuming center lane is at x=0
        float centerOffset = (totalLanes - 1) * laneWidth / 2f;
        return (lane * laneWidth) - centerOffset;
    }
 
    void HandleLaneMovement()
    {
        // Smoothly move to target lane position
        Vector3 currentPos = transform.position;
        float newX = Mathf.Lerp(currentPos.x, targetLanePosition, Time.deltaTime * laneChangeSpeed);
        transform.position = new Vector3(newX, currentPos.y, currentPos.z);
    }
 
    void HandleForwardMovement()
    {
        if (!autoForward)
            return;
        
        // Move forward automatically
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
        UpdateUI();
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
        UpdateUI();
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
        
        // Road beers make you drive slightly wonky (optional effect)
        StartCoroutine(RoadBeerEffect());
        
        UpdateUI();
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
            // Fallback beep if no sound assigned
            Debug.Log("*HORN SOUND*");
        }
        
        UpdateUI();
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
        
        Vector3 originalLanePos = transform.position;
        
        while (elapsed < wobbleDuration)
        {
            elapsed += Time.deltaTime;
            
            // Add slight sine wave wobble to X position
            float wobble = Mathf.Sin(elapsed * 3f) * wobbleAmount;
            Vector3 pos = transform.position;
            pos.x = targetLanePosition + wobble;
            transform.position = pos;
            
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
 
    void UpdateUI()
    {
        if (truckerUI != null)
        {
            truckerUI.UpdateInventory(hotdogsRemaining, bigGulpsRemaining, roadBeersRemaining);
            truckerUI.UpdateStats(hotdogsEaten, bigGulpsDrank, roadBeersDrank, hornsHonked);
        }
    }
 
    // Public getters for other scripts
    public int GetCurrentLane() => currentLane;
    public bool IsEating() => isEating;
    public bool IsDrinking() => isDrinking;
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
        UpdateUI();
    }
 
    void OnDrawGizmos()
    {
        // Visualize lanes in editor
        if (!Application.isPlaying)
            return;
        
        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < totalLanes; i++)
        {
            float lanePos = GetLanePosition(i);
            Vector3 start = new Vector3(lanePos, transform.position.y, transform.position.z - 50f);
            Vector3 end = new Vector3(lanePos, transform.position.y, transform.position.z + 50f);
            
            Gizmos.DrawLine(start, end);
        }
        
        // Highlight current lane
        Gizmos.color = Color.green;
        Vector3 currentLaneStart = new Vector3(targetLanePosition, transform.position.y, transform.position.z - 10f);
        Vector3 currentLaneEnd = new Vector3(targetLanePosition, transform.position.y, transform.position.z + 10f);
        Gizmos.DrawLine(currentLaneStart, currentLaneEnd);
    }
}