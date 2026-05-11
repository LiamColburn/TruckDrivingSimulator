using UnityEngine;
using System.Collections.Generic;
 
/// <summary>
/// Traffic system with object pooling - spawns vehicles, moves them backward, recycles when behind player
/// Cars are reused instead of destroyed/instantiated for better performance
/// </summary>
public class TrafficSpawner : MonoBehaviour
{
    [Header("Vehicle Pool (Assign all 35 prefabs)")]
    [SerializeField] private GameObject[] vehiclePrefabs; // Drag all 35 car prefabs here
    
    [Header("Spawn Settings")]
    [SerializeField] private int maxActiveVehicles = 10; // How many cars on road at once
    [SerializeField] private float spawnInterval = 2f; // Time between spawns (seconds)
    [SerializeField] private float minSpacing = 30f; // Minimum distance between cars
    
    [Header("Lane Positions (MUST MATCH TruckPlayerController!)")]
    [SerializeField] private float[] laneXPositions = new float[] { -4.66f, 0f, 4.66f }; // Left, Center, Right
    
    [Header("Movement")]
    [SerializeField] private float vehicleSpeed = 20f; // How fast cars move backward (relative to truck)
    [SerializeField] private float spawnDistance = 150f; // How far ahead to spawn
    [SerializeField] private float despawnDistance = -50f; // Z position to despawn (behind truck)
    
    [Header("References")]
    [SerializeField] private Transform playerTruck; // Your truck
    
    // Object pool for reusing vehicles
    private List<GameObject> vehiclePool = new List<GameObject>();
    private List<GameObject> activeVehicles = new List<GameObject>();
    private float spawnTimer = 0f;
    
    void Start()
    {
        // Find player if not assigned
        if (playerTruck == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTruck = player.transform;
            }
            else
            {
                Debug.LogError("TrafficSpawner: No player found! Tag your truck with 'Player'");
                return;
            }
        }
        
        // Validate
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
        {
            Debug.LogError("TrafficSpawner: No vehicle prefabs assigned! Drag all 35 car prefabs into the array.");
            return;
        }
        
        // Create initial pool
        InitializePool();
        
        Debug.Log($"TrafficSpawner ready with {vehiclePrefabs.Length} vehicle types. Max active: {maxActiveVehicles}");
    }
    
    void InitializePool()
    {
        // Pre-instantiate some vehicles for the pool
        int poolSize = Mathf.Min(maxActiveVehicles + 5, 20); // Create a reasonable pool
        
        for (int i = 0; i < poolSize; i++)
        {
            // Pick random prefab
            GameObject prefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
            GameObject vehicle = Instantiate(prefab, Vector3.zero, Quaternion.Euler(0, 180, 0));
            vehicle.name = $"{prefab.name}_Pooled";
            vehicle.transform.SetParent(transform);
            vehicle.SetActive(false); // Start inactive
            
            // Make sure it has "Vehicle" tag
            vehicle.tag = "Vehicle";
            
            vehiclePool.Add(vehicle);
        }
        
        Debug.Log($"Created pool of {poolSize} vehicles");
    }
    
    void Update()
    {
        if (playerTruck == null)
            return;
        
        // Spawn timer
        spawnTimer += Time.deltaTime;
        
        // Try to spawn if under limit and timer ready
        if (activeVehicles.Count < maxActiveVehicles && spawnTimer >= spawnInterval)
        {
            SpawnWave();
            spawnTimer = 0f;
        }
        
        // Move all vehicles and check for recycling
        UpdateVehicles();
    }
    
    void SpawnWave()
    {
        // Spawn 1 or 2 vehicles
        int numToSpawn = Random.Range(1, 3); // 1 or 2
        
        // Make sure we don't exceed max
        numToSpawn = Mathf.Min(numToSpawn, maxActiveVehicles - activeVehicles.Count);
        
        if (numToSpawn <= 0)
            return;
        
        for (int i = 0; i < numToSpawn; i++)
        {
            SpawnVehicle(i);
        }
    }
    
    void SpawnVehicle(int waveIndex)
    {
        // Get vehicle from pool or create new one
        GameObject vehicle = GetPooledVehicle();
        
        if (vehicle == null)
        {
            Debug.LogWarning("No available vehicles in pool! Creating new one...");
            GameObject prefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
            vehicle = Instantiate(prefab, Vector3.zero, Quaternion.Euler(0, 180, 0));
            vehicle.name = $"{prefab.name}_Extra";
            vehicle.transform.SetParent(transform);
            vehicle.tag = "Vehicle";
            vehiclePool.Add(vehicle);
        }
        
        // Pick random lane
        float laneX = laneXPositions[Random.Range(0, laneXPositions.Length)];
        
        // Calculate spawn Z position
        // If spawning multiple, offset them so they're not on top of each other
        float spawnZ = playerTruck.position.z + spawnDistance + (waveIndex * minSpacing);
        
        // Check if too close to another vehicle in same lane
        if (IsTooClose(laneX, spawnZ))
        {
            // Put vehicle back in pool
            vehicle.SetActive(false);
            return;
        }
        
        // Position vehicle
        vehicle.transform.position = new Vector3(laneX, 0f, spawnZ);
        vehicle.transform.rotation = Quaternion.Euler(0, 180, 0); // Face toward player
        vehicle.SetActive(true);
        
        // Add to active list
        if (!activeVehicles.Contains(vehicle))
        {
            activeVehicles.Add(vehicle);
        }
        
        Debug.Log($"Spawned {vehicle.name} in lane X={laneX:F2} at Z={spawnZ:F0}");
    }
    
    GameObject GetPooledVehicle()
    {
        // Find an inactive vehicle in the pool
        foreach (GameObject vehicle in vehiclePool)
        {
            if (vehicle != null && !vehicle.activeInHierarchy)
            {
                return vehicle;
            }
        }
        
        return null; // No available vehicles
    }
    
    bool IsTooClose(float laneX, float spawnZ)
    {
        // Check if another vehicle is too close in the same lane
        foreach (GameObject vehicle in activeVehicles)
        {
            if (vehicle == null || !vehicle.activeInHierarchy)
                continue;
            
            // Check same lane
            if (Mathf.Abs(vehicle.transform.position.x - laneX) < 1f)
            {
                // Check distance
                float distance = Mathf.Abs(vehicle.transform.position.z - spawnZ);
                if (distance < minSpacing)
                {
                    return true; // Too close!
                }
            }
        }
        
        return false; // Safe to spawn
    }
    
    void UpdateVehicles()
    {
        // Move vehicles and check for recycling
        for (int i = activeVehicles.Count - 1; i >= 0; i--)
        {
            GameObject vehicle = activeVehicles[i];
            
            // Skip if destroyed (collision) or inactive
            if (vehicle == null || !vehicle.activeInHierarchy)
            {
                activeVehicles.RemoveAt(i);
                continue;
            }
            
            // Move vehicle backward (toward and past player)
            // Using Space.Self with rotation 180 means forward = toward player
            vehicle.transform.Translate(Vector3.forward * vehicleSpeed * Time.deltaTime, Space.Self);
            
            // Check if behind despawn line
            if (vehicle.transform.position.z < playerTruck.position.z + despawnDistance)
            {
                // Recycle vehicle (don't destroy, just deactivate)
                vehicle.SetActive(false);
                activeVehicles.RemoveAt(i);
                Debug.Log($"Recycled vehicle (passed player). Active: {activeVehicles.Count}");
            }
        }
    }
    
    // Public methods for tuning
    public void SetMaxVehicles(int count)
    {
        maxActiveVehicles = count;
    }
    
    public void SetVehicleSpeed(float speed)
    {
        vehicleSpeed = speed;
    }
    
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }
    
    public int GetActiveVehicleCount()
    {
        return activeVehicles.Count;
    }
    
    // Cleanup
    public void ClearAllVehicles()
    {
        foreach (GameObject vehicle in activeVehicles)
        {
            if (vehicle != null)
                vehicle.SetActive(false);
        }
        activeVehicles.Clear();
    }
    
    // Remove a vehicle from active list (called when crashed into)
    public void RemoveVehicle(GameObject vehicle)
    {
        if (activeVehicles.Contains(vehicle))
        {
            activeVehicles.Remove(vehicle);
        }
    }
    
    void OnDrawGizmos()
    {
        if (playerTruck == null)
            return;
        
        // Draw spawn line (green)
        Gizmos.color = Color.green;
        float spawnZ = playerTruck.position.z + spawnDistance;
        Gizmos.DrawLine(new Vector3(-10, 0.5f, spawnZ), new Vector3(10, 0.5f, spawnZ));
        
        // Draw despawn line (red)
        Gizmos.color = Color.red;
        float despawnZ = playerTruck.position.z + despawnDistance;
        Gizmos.DrawLine(new Vector3(-10, 0.5f, despawnZ), new Vector3(10, 0.5f, despawnZ));
        
        // Draw lanes
        Gizmos.color = Color.yellow;
        foreach (float laneX in laneXPositions)
        {
            Vector3 start = new Vector3(laneX, 0.5f, playerTruck.position.z - 50);
            Vector3 end = new Vector3(laneX, 0.5f, playerTruck.position.z + spawnDistance);
            Gizmos.DrawLine(start, end);
        }
        
        // Labels
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            new Vector3(0, 2, spawnZ),
            "SPAWN LINE",
            new GUIStyle() { normal = new GUIStyleState() { textColor = Color.green } }
        );
        
        UnityEditor.Handles.Label(
            new Vector3(0, 2, despawnZ),
            "DESPAWN LINE",
            new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } }
        );
        #endif
    }
}