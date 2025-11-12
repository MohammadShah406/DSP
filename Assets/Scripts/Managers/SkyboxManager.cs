using UnityEngine;
using static TimeManager;

public class SkyboxManager : MonoBehaviour
{
    public static SkyboxManager Instance { get; private set; }

    [Header("Sky Presets (Procedural Skybox 2.0 Lite Materials)")]
    public Material morning;
    public Material afternoon;
    public Material evening;
    public Material night;

    [Header("Lights")]
    public Light sun;
    public Light moon;

    [Header("Sun Settings")]
    public float sunMaxIntensity = 1.2f;
    public float moonMaxIntensity = 0.8f;
    public Vector2 sunRotationBase = new Vector2(-90f, 170f);

    [Header("Transition Settings")]
    public float blendSpeed = 1f;

    private TimeManager timeManager;
    private Material blendSkybox;
    private Material lastPeriodMat;
    private float blendT = 1f; // fully at current period

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        timeManager = TimeManager.Instance;

        if (timeManager == null)
        {
            Debug.LogError("SkyboxManager: TimeManager instance not found!");
            return;
        }

        Material currentMat = GetPresetForTimePeriod(timeManager.currentTimePeriod);
        blendSkybox = new Material(currentMat);
        RenderSettings.skybox = blendSkybox;
        lastPeriodMat = currentMat;
    }

    private void LateUpdate()
    {
        if (timeManager == null) return;
        UpdateLightingAndSkybox();

    }

    private void UpdateLightingAndSkybox()
    {
        // Rotate sun and moon
        float time = timeManager.TimeOfDay;
        float sunAngle = time * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, sunRotationBase.y, 0f);
        moon.transform.rotation = Quaternion.Euler(sunAngle + 180f, sunRotationBase.y, 0f);

        // Sun/moon intensity
        float sunDot = Mathf.Clamp01(Vector3.Dot(sun.transform.forward, Vector3.down));
        sun.intensity = Mathf.Lerp(0f, sunMaxIntensity, sunDot);
        moon.intensity = Mathf.Lerp(moonMaxIntensity, 0f, sunDot);

        // Current period
        TimePeriod currentPeriod = timeManager.currentTimePeriod;
        Material currentMat = GetPresetForTimePeriod(currentPeriod);

        // If period changed, start blending
        if (lastPeriodMat != currentMat)
        {
            blendT = 0f;
            lastPeriodMat = currentMat;
        }

        // Blend skybox
        if (blendT < 1f)
        {
            blendT += Time.deltaTime * blendSpeed;
            blendSkybox.Lerp(blendSkybox, currentMat, Mathf.Clamp01(blendT));
        }
        else
        {
            blendSkybox.CopyPropertiesFromMaterial(currentMat);
        }
    }

    private Material GetPresetForTimePeriod(TimePeriod period)
    {
        return period switch
        {
            TimePeriod.Morning => morning,
            TimePeriod.Afternoon => afternoon,
            TimePeriod.Evening => evening,
            TimePeriod.Night => night,
            _ => morning
        };
    }

}
