using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks score - distance traveled and time survived
/// </summary>
public class ScoreTracker : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private float pointsPerSecond = 10f;
    [SerializeField] private float distanceMultiplier = 1f;
    
    [Header("UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text distanceText;
    [SerializeField] private Text timeText;
    
    private float score = 0f;
    private float distanceTraveled = 0f;
    private float timeSurvived = 0f;
    private Vector3 lastPosition;
    private bool isPlaying = true;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!isPlaying)
            return;
        
        // Track time
        timeSurvived += Time.deltaTime;
        
        // Track distance
        float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);
        distanceTraveled += distanceThisFrame;
        lastPosition = transform.position;
        
        // Calculate score
        score = (timeSurvived * pointsPerSecond) + (distanceTraveled * distanceMultiplier);
        
        // Update UI
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {Mathf.FloorToInt(score)}";
        }
        
        if (distanceText != null)
        {
            distanceText.text = $"Distance: {Mathf.FloorToInt(distanceTraveled)}m";
        }
        
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeSurvived / 60f);
            int seconds = Mathf.FloorToInt(timeSurvived % 60f);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    public void StopTracking()
    {
        isPlaying = false;
    }

    // Getters
    public float GetScore() => score;
    public float GetDistance() => distanceTraveled;
    public float GetTime() => timeSurvived;
}