using UnityEngine;

public class AudioLibrary : MonoBehaviour
{
    // Singleton instance
    public static AudioLibrary Instance;

    // Sound effect clips
     [Header("UI")]
    public AudioClip uiClick;

    [Header("Interactions")]
    public AudioClip Cooking;
    public AudioClip Harvesting;

    [Header("Misc")]
    public AudioClip Birds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Gets the sound effect clip by name.
    /// </summary>
    public AudioClip GetSfx(string sfxName)
    {
        return sfxName.ToLower() switch
        {
            "uiclick" => uiClick,

            "cooking" => Cooking,
            "harvesting" => Harvesting,

            "birds" => Birds,

            _ => null,
        };
    }
}
