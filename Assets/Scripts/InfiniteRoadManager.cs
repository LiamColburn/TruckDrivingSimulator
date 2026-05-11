using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Infinite road generation - spawns road chunks ahead, removes behind
/// Works by moving road pieces backward while player stays stationary
/// </summary>
public class InfiniteRoadManager : MonoBehaviour
{
    [Header("Road Prefab")]
    [SerializeField] private GameObject roadChunkPrefab; // Your road piece (plane, terrain, etc.)
    
    [Header("Chunk Settings")]
    [SerializeField] private float chunkLength = 100f; // Length of each road piece (Z-axis)
    [SerializeField] private float chunkWidth = 15f; // Width of road (X-axis)
    [SerializeField] private int chunksAhead = 3; // How many chunks to keep ahead of player
    [SerializeField] private int chunksBehind = 1; // How many chunks to keep behind player
    
    [Header("Movement")]
    [SerializeField] private float roadSpeed = 20f; // How fast road moves backward (should match traffic speed)
    
    [Header("References")]
    [SerializeField] private Transform playerTruck; // Your truck
    
    // Active road chunks
    private List<GameObject> activeChunks = new List<GameObject>();
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
        }
        
        if (roadChunkPrefab == null)
        {
            Debug.LogError("InfiniteRoadManager: No road chunk prefab assigned!");
            return;
        }
        
        // Spawn initial road chunks
        SpawnInitialChunks();
        
        Debug.Log($"Infinite road ready! Chunks: {activeChunks.Count}");
    }
    
    void SpawnInitialChunks()
    {
        // Spawn chunks ahead and behind player
        int totalChunks = chunksAhead + chunksBehind + 1; // +1 for chunk player is on
        
        // Calculate starting position (put player in middle)
        float startZ = playerTruck.position.z - (chunksBehind * chunkLength);
        
        for (int i = 0; i < totalChunks; i++)
        {
            float spawnZ = startZ + (i * chunkLength);
            SpawnChunk(spawnZ);
        }
        
        nextSpawnZ = startZ + (totalChunks * chunkLength);
    }
    
    void Update()
    {
        if (playerTruck == null)
            return;
        
        // Move all road chunks backward
        MoveRoadChunks();
        
        // Check if we need to spawn new chunks ahead
        CheckSpawnNewChunks();
        
        // Remove chunks that are too far behind
        RemoveOldChunks();
    }
    
    void MoveRoadChunks()
    {
        // Move all chunks backward (in world space)
        foreach (GameObject chunk in activeChunks)
        {
            if (chunk != null)
            {
                chunk.transform.Translate(Vector3.back * roadSpeed * Time.deltaTime, Space.World);
            }
        }
        
        // Update next spawn position
        nextSpawnZ -= roadSpeed * Time.deltaTime;
    }
    
    void CheckSpawnNewChunks()
    {
        // Find the furthest chunk ahead
        float furthestZ = float.MinValue;
        foreach (GameObject chunk in activeChunks)
        {
            if (chunk != null && chunk.transform.position.z > furthestZ)
            {
                furthestZ = chunk.transform.position.z;
            }
        }
        
        // Spawn new chunks if needed
        float targetZ = playerTruck.position.z + (chunksAhead * chunkLength);
        
        while (furthestZ < targetZ)
        {
            SpawnChunk(furthestZ + chunkLength);
            furthestZ += chunkLength;
        }
    }
    
    void RemoveOldChunks()
    {
        // Remove chunks that are too far behind player
        float despawnZ = playerTruck.position.z - ((chunksBehind + 1) * chunkLength);
        
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = activeChunks[i];
            
            if (chunk == null)
            {
                activeChunks.RemoveAt(i);
                continue;
            }
            
            // Check if chunk is behind despawn line
            if (chunk.transform.position.z < despawnZ)
            {
                Destroy(chunk);
                activeChunks.RemoveAt(i);
                Debug.Log($"Removed old road chunk. Active chunks: {activeChunks.Count}");
            }
        }
    }
    
    void SpawnChunk(float zPosition)
    {
        // Spawn road chunk
        Vector3 spawnPos = new Vector3(0f, 0f, zPosition);
        GameObject chunk = Instantiate(roadChunkPrefab, spawnPos, Quaternion.identity);
        chunk.name = $"RoadChunk_Z{zPosition:F0}";
        chunk.transform.SetParent(transform);
        
        activeChunks.Add(chunk);
        
        Debug.Log($"Spawned road chunk at Z={zPosition:F0}");
    }
    
    // Public methods
    public void SetRoadSpeed(float speed)
    {
        roadSpeed = speed;
    }
    
    public float GetRoadSpeed()
    {
        return roadSpeed;
    }
    
    void OnDrawGizmos()
    {
        if (playerTruck == null)
            return;
        
        // Draw spawn line (green) - where new chunks appear
        Gizmos.color = Color.green;
        float spawnZ = playerTruck.position.z + (chunksAhead * chunkLength);
        Gizmos.DrawLine(new Vector3(-chunkWidth/2, 0.1f, spawnZ), new Vector3(chunkWidth/2, 0.1f, spawnZ));
        
        // Draw despawn line (red) - where old chunks are removed
        Gizmos.color = Color.red;
        float despawnZ = playerTruck.position.z - ((chunksBehind + 1) * chunkLength);
        Gizmos.DrawLine(new Vector3(-chunkWidth/2, 0.1f, despawnZ), new Vector3(chunkWidth/2, 0.1f, despawnZ));
        
        // Draw player zone (yellow)
        Gizmos.color = Color.yellow;
        float playerZoneStart = playerTruck.position.z - (chunkLength / 2);
        float playerZoneEnd = playerTruck.position.z + (chunkLength / 2);
        Gizmos.DrawWireCube(
            new Vector3(0, 0.1f, playerTruck.position.z),
            new Vector3(chunkWidth, 0.2f, chunkLength)
        );
    }
}