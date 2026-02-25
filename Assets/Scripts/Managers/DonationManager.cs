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
            return;
        }

        // Instead of exact minutes, we should check if the current time is AT or AFTER the expected time
        // and if it hasn't been added to TempInventory yet AND hasn't been collected (fired) yet.
        
        CheckAndPrepareDonation(d, h, m, index: 0, expDay: 1, expHour: 16, expMinute: 0);
        CheckAndPrepareDonation(d, h, m, index: 1, expDay: 2, expHour: 12, expMinute: 0);
        CheckAndPrepareDonation(d, h, m, index: 2, expDay: 2, expHour: 15, expMinute: 0);
    }

    private void CheckAndPrepareDonation(int curDay, int curHour, int curMin, int index, int expDay, int expHour, int expMinute)
    {
        // 1. If we already have this in Temp inventory (uncollected), skip
        if (index < allDonationsResource.Count && allDonationsResource[index] != null)
        {
            var res = allDonationsResource[index];
            if (TempInventoryResourceData.Exists(r => r.resourceName == res.resourceName)) return;
        }
        else return;

        // 2. If it was already collected (fired), skip
        string collectionKey = $"collected_{allDonationsResource[index].resourceName}_{index}";
        if (fired.Contains(collectionKey)) return;

        // 3. Check if time has passed
        bool timePassed = false;
        if (curDay > expDay) timePassed = true;
        else if (curDay == expDay)
        {
            if (curHour > expHour) timePassed = true;
            else if (curHour == expHour && curMin >= expMinute) timePassed = true;
        }

        if (timePassed)
        {
            if (index < allDonationsItemData.Count && allDonationsItemData[index] != null)
            {
                TempInventoryItemData.Add(allDonationsItemData[index]);
                TempInventoryResourceData.Add(allDonationsResource[index]);
            }
        }
    }

    public void TryCheckDonations()
    {
        // Before adding to inventory, re-check current time just in case
        if (TimeManager.Instance != null)
        {
            CheckCurrentDonations(TimeManager.Instance.days, TimeManager.Instance.hours, TimeManager.Instance.minutes);
        }
        
        TryAddDonationToInventory();    
    }
    private string GetDonationKey(int donationIndex)
    {
        return $"donation_{donationIndex}";
    }

    public void TryAddDonationToInventory()
    {
        // Use a list to store items to show in a single UI window if possible,
        // or just trigger them one by one as it was doing.
        List<ResourceData> itemsCollectedThisTime = new List<ResourceData>();

        for (int i = 0; i < TempInventoryResourceData.Count; i++)
        {
            var resToCollect = TempInventoryResourceData[i];
            
            // Check if this specific resource was already collected (fired)
            // We use a key based on the resource name and its index to be unique
            string collectionKey = $"collected_{resToCollect.resourceName}_{i}";
            if (fired.Contains(collectionKey)) continue;

            // Mark as fired
            fired.Add(collectionKey);
            PlayerPrefs.SetInt(collectionKey, 1);
            PlayerPrefs.Save();

            // Find matching ItemData
            ItemData matchingItem = allDonationsItemData.Find(item => item != null && item.itemName == resToCollect.resourceName);

            if (matchingItem != null)
            {
                // Add to inventory
                GameManager.Instance.itemDatabase.Add(matchingItem);
                GameManager.Instance.AddResource(resToCollect.resourceName, resToCollect.quantity);
                
                itemsCollectedThisTime.Add(resToCollect);
            }
        }

        if (itemsCollectedThisTime.Count > 0)
        {
            // Show UI
            /*
            if (UIManager.Instance != null && UIManager.Instance.ScavengeUI != null)
            {
                var scavUI = UIManager.Instance.ScavengeUI.GetComponent<ScavengerUI>();
                if (scavUI != null)
                {
                    scavUI.Setup(itemsCollectedThisTime, true);
                }
            }
            */

            // Toggle DonationUI instead
            if (UIManager.Instance != null && UIManager.Instance.donationUI != null)
            {
                UIManager.Instance.donationUI.SetActive(true);
            }

            if (AudioPlayer.Instance != null && AudioLibrary.Instance != null)
            {
                AudioPlayer.Instance.Play(AudioLibrary.Instance.GetSfx("donationreceived"));
            }
        }
    }

    /// <summary>
    /// Loads previously fired donation keys into memory
    /// </summary>
    private void LoadFiredDonations()
    {
        fired.Clear();
        Debug.Log("DonationManager: Loading fired donation keys.");

        for (int i = 0; i < allDonationsResource.Count; i++)
        {
            string resName = allDonationsResource[i].resourceName;
            string collectionKey = $"collected_{resName}_{i}";
            if (PlayerPrefs.GetInt(collectionKey, 0) == 1)
            {
                fired.Add(collectionKey);
                Debug.Log($"DonationManager: Loaded fired donation key: {collectionKey}");
            }
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
