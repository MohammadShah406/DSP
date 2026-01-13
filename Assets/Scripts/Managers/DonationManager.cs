using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class DonationManager : MonoBehaviour
{
    public static DonationManager Instance;

    [Header("Donation data")]
    [SerializeField] public List<ItemData> allDonationsItemData = new();
    [SerializeField] private List<ResourceData> allDonationsResource = new();

    [SerializeField] private List<GameObject> donationGameObjects = new();
    [SerializeField] private List<ItemData> TempInventoryItemData = new();
    [SerializeField] private List<ResourceData> TempInventoryResourceData = new();

    public int hours;
    public int minutes;
    public int days;

    private bool _subscribed = false;

    // Runtime cache
    private HashSet<string> fired = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        if (_subscribed) return;

        if (TimeManager.Instance == null) return;

        TimeManager.Instance.MinuteChanged += OnMinuteChanged;
        _subscribed = true;

        // Initial check
        CheckCurrentDonations(TimeManager.Instance.days, TimeManager.Instance.hours, TimeManager.Instance.minutes);
    }

    /// <summary>
    /// Unsubscribes from the TimeManager events    
    /// </summary>
    private void Unsubscribe()
    {
        if (!_subscribed) return;
        if (TimeManager.Instance == null) return;

        TimeManager.Instance.MinuteChanged -= OnMinuteChanged;
        _subscribed = false;
    }

    /// <summary>
    /// Called when the minute changes
    /// </summary>
    private void OnMinuteChanged(int hours, int minutes, int days)
    {
        Debug.Log($"DonationManager: Time changed to Day {days}, Hour {hours}, Minute {minutes}");
        CheckCurrentDonations(days, hours, minutes);

        this.hours = hours;
        this.minutes = minutes;
        this.days = days;
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
        TriggerOnce(d, h, m, donationIndex: 1, expectedDay: 2, expectedHour: 12, expectedMinute: 0);
        TriggerOnce(d, h, m, donationIndex: 2, expectedDay: 2, expectedHour: 15, expectedMinute: 0);

    }

    public void TryCheckDonations()
    {
        TryAddDonationToInventory();    
    }

    /// <summary>
    /// Triggers a donation only once at the specified time
    /// </summary>
    private void TriggerOnce(int currentDay, int currentHour, int currentMinute,
                             int donationIndex, int expectedDay, int expectedHour, int expectedMinute)
    {
        if (currentDay != expectedDay || currentHour != expectedHour || currentMinute != expectedMinute)
        { 
            Debug.Log($"DonationManager: Not time for donation #{donationIndex + 1} yet. Current time: Day {currentDay}, {currentHour:00}:{currentMinute:00}. Expected time: Day {expectedDay}, {expectedHour:00}:{expectedMinute:00}");
            return;
        }

        if(TempInventoryItemData.Count > donationIndex)
        {
            Debug.Log($"DonationManager: Donation #{donationIndex + 1} already triggered.");
            return;
        }

        TempInventoryItemData.Add(allDonationsItemData[donationIndex]);
        TempInventoryResourceData.Add(allDonationsResource[donationIndex]);
    }
    private string GetDonationKey(int donationIndex)
    {
        return $"donation_{donationIndex}";
    }

    public void TryAddDonationToInventory()
    {

        for (int donationIndex = 0; donationIndex < TempInventoryItemData.Count; donationIndex++)
        {
            string donationKey = GetDonationKey(donationIndex);
            if (fired.Contains(donationKey)) continue;

            Debug.Log($"DonationManager: Triggering donation #{donationIndex}");

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
            }

            if (AudioPlayer.Instance != null && AudioLibrary.Instance != null)
            {
                AudioPlayer.Instance.Play(AudioLibrary.Instance.GetSfx("donationreceived"));
            }


            else
            {
                Debug.LogWarning($"Donation index {donationIndex} is missing in allDonations list.");
            }
        }
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
        string key = GetDonationKey(index);
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
