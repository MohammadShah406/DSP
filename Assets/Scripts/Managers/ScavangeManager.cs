using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScavengerItem
{
    public string eventName;
    public GameObject targetBin;
    public List<ResourceData> resourcesToAdd;
    
    [Header("Schedule")]
    public int day = 1;
    public int hour = 12;
    public int minute = 0;

    [HideInInspector]
    public bool hasFired = false;
}

public class ScavangeManager : MonoBehaviour
{
    public static ScavangeManager Instance { get; private set; }

    [Header("Scavenge Events")]
    public List<ScavengerItem> scavangableItems = new List<ScavengerItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.MinuteChanged += OnMinuteChanged;
        }
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.MinuteChanged -= OnMinuteChanged;
        }
    }

    private void Start()
    {
        // Fallback if TimeManager wasn't ready in OnEnable
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.MinuteChanged -= OnMinuteChanged; // Avoid double subscription
            TimeManager.Instance.MinuteChanged += OnMinuteChanged;
        }
    }

    private void OnMinuteChanged(int hours, int minutes, int days)
    {
        CheckScavengeEvents(days, hours, minutes);
    }

    private void CheckScavengeEvents(int currentDay, int currentHour, int currentMinute)
    {
        foreach (var item in scavangableItems)
        {
            if (item.hasFired) continue;

            if (item.day == currentDay && item.hour == currentHour && item.minute == currentMinute)
            {
                TriggerScavengeEvent(item);
            }
        }
    }

    private void TriggerScavengeEvent(ScavengerItem item)
    {
        if (item.targetBin == null)
        {
            Debug.LogWarning($"[ScavangeManager] Event '{item.eventName}' has no target bin assigned!");
            item.hasFired = true;
            return;
        }

        ResourceContainer container = item.targetBin.GetComponent<ResourceContainer>();
        if (container == null)
        {
            // The user said they will add the script, but if it's missing, we add it now
            container = item.targetBin.AddComponent<ResourceContainer>();
            Debug.Log($"[ScavangeManager] Added ResourceContainer to {item.targetBin.name} for event '{item.eventName}'");
        }

        container.AddResources(item.resourcesToAdd);
        item.hasFired = true;
        
        Debug.Log($"[ScavangeManager] Added {item.resourcesToAdd.Count} resources to {item.targetBin.name} at Day {item.day} {item.hour:00}:{item.minute:00}");
    }
}
