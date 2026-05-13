using UnityEngine;

/// <summary>
/// Slowly spins the windmill blade group around the Z axis (facing the road).
/// Attach to the BladeGroup child of any windmill built by RoadsideScenerySpawner.
/// </summary>
public class WindmillRotator : MonoBehaviour
{
    [SerializeField] private float rpm = 5f;

    void Update()
    {
        transform.Rotate(Vector3.forward, rpm * 6f * Time.deltaTime, Space.Self);
    }
}
