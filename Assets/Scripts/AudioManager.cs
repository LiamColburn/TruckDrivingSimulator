using UnityEngine;

/// <summary>
/// Singleton audio hub. Persists across scene reloads.
/// Add this to a dedicated "AudioManager" GameObject in the scene.
/// Assign AudioClip assets in the Inspector; all slots are optional —
/// missing clips are silent rather than errors.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Clips (assign in Inspector)")]
    [SerializeField] private AudioClip hornClip;
    [SerializeField] private AudioClip eatClip;
    [SerializeField] private AudioClip drinkClip;
    [SerializeField] private AudioClip burpClip;
    [SerializeField] private AudioClip crashClip;

    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume  = 1f;
    [SerializeField] [Range(0f, 1f)] private float hornVolume = 0.85f;

    private AudioSource sfxSource;
    private AudioSource hornSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource            = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        hornSource            = gameObject.AddComponent<AudioSource>();
        hornSource.playOnAwake = false;
    }

    // ── public API ────────────────────────────────────────────────────────────

    public void PlayHorn()  { if (hornClip  != null) hornSource.PlayOneShot(hornClip, hornVolume); }
    public void PlayEat()   => Play(eatClip);
    public void PlayDrink() => Play(drinkClip);
    public void PlayBurp()  => Play(burpClip);
    public void PlayCrash() => Play(crashClip);

    // ── internal ──────────────────────────────────────────────────────────────

    void Play(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
