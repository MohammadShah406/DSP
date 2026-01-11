using UnityEngine;

public class AudioLibrary : MonoBehaviour
{
    // Singleton instance
    public static AudioLibrary Instance;

    // Sound effect clips
     [Header("UI")]
    public AudioClip uiClickPrimary;
    public AudioClip uiClickSecondary;
    public AudioClip uiHover;

    [Header("Donation/Upgrades")]
    public AudioClip donationReceived;
    public AudioClip resourceAdded;
    public AudioClip upgradeDone;


    [Header("Interactions")]
    public AudioClip cooking;
    public AudioClip harvesting;

    [Header("Misc")]
    public AudioClip birds;

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
            "uiclickprimary" => uiClickPrimary,
            "uiclicksecondary" => uiClickSecondary,
            "uihover" => uiHover,

            "donationreceived" => donationReceived,
            "resourceadded" => resourceAdded,
            "upgradedone" => upgradeDone,

            "cooking" => cooking,
            "harvesting" => harvesting,

            "birds" => birds,

            _ => null,
        };
    }
}
