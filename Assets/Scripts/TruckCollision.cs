using UnityEngine;
using UnityEngine.SceneManagement;
 
/// <summary>
/// Collision detection - hit a vehicle = game over
/// Works with "Vehicle" tag
/// </summary>
public class TruckCollision : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private bool enableGameOver = true;
    [SerializeField] private float respawnDelay = 2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip crashSound;
    
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    
    private AudioSource audioSource;
    private bool hasCrashed = false;
    private int crashCount = 0;
    private TrafficSpawner trafficSpawner;
 
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Find traffic spawner (to remove crashed vehicles from active list)
        trafficSpawner = FindFirstObjectByType<TrafficSpawner>();
    }
 
    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit a vehicle (using your "Vehicle" tag)
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            if (!hasCrashed)
            {
                Crash(collision.gameObject);
            }
        }
    }
 
    void Crash(GameObject hitVehicle)
    {
        hasCrashed = true;
        crashCount++;
        
        Debug.Log($"CRASH! You hit {hitVehicle.name}! Total crashes: {crashCount}");
        
        // Play crash sound
        if (crashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(crashSound);
        }
        
        // Spawn explosion effect at collision point
        if (explosionEffect != null)
        {
            Vector3 explosionPos = (transform.position + hitVehicle.transform.position) / 2f;
            Instantiate(explosionEffect, explosionPos, Quaternion.identity);
        }
        
        // Remove the hit vehicle from traffic system
        if (trafficSpawner != null)
        {
            trafficSpawner.RemoveVehicle(hitVehicle);
        }
        
        // Destroy the vehicle we hit
        Destroy(hitVehicle);
        
        // Disable player control
        TruckPlayerController playerController = GetComponent<TruckPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Stop score tracking
        ScoreTracker scoreTracker = GetComponent<ScoreTracker>();
        if (scoreTracker != null)
        {
            scoreTracker.StopTracking();
        }
        
        if (enableGameOver)
        {
            ShowGameOver();
        }
        else
        {
            // Just respawn after delay
            Invoke("Respawn", respawnDelay);
        }
    }
 
    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Pause the game
        Time.timeScale = 0f;
        
        Debug.Log("GAME OVER! Press R to restart");
    }
 
    void Respawn()
    {
        // Reset truck position to center lane
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;
        
        // Re-enable player control
        TruckPlayerController playerController = GetComponent<TruckPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        hasCrashed = false;
        
        Debug.Log("Respawned! Be more careful!");
    }
 
    void Update()
    {
        // Press R to restart after game over
        if (hasCrashed && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }
 
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
 
    // Public methods for UI buttons
    public void OnRestartButton()
    {
        RestartGame();
    }
 
    public void OnQuitButton()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
 
    // Getters
    public int GetCrashCount() => crashCount;
    public bool HasCrashed() => hasCrashed;
}