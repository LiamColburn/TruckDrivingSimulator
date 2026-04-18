using UnityEngine;
using UnityEngine.SceneManagement;
 
/// <summary>
/// Simple collision detection - hit a car = game over
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
    }
 
    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit another vehicle
        if (collision.gameObject.CompareTag("Traffic") || 
            collision.gameObject.CompareTag("Vehicle") ||
            collision.gameObject.name.Contains("Car") ||
            collision.gameObject.name.Contains("Truck"))
        {
            if (!hasCrashed)
            {
                Crash();
            }
        }
    }
 
    void Crash()
    {
        hasCrashed = true;
        crashCount++;
        
        Debug.Log($"CRASH! You hit a car! Total crashes: {crashCount}");
        
        // Play crash sound
        if (crashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(crashSound);
        }
        
        // Spawn explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // Disable player control
        TruckPlayerController playerController = GetComponent<TruckPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
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
        // Reset truck position
        transform.position = new Vector3(0, 2, 0);
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
