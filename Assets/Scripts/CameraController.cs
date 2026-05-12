using UnityEngine;

/// <summary>
/// Attach to the Camera GameObject (which should be a child of the truck).
/// Provides: idle driving bob, lane-change roll tilt, road-beer FOV pulse.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Idle Bob")]
    [SerializeField] private float bobSpeed  = 1.3f;
    [SerializeField] private float bobY      = 0.04f;
    [SerializeField] private float bobX      = 0.015f;

    [Header("Lane Tilt")]
    [SerializeField] private float tiltAngle = 2.5f;   // degrees of roll at full tilt
    [SerializeField] private float tiltSpeed = 4f;     // lerp speed toward target

    [Header("Road Beer FOV Pulse")]
    [SerializeField] private float fovPulseAmount   = 5f;   // degrees of variance
    [SerializeField] private float fovPulseSpeed    = 0.65f; // cycles per second
    [SerializeField] private float fovEffectDuration = 5f;  // must match RoadBeerEffect duration

    private Camera cam;
    private Vector3    baseLocalPos;
    private Quaternion baseLocalRot;
    private float      normalFOV;

    private float currentTilt = 0f;
    private float targetTilt  = 0f;

    private bool  fovActive   = false;
    private float fovElapsed  = 0f;

    void Start()
    {
        cam = GetComponent<Camera>();
        baseLocalPos = transform.localPosition;
        baseLocalRot = transform.localRotation;
        if (cam != null) normalFOV = cam.fieldOfView;
    }

    void Update()
    {
        ApplyBob();
        ApplyTilt();
        ApplyFOV();
    }

    // ── public API ────────────────────────────────────────────────────────────

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

    // ── internal ──────────────────────────────────────────────────────────────

    void ApplyBob()
    {
        float t  = Time.time * bobSpeed;
        float dy = Mathf.Sin(t) * bobY;
        float dx = Mathf.Sin(t * 0.65f) * bobX;
        transform.localPosition = baseLocalPos + new Vector3(dx, dy, 0f);
    }

    void ApplyTilt()
    {
        // Ease toward target, then decay target back to 0 (simulates inertia settling)
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        targetTilt  = Mathf.Lerp(targetTilt,  0f,         Time.deltaTime * tiltSpeed * 0.45f);

        transform.localRotation = baseLocalRot * Quaternion.Euler(0f, 0f, currentTilt);
    }

    void ApplyFOV()
    {
        if (!fovActive || cam == null) return;

        fovElapsed += Time.deltaTime;
        float fade   = 1f - Mathf.Clamp01(fovElapsed / fovEffectDuration);
        float pulse  = Mathf.Sin(fovElapsed * fovPulseSpeed * Mathf.PI * 2f) * fovPulseAmount * fade;
        cam.fieldOfView = normalFOV + pulse;

        if (fovElapsed >= fovEffectDuration)
        {
            fovActive = false;
            cam.fieldOfView = normalFOV;
        }
    }
}
