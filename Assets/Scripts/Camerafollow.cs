using UnityEngine;

/// <summary>
/// Simple camera that stays in front of the cube/truck
/// Fixed straight view, not affected by collisions or movement
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // The cube/truck to follow
    
    [Header("Camera Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, 0f); // Position relative to target
    // Y = height above, Z = distance in front
    
    [Header("Smoothing")]
    [SerializeField] private float positionSmoothSpeed = 5f; // How fast camera catches up (higher = snappier)
    
    void Start()
    {
        // Auto-find target if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log($"CameraFollow: Auto-found target '{player.name}'");
            }
            else
            {
                Debug.LogError("CameraFollow: No target assigned and no object with 'Player' tag found!");
                enabled = false;
                return;
            }
        }
        
        // Set initial position immediately (no lerp on first frame)
        transform.position = target.position + offset;
    }
    
    void LateUpdate()
    {
        if (target == null)
            return;
        
        // Calculate desired position (follows cube but maintains offset)
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move to desired position
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmoothSpeed * Time.deltaTime
        );
        
        // Rotation is now controlled manually in the Inspector - no forced rotation
    }
    
    // Public methods for runtime adjustments
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    public void SetSmoothSpeed(float newSpeed)
    {
        positionSmoothSpeed = newSpeed;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}