using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Road system with object pooling - spawns road chunks ahead of player
/// Uses EXACT same pattern as TrafficSpawner for consistency
/// Chunks move BACKWARD toward and past the player
/// Chunks stay ACTIVE (moved far away instead of deactivated) to keep colliders working
/// </summary>
public class InfiniteRoadManager : MonoBehaviour
{
    [Header("Road Chunk Prefab")]
    [SerializeField] private GameObject roadChunkPrefab; // Your road piece prefab
    
    [Header("Spawn Settings")]
    [SerializeField] private int maxActiveChunks = 5; // How many chunks on road at once
    [SerializeField] private float chunkLength = 20f; // How long each road piece is (adjust to match your prefab)
    [SerializeField] private float spawnInterval = 1f; // Time between spawn checks (seconds)
    [SerializeField] private float roadYPosition = -1.7465f; // Y position to spawn chunks at
    
    [Header("Movement")]
    [SerializeField] private float roadSpeed = 20f; // How fast chunks move backward (MUST match TrafficSpawner vehicleSpeed!)
    [SerializeField] private float spawnDistance = 300f; // How far ahead to spawn chunks
    [SerializeField] private float despawnDistance = -50f; // How far behind to despawn
    
    [Header("References")]
    [SerializeField] private Transform playerTruck; // Your truck/cube
    
    // Object pool for reusing chunks
    private List<GameObject> chunkPool = new List<GameObject>();
    private List<GameObject> activeChunks = new List<GameObject>();
    private float spawnTimer = 0f;
    private float nextSpawnZ = 0f;
    
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
                Debug.LogError("InfiniteRoadManager: No player found! Tag your truck with 'Player'");
                return;
            }
        }
        
        // Validate
        if (roadChunkPrefab == null)
        {
            Debug.LogError("InfiniteRoadManager: No road chunk prefab assigned!");
            return;
        }
        
        // Create initial pool
        InitializePool();
        
        // Set initial spawn position
        nextSpawnZ = playerTruck.position.z;
        
        Debug.Log($"InfiniteRoadManager ready! Speed: {roadSpeed}");
    }
    
    void InitializePool()
    {
        // Pre-instantiate some chunks for the pool
        int poolSize = maxActiveChunks + 2; // Create a few extra
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject chunk = Instantiate(roadChunkPrefab, new Vector3(0f, -10000f, -10000f), Quaternion.identity);
            chunk.name = $"RoadChunk_Pooled_{i}";
            chunk.transform.SetParent(transform);
            // Keep active but position far away (keeps colliders working)
            chunk.SetActive(true);
            
            // Make sure Rigidbody is kinematic
            Rigidbody rb = chunk.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            
            chunkPool.Add(chunk);
        }
        
        Debug.Log($"Created pool of {poolSize} road chunks");
    }
    
    void Update()
    {
        if (playerTruck == null)
            return;
        
        // Spawn timer
        spawnTimer += Time.deltaTime;
        
        // Try to spawn if under limit and timer ready
        if (activeChunks.Count < maxActiveChunks && spawnTimer >= spawnInterval)
        {
            SpawnChunks();
            spawnTimer = 0f;
        }
        
        // Move all chunks and check for recycling
        UpdateChunks();
    }
    
    void SpawnChunks()
    {
        // Calculate where we need chunks
        float targetZ = playerTruck.position.z + spawnDistance;
        
        // Keep spawning until we fill the distance
        while (nextSpawnZ < targetZ && activeChunks.Count < maxActiveChunks)
        {
            SpawnChunk();
        }
    }
    
    void SpawnChunk()
    {
        // Get chunk from pool or create new one
        GameObject chunk = GetPooledChunk();
        
        if (chunk == null)
        {
            Debug.LogWarning("No available chunks in pool! Creating new one...");
            chunk = Instantiate(roadChunkPrefab, new Vector3(0f, -10000f, -10000f), Quaternion.identity);
            chunk.name = $"RoadChunk_Extra";
            chunk.transform.SetParent(transform);
            chunk.SetActive(true);
            
            // Make sure Rigidbody is kinematic
            Rigidbody rb = chunk.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            
            chunkPool.Add(chunk);
        }
        
        // Make sure Rigidbody is kinematic and gravity is off (in case prefab changed)
        Rigidbody chunkRb = chunk.GetComponent<Rigidbody>();
        if (chunkRb != null)
        {
            chunkRb.isKinematic = true;
            chunkRb.useGravity = false;
        }
        
        // Position chunk at correct position - FORCE X to 0 to center it
        Vector3 spawnPos = new Vector3(1.32307f, -1.7465f, nextSpawnZ);
        chunk.transform.position = spawnPos;
        chunk.transform.rotation = Quaternion.identity;
        chunk.transform.localScale = Vector3.one; // Reset scale if needed
        // Chunk is already active, just repositioned
        
        // Add to active list
        if (!activeChunks.Contains(chunk))
        {
            activeChunks.Add(chunk);
        }
        
        // Move spawn position forward for next chunk
        nextSpawnZ += chunkLength;
        
        Debug.Log($"Spawned road chunk at Z={spawnPos.z:F0}. Total: {activeChunks.Count}");
    }
    
    GameObject GetPooledChunk()
    {
        // Find an inactive or far-away chunk in the pool
        foreach (GameObject chunk in chunkPool)
        {
            if (chunk != null && chunk.transform.position.z < -5000f)
            {
                return chunk;
            }
        }
        
        return null; // No available chunks
    }
    
    void UpdateChunks()
    {
        // Move chunks and check for recycling
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = activeChunks[i];
            
            // Skip if destroyed
            if (chunk == null)
            {
                activeChunks.RemoveAt(i);
                continue;
            }
            
            // Move chunk backward (toward and past player)
            chunk.transform.Translate(Vector3.back * roadSpeed * Time.deltaTime, Space.World);
            
            // Check if behind despawn line
            if (chunk.transform.position.z < playerTruck.position.z + despawnDistance)
            {
                // Recycle chunk - move it far away instead of deactivating (keeps colliders working)
                chunk.transform.position = new Vector3(0f, -10000f, -10000f);
                activeChunks.RemoveAt(i);
                Debug.Log($"Recycled road chunk. Active: {activeChunks.Count}");
            }
        }
        
        // Also update next spawn position (moves backward with chunks)
        nextSpawnZ -= roadSpeed * Time.deltaTime;
    }
    
    // Public methods for tuning
    public void SetMaxChunks(int count)
    {
        maxActiveChunks = count;
    }
    
    public void SetRoadSpeed(float speed)
    {
        roadSpeed = speed;
    }
    
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }
    
    public int GetActiveChunkCount()
    {
        return activeChunks.Count;
    }
    
    // Cleanup
    public void ClearAllChunks()
    {
        foreach (GameObject chunk in activeChunks)
        {
            if (chunk != null)
                chunk.transform.position = new Vector3(0f, -10000f, -10000f);
        }
        activeChunks.Clear();
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
        
        // Draw player position (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(0, 0.5f, playerTruck.position.z), 2f);
        
        #if UNITY_EDITOR
        // Labels
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