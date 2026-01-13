using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class DonationManager : MonoBehaviour
{
    public static DonationManager instance;

    [Header("Donation data")]
    [SerializeField] public List<ItemData> allDonationsItemData = new();
    [SerializeField] private List<ResourceData> allDonationsResource = new();

    [SerializeField] private List<GameObject> donationGameObjects = new();



    private bool subscribed = false;

    // Runtime cache
    private HashSet<string> fired = new HashSet<string>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //LoadFiredDonations();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Try to subscribe to TimeManager events
        TrySubscribe();
    }

    private void OnDisable()
    {
        // Unsubscribe from TimeManager events
        Unsubscribe();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TrySubscribe();
    }

    /// <summary>
    /// Tries to subscribe to the TimeManager events
    /// </summary>
    private void TrySubscribe()
    {
        if (subscribed) return;

        if (TimeManager.Instance == null) return;

        TimeManager.Instance.MinuteChanged += OnMinuteChanged;
        subscribed = true;

        // Initial check
        CheckCurrentDonations(TimeManager.Instance.days, TimeManager.Instance.hours, TimeManager.Instance.minutes);
    }

    /// <summary>
    /// Unsubscribes from the TimeManager events    
    /// </summary>
    private void Unsubscribe()
    {
        if (!subscribed) return;
        if (TimeManager.Instance == null) return;

        TimeManager.Instance.MinuteChanged -= OnMinuteChanged;
        subscribed = false;
    }

    /// <summary>
    /// Called when the minute changes
    /// </summary>
    private void OnMinuteChanged(int hours, int minutes, int days)
    {
        Debug.Log($"DonationManager: Time changed to Day {days}, Hour {hours}, Minute {minutes}");
        CheckCurrentDonations(days, hours, minutes);
    }

    /// <summary>
    /// Checks the current donations based on the time
    /// </summary>
    private void CheckCurrentDonations(int d, int h, int m)
    {
        // Ensure we have donations configured
        if (allDonationsItemData == null || allDonationsItemData.Count == 0 || allDonationsResource == null || allDonationsResource.Count == 0)
        {
            Debug.LogWarning("DonationManager: No donations configured.");
            return;
        }

        // Day 1 @ 16:00 -> Donation 1
        // Day 2 @ 10:00 -> Donation 2

        Debug.Log($"DonationManager: Checking donations for Day {d}, Hour {h}, Minute {m}");

        TriggerOnce(d, h, m, donationIndex: 0, expectedDay: 1, expectedHour: 16, expectedMinute: 0);
        TriggerOnce(d, h, m, donationIndex: 1, expectedDay: 2, expectedHour: 10, expectedMinute: 0);
    }

    /// <summary>
    /// Triggers a donation only once at the specified time
    /// </summary>
    private void TriggerOnce(int currentDay, int currentHour, int currentMinute,
                             int donationIndex, int expectedDay, int expectedHour, int expectedMinute)
    {
        if (currentDay != expectedDay || currentHour != expectedHour || currentMinute != expectedMinute)
            return;

        string donationKey = GetDonationKey(donationIndex, expectedDay, expectedHour, expectedMinute);
        if (fired.Contains(donationKey)) return;

        Debug.Log($"DonationManager: Triggering donation #{donationIndex + 1} for Day {expectedDay} at {expectedHour:00}:{expectedMinute:00}");

        // Mark as fired
        fired.Add(donationKey);
        PlayerPrefs.SetInt(donationKey, 1);
        PlayerPrefs.Save();

        if (donationIndex >= 0 && donationIndex < allDonationsItemData.Count && allDonationsItemData[donationIndex] != null)
        {
            Debug.Log($"Get a donation! Donation #{donationIndex + 1} added to inventory.");
            // Add the donation item to the inventory
            GameManager.Instance.itemDatabase.Add(allDonationsItemData[donationIndex]);
            GameManager.Instance.resources.Add(allDonationsResource[donationIndex]);

            if (AudioPlayer.Instance != null && AudioLibrary.Instance != null)
            {
                AudioPlayer.Instance.Play(AudioLibrary.Instance.GetSfx("donationreceived"));
            }

        }
        else
        {
            Debug.LogWarning($"Donation index {donationIndex} is missing in allDonations list.");
        }
    }
    private string GetDonationKey(int donationIndex, int day, int hour, int minute)
    {
        return $"donation_{donationIndex}_day{day}_{hour:00}{minute:00}";
    }

    /// <summary>
    /// Loads previously fired donation keys into memory
    /// </summary>
    private void LoadFiredDonations()
    {
        fired.Clear();

        Debug.Log("DonationManager: Loading fired donation keys from PlayerPrefs.");

        // iterate through all donations and check their keys
        for (int i = 0; i < allDonationsItemData.Count; i++)
        {
            TryLoadKey(i, 1, 16, 0);
            TryLoadKey(i, 2, 10, 0);
        }
    }

    /// <summary>
    /// Tries to load a donation key from PlayerPrefs
    /// </summary>
    private void TryLoadKey(int index, int day, int hour, int minute)
    {
        string key = GetDonationKey(index, day, hour, minute);
        if (PlayerPrefs.GetInt(key, 0) == 1)
        {
            fired.Add(key);
            Debug.Log($"DonationManager: Loaded fired donation key: {key}");
        }
    }

    public void PlaceItem(ItemData data)
    {
        Debug.Log("DonationManager: PlaceItem called.");

        foreach (GameObject obj in donationGameObjects)
        {
            if (obj.name == data.itemName)
            {
                Debug.Log($"DonationManager: Found matching donation GameObject for item '{data.itemName}'. Activating placement mode.");
                obj.SetActive(true);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.upgradesDone += 1;
                }

                if (AudioPlayer.Instance != null && AudioLibrary.Instance != null)
                {
                    AudioPlayer.Instance.Play(AudioLibrary.Instance.GetSfx("upgradedone"));
                }

                return;
            }
        }

    }
}
