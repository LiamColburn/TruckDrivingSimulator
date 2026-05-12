using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// Plays video clips as full-screen overlays for trucker activities
/// Videos play when eating hotdogs, drinking beers/big gulps, smoking
/// </summary>
public class TruckerVideoOverlay : MonoBehaviour
{
    [Header("Video Clips")]
    [SerializeField] private VideoClip hotdogVideo;
    [SerializeField] private VideoClip beerVideo;
    [SerializeField] private VideoClip bigGulpVideo;
    [SerializeField] private VideoClip smokingVideo;
    
    [Header("UI References")]
    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private VideoPlayer videoPlayer;
    
    [Header("Settings")]
    [SerializeField] private bool pauseGameDuringVideo = false;
    [SerializeField] private float videoAlpha = 0.5f; // 0-1, how opaque the video is
    
    private bool isPlayingVideo = false;
    
    void Start()
    {
        SetupVideoOverlay();
    }
    
    void SetupVideoOverlay()
    {
        // Create canvas if not assigned
        if (overlayCanvas == null)
        {
            GameObject canvasGO = new GameObject("VideoOverlayCanvas");
            overlayCanvas = canvasGO.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 999; // On top of everything
            
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create video display if not assigned
        if (videoDisplay == null)
        {
            GameObject imageGO = new GameObject("VideoDisplay");
            imageGO.transform.SetParent(overlayCanvas.transform, false);
            
            videoDisplay = imageGO.AddComponent<RawImage>();
            
            // Stretch to fill screen
            RectTransform rt = videoDisplay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            
            // Set alpha
            Color color = videoDisplay.color;
            color.a = videoAlpha;
            videoDisplay.color = color;
        }
        
        // Create video player if not assigned
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }
        
        // Setup video player
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.VideoClip;
        
        // Create render texture
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
        videoPlayer.targetTexture = renderTexture;
        videoDisplay.texture = renderTexture;
        
        // Hide by default
        overlayCanvas.gameObject.SetActive(false);
        
        // Subscribe to video end event
        videoPlayer.loopPointReached += OnVideoFinished;
        
        Debug.Log("TruckerVideoOverlay ready!");
    }
    
    /// <summary>
    /// Play hotdog eating video
    /// </summary>
    public void PlayHotdogVideo()
    {
        if (hotdogVideo != null)
        {
            PlayVideo(hotdogVideo, "Hotdog");
        }
        else
        {
            Debug.LogWarning("No hotdog video assigned!");
        }
    }
    
    /// <summary>
    /// Play beer drinking video
    /// </summary>
    public void PlayBeerVideo()
    {
        if (beerVideo != null)
        {
            PlayVideo(beerVideo, "Beer");
        }
        else
        {
            Debug.LogWarning("No beer video assigned!");
        }
    }
    
    /// <summary>
    /// Play Big Gulp drinking video
    /// </summary>
    public void PlayBigGulpVideo()
    {
        if (bigGulpVideo != null)
        {
            PlayVideo(bigGulpVideo, "Big Gulp");
        }
        else
        {
            Debug.LogWarning("No Big Gulp video assigned!");
        }
    }
    
    /// <summary>
    /// Play smoking video
    /// </summary>
    public void PlaySmokingVideo()
    {
        if (smokingVideo != null)
        {
            PlayVideo(smokingVideo, "Smoking");
        }
        else
        {
            Debug.LogWarning("No smoking video assigned!");
        }
    }
    
    void PlayVideo(VideoClip clip, string activityName)
    {
        if (isPlayingVideo)
        {
            Debug.Log("Already playing a video!");
            return;
        }
        
        isPlayingVideo = true;
        
        // Pause game if enabled
        if (pauseGameDuringVideo)
        {
            Time.timeScale = 0f;
        }
        
        // Setup and play video
        videoPlayer.clip = clip;
        overlayCanvas.gameObject.SetActive(true);
        videoPlayer.Play();
        
        Debug.Log($"Playing {activityName} video ({clip.length:F1} seconds)");
    }
    
    void OnVideoFinished(VideoPlayer vp)
    {
        // Hide overlay
        overlayCanvas.gameObject.SetActive(false);
        isPlayingVideo = false;
        
        // Unpause game
        if (pauseGameDuringVideo)
        {
            Time.timeScale = 1f;
        }
        
        Debug.Log("Video finished!");
    }
    
    /// <summary>
    /// Stop video early
    /// </summary>
    public void StopVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            OnVideoFinished(videoPlayer);
        }
    }
    
    /// <summary>
    /// Set video opacity (0 = invisible, 1 = fully opaque)
    /// </summary>
    public void SetVideoAlpha(float alpha)
    {
        videoAlpha = Mathf.Clamp01(alpha);
        if (videoDisplay != null)
        {
            Color color = videoDisplay.color;
            color.a = videoAlpha;
            videoDisplay.color = color;
        }
    }
    
    void OnDestroy()
    {
        // Cleanup
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
        
        // Make sure game isn't paused
        Time.timeScale = 1f;
    }
}