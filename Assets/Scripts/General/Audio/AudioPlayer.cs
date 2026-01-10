using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    // Singleton instance
    public static AudioPlayer Instance;

    // Optional audio settings
    [Header("Optional defaults")]
    [Range(0f, 1f)] public float defaultVolume = 1f;
    public float pitchMin = 1f;
    public float pitchMax = 1.1f;

    // Audio source component
    public AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    public void Play(AudioClip clip, float volume = -1f)
    {
        if (clip == null) return;

        audioSource.pitch = Random.Range(pitchMin, pitchMax);
        audioSource.PlayOneShot(clip, volume < 0f ? defaultVolume : volume);
    }
}
