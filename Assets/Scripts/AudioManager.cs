using System.Collections;
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
    [SerializeField] private AudioClip smokeClip;

    [Header("Background Music (drag your 4 MP3s here in order)")]
    [SerializeField] private AudioClip[] musicClips;

    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume   = 1f;
    [SerializeField] [Range(0f, 1f)] private float hornVolume  = 0.85f;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.4f;

    private AudioSource sfxSource;
    private AudioSource hornSource;
    private AudioSource musicSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource             = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        hornSource             = gameObject.AddComponent<AudioSource>();
        hornSource.playOnAwake = false;

        musicSource             = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop        = false;
        musicSource.volume      = musicVolume;

        if (musicClips != null && musicClips.Length > 0)
            StartCoroutine(MusicLoop());
    }

    // ── public API ────────────────────────────────────────────────────────────

    public void PlayHorn()  { if (hornClip  != null) hornSource.PlayOneShot(hornClip, hornVolume); }
    public void PlayEat()   => Play(eatClip);
    public void PlayDrink() => Play(drinkClip);
    public void PlayBurp()  => Play(burpClip);
    public void PlayCrash() => Play(crashClip);

    public void PlaySmoke() => Play(smokeClip);

    // ── music ─────────────────────────────────────────────────────────────────

    IEnumerator MusicLoop()
    {
        int index = 0;
        while (true)
        {
            AudioClip clip = musicClips[index % musicClips.Length];
            if (clip != null)
            {
                musicSource.clip = clip;
                musicSource.Play();
                yield return new WaitForSeconds(clip.length);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
            index++;
        }
    }

    // ── internal ──────────────────────────────────────────────────────────────

    void Play(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip, sfxVolume);
    }
}
