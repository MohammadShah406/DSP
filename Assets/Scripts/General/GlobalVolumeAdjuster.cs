using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class GlobalVolumeAdjuster : MonoBehaviour
{
    //global volume reference
    [Header("Global Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("Hope Value (0–100+)")]
    [Range(0, 100)]
    public int hope;

    [Header("For Editor Only")]
    public bool SetVolume = false;

    // Cached overrides
    private FilmGrain filmGrain;
    private ColorAdjustments colorAdjustments;
    private WhiteBalance whiteBalance;
    private Vignette vignette;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!globalVolume || !globalVolume.profile)
        {
            Debug.LogError("Global Volume or Volume Profile not assigned!");
            return;
        }

        // Cache overrides once
        globalVolume.profile.TryGet(out filmGrain);
        globalVolume.profile.TryGet(out colorAdjustments);
        globalVolume.profile.TryGet(out whiteBalance);
        globalVolume.profile.TryGet(out vignette);

        SetGlobalSettingsStart();
        SetGlobalSettings();
    }

    public void SetGlobalSettingsStart()
    {
        // Film Grain defaults
        if (filmGrain != null)
        {
            filmGrain.active = true;
            filmGrain.type.value = FilmGrainLookup.Thin1;
            filmGrain.intensity.value = 0.5f;

            colorAdjustments.saturation.value = -100f;
            whiteBalance.temperature.value = 0f;
            vignette.intensity.value = 0.4f;
        }
    }

    private void Update()
    {
        if (SetVolume)
        {
            SetGlobalSettings();
            SetVolume = false;
        }
    }


    /// <summary>
    /// Adjusts global volume settings based on the current hope value.
    /// </summary>
    public void SetGlobalSettings()
    {
        if (colorAdjustments == null || whiteBalance == null || vignette == null)
            return;

        // Clamp to 0..100 for predictable blending
        float h = Mathf.Clamp(hope, 0f, 100f);

        // Targets: (saturation, temperature, vignette)
        Vector3 L0 = new Vector3(-100f, 0f, 0.4f);
        Vector3 L1 = new Vector3(-20f, 20f, 0.2f);
        Vector3 L2 = new Vector3(-10f, 30f, 0.1f);
        Vector3 L3 = new Vector3(40f, 40f, 0.0f);

        Vector3 result;

        if (h <= 40f)
        {
            // 0..40 -> L0..L1
            float t = Mathf.InverseLerp(0f, 40f, h);
            result = Vector3.Lerp(L0, L1, t);
        }
        else if (h <= 60f)
        {
            // 40..60 -> L1..L2
            float t = Mathf.InverseLerp(40f, 60f, h);
            result = Vector3.Lerp(L1, L2, t);
        }
        else if (h <= 80f)
        {
            // 60..80 -> L2..L3
            float t = Mathf.InverseLerp(60f, 80f, h);
            result = Vector3.Lerp(L2, L3, t);
        }
        else
        {
            // 80..100+ -> L3
            result = L3;
        }

        // Apply
        colorAdjustments.saturation.value = result.x;
        whiteBalance.temperature.value = result.y;
        vignette.intensity.value = result.z;
    }
}
