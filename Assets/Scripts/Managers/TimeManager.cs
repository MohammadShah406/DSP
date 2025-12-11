using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Clock")]
    [SerializeField] public int minutes = 0;
    [SerializeField] public int hours = 8;
    [SerializeField] public int days = 1;
    public TimePeriod currentTimePeriod = TimePeriod.Morning;

    [Tooltip("In-game minutes per real-time second.")]
    [SerializeField, Range(0.1f, 120f)] private float gameMinutesPerSecond = 1f;
    [SerializeField] private bool isPaused = false;
    private int lastAutoPauseDay = 0;

    [Header("Active Hours Clamp")]
    [SerializeField] private bool restrictToActiveHours = false;
    [SerializeField, Range(0, 23)] private int activeStartHour = 8;
    [SerializeField, Range(0, 59)] private int activeStartMinute = 0;
    [SerializeField, Range(0, 23)] private int activeEndHour = 22;
    [SerializeField, Range(0, 59)] private int activeEndMinute = 59;

    [SerializeField] private bool autoPauseAtEndOfActiveHours = true;
    [SerializeField] private bool wrapToStartOnExceed = true;


    [Header("Lighting (Day/Night) If SkyBox Manager is not present")]
    [SerializeField] private Light sun;
    [SerializeField, Range(0f, 2f)] private float maxSunIntensity = 1.2f;
    [SerializeField, GradientUsage(true)] private Gradient sunColorOverDay;
    [SerializeField, GradientUsage(true)] private Gradient ambientColorOverDay;
    [SerializeField] private Vector2 sunBaseRotation = new Vector2(-90f, 170f);

    // Events
    public event Action<int, int, int> MinuteChanged;
    public event Action<int, int, int> HourChanged;
    public event Action<int> DayChanged;

    private float minuteAccumulator = 0f;

    public string FormattedTime => $"{hours:00}:{minutes:00}";
    public float TimeOfDay => (hours + (minutes / 60f)) / 24f;

    

    [Header("Time Periods")]
    [SerializeField] private TimeRange morning = new TimeRange { startHour = 6, startMinute = 0, endHour = 11, endMinute = 59 };
    [SerializeField] private TimeRange afternoon = new TimeRange { startHour = 12, startMinute = 0, endHour = 17, endMinute = 59 };
    [SerializeField] private TimeRange evening = new TimeRange { startHour = 18, startMinute = 0, endHour = 21, endMinute = 59 };
    [SerializeField] private TimeRange night = new TimeRange { startHour = 22, startMinute = 0, endHour = 5, endMinute = 59 };

    public enum TimePeriod
    {
        Morning,
        Afternoon,
        Evening,
        Night
    }

    [Serializable]
    public struct TimeRange
    {
        [Range(0, 23)] public int startHour;
        [Range(0, 59)] public int startMinute;
        [Range(0, 23)] public int endHour;
        [Range(0, 59)] public int endMinute;

        public bool Includes(int hour, int minute)
        {
            int total = hour * 60 + minute;
            int startTotal = startHour * 60 + startMinute;
            int endTotal = endHour * 60 + endMinute;

            if (endTotal >= startTotal)
                return total >= startTotal && total <= endTotal;
            else // wraps around midnight
                return total >= startTotal || total <= endTotal;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple TimeManager instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        if (sun == null)
        {
            Debug.LogWarning("TimeManager: No sun assigned, searching for a directional light in the scene.");
            sun = GameObject.FindAnyObjectByType<Light>();
            if (sun == null || sun.type != LightType.Directional)
            {
                Debug.LogWarning("TimeManager: No directional light found for the sun.");
            }
        }

        CheckForPendingLoad();

        ClampTime();
        ApplyActiveHourClamp(true);
        UpdateLighting();
    }

    private void Update()
    {
        if (isPaused)
        {
            UpdateLighting();
            return;
        }

        minuteAccumulator += Time.deltaTime * gameMinutesPerSecond;

        while (minuteAccumulator >= 1f)
        {
            minuteAccumulator -= 1f;
            IncrementMinute();

            // Auto-pause logic (Done once per day)
            if (restrictToActiveHours && autoPauseAtEndOfActiveHours)
            {
                if (CurrentTotalMinutes() >= EndWindowTotalMinutes() && lastAutoPauseDay != days)
                {
                    PauseAtEndOfWindow();
                    break; 
                }
            }

            // Wrap logic to go back to start of active window
            if (wrapToStartOnExceed && HasExceededActiveWindow())
            {
                WrapToStartOfWindow();
                break;
            }
        }

        UpdateLighting();
    }

    private void CheckForPendingLoad()
    {
        // Apply pending time if available
        if (PendingGameLoad.day.HasValue && PendingGameLoad.timeOfDay.HasValue)
        {
            SetTime(PendingGameLoad.day.Value, PendingGameLoad.timeOfDay.Value);

            PendingGameLoad.day = null;
            PendingGameLoad.timeOfDay = null;
        }
    }

    private void IncrementMinute()
    {
        minutes++;
        if (minutes >= 60)
        {
            minutes = 0;
            hours++;
            HourChanged?.Invoke(hours, minutes, days);

            if (hours >= 24)
            {
                hours = 0;
                days++;
                DayChanged?.Invoke(days);
                lastAutoPauseDay = days - 1;
            }
        }

        currentTimePeriod = CurrentTimePeriod();

        MinuteChanged?.Invoke(hours, minutes, days);
    }

    private void PauseAtEndOfWindow()
    {
        isPaused = true;
        lastAutoPauseDay = days;

        int endTotal = EndWindowTotalMinutes();
        hours = endTotal / 60;
        minutes = endTotal % 60;
        HourChanged?.Invoke(hours, minutes, days);
        MinuteChanged?.Invoke(hours, minutes, days);
    }

    private bool HasExceededActiveWindow()
    {
        if (!restrictToActiveHours) return false;
        return CurrentTotalMinutes() > EndWindowTotalMinutes();
    }

    private void WrapToStartOfWindow()
    {
        hours = activeStartHour;
        minutes = activeStartMinute;
        days++;
        DayChanged?.Invoke(days);
        HourChanged?.Invoke(hours, minutes, days);
        MinuteChanged?.Invoke(hours, minutes, days);
    }

    private int CurrentTotalMinutes() => hours * 60 + minutes;
    private int StartWindowTotalMinutes() => activeStartHour * 60 + activeStartMinute;
    private int EndWindowTotalMinutes() => activeEndHour * 60 + activeEndMinute;

    private void ApplyActiveHourClamp(bool invokeEvents)
    {
        if (!restrictToActiveHours) return;

        int start = StartWindowTotalMinutes();
        int end = EndWindowTotalMinutes();
        int current = CurrentTotalMinutes();

        if (current < start)
            SetTimeFromTotal(start, invokeEvents);
        else if (!wrapToStartOnExceed && current > end)
            SetTimeFromTotal(end, invokeEvents);
    }

    private void SetTimeFromTotal(int totalMinutesOfDay, bool invokeEvents)
    {
        int h = totalMinutesOfDay / 60;
        int m = totalMinutesOfDay % 60;
        bool hourChanged = h != hours;
        bool minuteChanged = m != minutes;

        hours = h;
        minutes = m;

        if (invokeEvents)
        {
            if (hourChanged) HourChanged?.Invoke(hours, minutes, days);
            if (minuteChanged) MinuteChanged?.Invoke(hours, minutes, days);
        }
    }

    private void UpdateLighting()
    {
        if (SkyboxManager.Instance != null)
        {
            return; // Let SkyboxManager handle lighting if it exists
        }

        float t = TimeOfDay;

        if (sun != null)
        {
            float elevation = (t * 360f) + sunBaseRotation.x;
            float azimuth = sunBaseRotation.y;
            sun.transform.rotation = Quaternion.Euler(elevation, azimuth, 0f);

            float daylight = Mathf.Clamp01(Vector3.Dot(sun.transform.forward, Vector3.down));
            float intensity = Mathf.Lerp(0f, maxSunIntensity, Mathf.SmoothStep(0f, 1f, daylight));
            sun.intensity = intensity;

            sun.color = sunColorOverDay.Evaluate(t);
            sun.shadows = LightShadows.Soft;
        }

        RenderSettings.ambientLight = ambientColorOverDay.Evaluate(t);
        RenderSettings.ambientIntensity = Mathf.Lerp(
            0.1f,
            1.0f,
            Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.20f, 0.80f, t))
        );
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (!paused && lastAutoPauseDay == days)
            lastAutoPauseDay = days - 1;
    }

    public void SetTime(int h, int m, int? d = null)
    {
        hours = Mathf.Clamp(h, 0, 23);
        minutes = Mathf.Clamp(m, 0, 59);
        if (d.HasValue) days = Mathf.Max(0, d.Value);

        ApplyActiveHourClamp(false);
        UpdateLighting();
    }

    public void SetTime(int d, float timeOfDay)
    {
        days = Mathf.Max(0, d);
        float totalMinutes = Mathf.Clamp01(timeOfDay) * 24f * 60f;
        int h = Mathf.FloorToInt(totalMinutes / 60f);
        int m = Mathf.FloorToInt(totalMinutes % 60f);
        hours = h;
        minutes = m;
        ApplyActiveHourClamp(false);
        UpdateLighting();
    }

    public void AddMinutes(int mins)
    {
        if (mins == 0) return;

        int targetTotal = CurrentTotalMinutes() + mins;
        int wrapped = targetTotal % (24 * 60);
        if (wrapped < 0) wrapped += 24 * 60;
        int daysDelta = targetTotal / (24 * 60);

        SetTimeFromTotal(wrapped, true);
        if (daysDelta != 0)
        {
            days += daysDelta;
            DayChanged?.Invoke(days);
        }

        ApplyActiveHourClamp(false);
        UpdateLighting();
    }

    private void ClampTime()
    {
        minutes = Mathf.Clamp(minutes, 0, 59);
        hours = Mathf.Clamp(hours, 0, 23);
        days = Mathf.Max(1, days);

        activeStartHour = Mathf.Clamp(activeStartHour, 0, 23);
        activeStartMinute = Mathf.Clamp(activeStartMinute, 0, 59);
        activeEndHour = Mathf.Clamp(activeEndHour, 0, 23);
        activeEndMinute = Mathf.Clamp(activeEndMinute, 0, 59);

        if (EndWindowTotalMinutes() < StartWindowTotalMinutes())
        {
            activeEndHour = activeStartHour;
            activeEndMinute = activeStartMinute;
        }
    }

    public TimePeriod CurrentTimePeriod()
    {
        if (morning.Includes(hours, minutes))
            return TimePeriod.Morning;
        else if (afternoon.Includes(hours, minutes))
            return TimePeriod.Afternoon;
        else if (evening.Includes(hours, minutes))
            return TimePeriod.Evening;
        else if(night.Includes(hours, minutes))
            return TimePeriod.Night;
        else
            return TimePeriod.Night;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        ClampTime();
        ApplyActiveHourClamp(true);
        if (!Application.isPlaying) UpdateLighting();
    }
#endif
}
