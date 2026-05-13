using UnityEngine;
 
/// <summary>
/// Adds headlights to the player truck.
/// Attach this to the Cube (player truck).
/// Creates two spotlight headlights automatically.
/// </summary>
public class TruckHeadlights : MonoBehaviour
{
    void Start()
    {
        // Create left headlight
        GameObject leftLight = new GameObject("LeftHeadlight");
        Light leftComp = leftLight.AddComponent<Light>();
        leftComp.type = LightType.Spot;
        leftComp.color = new Color(1f, 0.95f, 0.8f); // Warm white
        leftComp.intensity = 1000f;
        leftComp.range = 100f;
        leftComp.spotAngle = 60f;
        
        // Attach to cube
        leftLight.transform.SetParent(transform);
        leftLight.transform.localPosition = new Vector3(-1f, 0.5f, 2.5f); // Left side, up, forward
        leftLight.transform.localRotation = Quaternion.Euler(10f, 0f, 0f); // Point slightly down
        
        // Create right headlight
        GameObject rightLight = new GameObject("RightHeadlight");
        Light rightComp = rightLight.AddComponent<Light>();
        rightComp.type = LightType.Spot;
        rightComp.color = new Color(1f, 0.95f, 0.8f); // Warm white
        rightComp.intensity = 1000f;
        rightComp.range = 100f;
        rightComp.spotAngle = 60f;
        
        // Attach to cube
        rightLight.transform.SetParent(transform);
        rightLight.transform.localPosition = new Vector3(0.75f, 0.5f, 2.5f); // Right side, up, forward
        rightLight.transform.localRotation = Quaternion.Euler(10f, 0f, 0f); // Point slightly down
        
        Debug.Log("Simple headlights created!");
    }
}