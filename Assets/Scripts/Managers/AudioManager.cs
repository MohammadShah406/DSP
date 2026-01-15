using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    // Audio Mixer Reference
    [Header("Mixer")]
    public AudioMixer mainMixer;

    // UI Sliders
    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // Validate references
        if (mainMixer == null)
        {
            Debug.LogError("AudioManager: Main AudioMixer is not assigned.");
            return;
        }
        if (masterSlider == null || musicSlider == null || sfxSlider == null)
        {
            Debug.LogError("AudioManager: One or more sliders are not assigned.");
            return;
        }

        // Load saved values or default to 0dB
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfx = PlayerPrefs.GetFloat("SfxVolume", 0.5f);

        // Apply values
        ApplyVolumes(master, music, sfx);

        // Update UI
        masterSlider.value = master;
        musicSlider.value = music;
        sfxSlider.value = sfx;

        // Hook up events
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    /// <summary>
    /// Applies the volume settings to the audio mixer.
    /// </summary>
    void ApplyVolumes(float master, float music, float sfx)
    {
        float masterDb = LinearToDecibel(master);
        float musicDb = LinearToDecibel(music) + masterDb;
        float sfxDb = LinearToDecibel(sfx) + masterDb;

        mainMixer.SetFloat("MusicVolume", musicDb);
        mainMixer.SetFloat("SfxVolume", sfxDb);
    }

    /// <summary>
    /// Sets the master volume level.
    /// </summary>
    public void SetMasterVolume(float value)
    {
        Debug.Log("MASTER CHANGED: " + value);
        PlayerPrefs.SetFloat("MasterVolume", value);
        ApplyVolumes(value, musicSlider.value, sfxSlider.value);
    }

    /// <summary>
    /// Sets the music volume level.
    /// </summary>
    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        ApplyVolumes(masterSlider.value, value, sfxSlider.value);
    }

    /// <summary>
    /// Sets the sound effects volume level.
    /// </summary>
    public void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat("SfxVolume", value);
        ApplyVolumes(masterSlider.value, musicSlider.value, value);
    }

    /// <summary>
    /// Converts a linear volume value to decibels.
    /// </summary>
    float LinearToDecibel(float value)
    {
        if (value <= 0.0001f)
            return -80f; // silence

        return Mathf.Log10(value) * 20f;
    }

    /// <summary>
    /// Plays the menu click sound effect.
    /// </summary>
    public void PlayMenuClick()
    {
        if (AudioPlayer.Instance == null || AudioLibrary.Instance == null) return;

        // Play the menu click sound
        AudioPlayer.Instance.Play(AudioLibrary.Instance.GetSfx("uiclick"));
    }
}
