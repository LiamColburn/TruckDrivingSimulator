using UnityEngine;

/// <summary>
/// Fixes road chunk materials to receive lighting.
/// Attach to RoadManager - runs once at start to fix all road materials.
/// </summary>
public class FixRoadLighting : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool fixOnStart = true;
    
    void Start()
    {
        if (fixOnStart)
        {
            FixAllRoadMaterials();
        }
    }
    
    [ContextMenu("Fix Road Materials Now")]
    public void FixAllRoadMaterials()
    {
        // Find all renderers in the scene (including inactive/pooled chunks)
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        int fixedCount = 0;
        
        foreach (Renderer renderer in allRenderers)
        {
            // Check if this is part of a road chunk
            if (renderer.gameObject.name.Contains("RoadChunk") || 
                renderer.transform.parent != null && renderer.transform.parent.name.Contains("RoadChunk"))
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat != null)
                    {
                        // Get current shader name
                        string currentShader = mat.shader.name;
                        
                        // If it's unlit, change to lit
                        if (currentShader.Contains("Unlit"))
                        {
                            // Try to find appropriate lit shader
                            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
                            
                            if (litShader == null)
                            {
                                litShader = Shader.Find("Standard");
                            }
                            
                            if (litShader != null)
                            {
                                mat.shader = litShader;
                                fixedCount++;
                                Debug.Log($"Fixed material: {mat.name} - Changed from {currentShader} to {litShader.name}");
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log($"✅ Fixed {fixedCount} road materials to receive lighting!");
    }
}