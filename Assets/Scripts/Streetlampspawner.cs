using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns street lamps every 4-5 seconds that move backward toward the player.
/// Simple and clean - just like the traffic spawner pattern.
/// </summary>
public class StreetLampSpawner : MonoBehaviour
{
    [Header("Street Lamp Prefab")]
    [SerializeField] private GameObject streetLampPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 4.5f; // Spawn every 4-5 seconds
    [SerializeField] private float spawnDistanceAhead = 300f; // How far ahead to spawn
    [SerializeField] private float lampRightX = 7.5f; // Right lamp X position (from image)
    [SerializeField] private float lampLeftX = -7.5f; // Left lamp X position (from image)
    [SerializeField] private float lampY = 0.17f; // Y position (from image)
    
    [Header("Movement")]
    [SerializeField] private float lampSpeed = 60f; // VERY FAST! (3x road speed)
    [SerializeField] private float despawnDistance = -50f; // Despawn when this far behind player
    
    [Header("References")]
    [SerializeField] private Transform playerTruck;
    
    private List<GameObject> activeLamps = new List<GameObject>();
    private float spawnTimer = 0f;
    
    void Start()
    {
        if (playerTruck == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTruck = player.transform;
            }
        }
        
        if (streetLampPrefab == null)
        {
            Debug.LogError("StreetLampSpawner: No street lamp prefab assigned!");
            return;
        }
        
        Debug.Log("Street lamp spawner ready!");
    }
    
    void Update()
    {
        if (playerTruck == null || streetLampPrefab == null)
            return;
        
        // Spawn timer
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval)
        {
            SpawnLampPair();
            spawnTimer = 0f;
        }
        
        // Move all lamps backward
        MoveLamps();
    }
    
    void SpawnLampPair()
    {
        float spawnZ = playerTruck.position.z + spawnDistanceAhead;
        
        // Spawn right side lamp (exact position from image, mirrored)
        GameObject rightLamp = Instantiate(streetLampPrefab, new Vector3(lampRightX, lampY, spawnZ), Quaternion.identity);
        rightLamp.name = "StreetLamp_Right";
        rightLamp.transform.SetParent(transform);
        rightLamp.transform.localScale = new Vector3(-0.2294f, 0.2294f, 0.2294f); // Flipped on X axis
        MakeLampBlack(rightLamp);
        AddLightToLamp(rightLamp);
        activeLamps.Add(rightLamp);
        
        // Spawn left side lamp (exact position from image)
        GameObject leftLamp = Instantiate(streetLampPrefab, new Vector3(lampLeftX, lampY, spawnZ), Quaternion.identity);
        leftLamp.name = "StreetLamp_Left";
        leftLamp.transform.SetParent(transform);
        leftLamp.transform.localScale = new Vector3(0.2294f, 0.2294f, 0.2294f); // Normal scale
        MakeLampBlack(leftLamp);
        AddLightToLamp(leftLamp);
        activeLamps.Add(leftLamp);
    }
    
    void AddLightToLamp(GameObject lamp)
    {
        // Create a light attached to this lamp
        GameObject lightObj = new GameObject("LampLight");
        Light lightComp = lightObj.AddComponent<Light>();
        lightComp.type = LightType.Spot;
        lightComp.color = new Color(1f, 0.95f, 0.8f); // Warm white
        lightComp.intensity = 200f;
        lightComp.range = 100f;
        lightComp.spotAngle = 120f; 
        lightComp.innerSpotAngle = 97f; 
        
        // Attach to lamp and position at top
        lightObj.transform.SetParent(lamp.transform);
        lightObj.transform.localPosition = new Vector3(10f, 10f, 0f); // 3 units up from lamp base
        lightObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Point straight down
    }
    
    void MakeLampBlack(GameObject lamp)
    {
        // Get all renderers and set their materials to black
        Renderer[] renderers = lamp.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                mat.color = Color.black;
            }
        }
    }
    
    void MoveLamps()
    {
        for (int i = activeLamps.Count - 1; i >= 0; i--)
        {
            GameObject lamp = activeLamps[i];
            
            if (lamp == null)
            {
                activeLamps.RemoveAt(i);
                continue;
            }
            
            // Move backward in -Z direction
            lamp.transform.Translate(Vector3.back * lampSpeed * Time.deltaTime, Space.World);
            
            // Despawn if behind player
            if (lamp.transform.position.z < playerTruck.position.z + despawnDistance)
            {
                Destroy(lamp);
                activeLamps.RemoveAt(i);
            }
        }
    }
}