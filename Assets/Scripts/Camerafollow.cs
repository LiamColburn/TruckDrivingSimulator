using UnityEngine;

/// <summary>
/// Simple camera that stays behind the cube/truck.
/// Also handles idle bob, lane-change tilt, and road-beer FOV pulse
/// as additive effects applied after the base follow lerp.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // The cube/truck to follow

    [Header("Camera Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, 0f); // Position relative to target
    // Y = height above, Z = distance behind

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothSpeed = 5f; // How fast camera catches up (higher = snappier)

    [Header("Idle Bob")]
    [SerializeField] private float bobSpeed = 1.2f;
    [SerializeField] private float bobY     = 0.03f;
    [SerializeField] private float bobX     = 0.01f;

    [Header("Lane Tilt")]
    [SerializeField] private float tiltAngle = 2.5f;
    [SerializeField] private float tiltSpeed = 4f;

    [Header("Road Beer FOV Pulse")]
    [SerializeField] private float fovPulseAmount    = 5f;
    [SerializeField] private float fovPulseSpeed     = 0.65f;
    [SerializeField] private float fovEffectDuration = 5f;

    private Camera     cam;
    private Quaternion baseRotation;
    private float      normalFOV;

    private float   currentTilt    = 0f;
    private float   targetTilt     = 0f;
    private bool    fovActive      = false;
    private float   fovElapsed     = 0f;
    private Vector3 followedPosition;  // lerp result before bob — keeps bob from drifting into the lerp

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

        cam          = GetComponent<Camera>();
        baseRotation = transform.rotation;
        if (cam != null) normalFOV = cam.fieldOfView;

        // Set initial position immediately (no lerp on first frame)
        followedPosition   = target.position + offset;
        transform.position = followedPosition;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Base follow — lerp from the clean followed position (no bob in the from-value)
        Vector3 desiredPosition = target.position + offset;
        followedPosition = Vector3.Lerp(
            followedPosition,
            desiredPosition,
            positionSmoothSpeed * Time.deltaTime
        );

        // Bob and tilt are pure deltas applied on top — followedPosition is never contaminated
        ApplyBob();
        ApplyTilt();
        ApplyFOV();
    }
    
    // ── Polish effects (public API — signatures match deleted CameraController) ─

    /// <summary>direction: -1 = turning left, 1 = turning right</summary>
    public void TriggerLaneTilt(int direction)
    {
        targetTilt = direction * tiltAngle;
    }

    public void TriggerRoadBeerEffect()
    {
        fovActive  = true;
        fovElapsed = 0f;
    }

    // ── Internal effect helpers ───────────────────────────────────────────────

    void ApplyBob()
    {
        float t = Time.time * bobSpeed;
        transform.position = followedPosition + new Vector3(
            Mathf.Sin(t * 0.65f) * bobX,
            Mathf.Sin(t)         * bobY,
            0f
        );
    }

    void ApplyTilt()
    {
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        targetTilt  = Mathf.Lerp(targetTilt,  0f,         Time.deltaTime * tiltSpeed * 0.45f);
        transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, currentTilt);
    }

    void ApplyFOV()
    {
        if (!fovActive || cam == null) return;

        fovElapsed += Time.deltaTime;
        float fade  = 1f - Mathf.Clamp01(fovElapsed / fovEffectDuration);
        float pulse = Mathf.Sin(fovElapsed * fovPulseSpeed * Mathf.PI * 2f) * fovPulseAmount * fade;
        cam.fieldOfView = normalFOV + pulse;

        if (fovElapsed >= fovEffectDuration)
        {
            fovActive = false;
            cam.fieldOfView = normalFOV;
        }
    }

    // ── Runtime adjustment helpers ────────────────────────────────────────────

    public void SetOffset(Vector3 newOffset)      => offset              = newOffset;
    public void SetSmoothSpeed(float newSpeed)    => positionSmoothSpeed = newSpeed;
    public void SetTarget(Transform newTarget)    => target              = newTarget;
}