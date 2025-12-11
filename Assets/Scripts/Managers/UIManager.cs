using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject statsPanel;
    public GameObject inventoryPanel;

    [Header("HUD Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public Image hopeBar;
    public TextMeshProUGUI hopeText;

    [Header("Character Stats Display")]
    public Transform characterStatsContainer;
    public GameObject characterStatPrefab;

    [Header("Inventory Display")]
    public Transform inventoryGrid;
    public GameObject inventoryItemPrefab;

    public bool IsPaused { get; private set; }

    private Dictionary<CharacterStats, GameObject> characterStatDisplays = new Dictionary<CharacterStats, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (statsPanel != null)
            statsPanel.SetActive(false);
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    private void Start()
    {
        // Subscribe to events
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.MinuteChanged += UpdateTimeDisplay;
            UpdateTimeDisplay(TimeManager.Instance.hours, TimeManager.Instance.minutes, TimeManager.Instance.days);
        }

       // CharacterStats.OnStatChanged += OnCharacterStatChanged;

        //if (InventoryManager.Instance != null)
        //{
        //    InventoryManager.OnResourceChanged += OnResourceChanged;
        //}

       // RefreshAllUI();
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.MinuteChanged -= UpdateTimeDisplay;
        }

        //CharacterStats.OnStatChanged -= OnCharacterStatChanged;

        //if (InventoryManager.Instance != null)
        //{
        //    InventoryManager.OnResourceChanged -= OnResourceChanged;
        //}
    }

    private void Update()
    {
        // Toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        // Toggle inventory
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();

        // Toggle character stats
        if (Input.GetKeyDown(KeyCode.C))
            ToggleStats();

        // Update hope bar continuously
        UpdateHopeDisplay();
    }

    private void UpdateTimeDisplay(int hours, int minutes, int days)
    {
        if (timeText != null)
            timeText.text = $"{hours:00}:{minutes:00}";

        if (dayText != null)
            dayText.text = $"Day {days}";
    }

    private void UpdateHopeDisplay()
    {
        if (GameManager.Instance == null) return;

        if (hopeBar != null)
            hopeBar.fillAmount = GameManager.Instance.hope / 100f;

        if (hopeText != null)
            hopeText.text = $"Hope: {GameManager.Instance.hope}";
    }

    //private void OnCharacterStatChanged(CharacterStats character, string statName, int oldVal, int newVal)
    //{
    //    RefreshCharacterStats();
    //}

    //private void OnResourceChanged(ResourceData resource, int delta)
    //{
    //    RefreshInventory();
    //}

    //private void RefreshAllUI()
    //{
    //    RefreshCharacterStats();
    //    RefreshInventory();
    //    UpdateHopeDisplay();
    //}

    private void RefreshCharacterStats()
    {
        if (characterStatsContainer == null || GameManager.Instance == null) return;

        // Clear old displays
        foreach (Transform child in characterStatsContainer)
            Destroy(child.gameObject);

        characterStatDisplays.Clear();

        // Use helper method to get Character components
        foreach (CharacterStats character in GameManager.Instance.GetCharacterComponents())
        {
            if (characterStatPrefab != null)
            {
                GameObject statDisplay = Instantiate(characterStatPrefab, characterStatsContainer);
                characterStatDisplays[character] = statDisplay;
                UpdateCharacterStatDisplay(character, statDisplay);
            }
        }
    }

    private void UpdateCharacterStatDisplay(CharacterStats character, GameObject display)
    {
        // Update the prefab's text components
        TextMeshProUGUI[] texts = display.GetComponentsInChildren<TextMeshProUGUI>();

        if (texts.Length > 0)
        {
            texts[0].text = $"{character.name}\n" +
                           $"Health: {character.health}\n" +
                           $"Stability: {character.stability}\n" +
                           $"Learning: {character.learning}\n" +
                           $"Work: {character.workReadiness}\n" +
                           $"Trust: {character.trust}";
        }

        // Update stat bars if they exist
        Image[] images = display.GetComponentsInChildren<Image>();
        if (images.Length >= 5)
        {
            images[0].fillAmount = character.health / 100f;
            images[1].fillAmount = character.stability / 100f;
            images[2].fillAmount = character.learning / 100f;
            images[3].fillAmount = character.workReadiness / 100f;
            images[4].fillAmount = character.trust / 100f;
        }
    }

    //private void RefreshInventory()
    //{
    //    if (inventoryGrid == null || InventoryManager.Instance == null) return;

    //    // Clear old items
    //    foreach (Transform child in inventoryGrid)
    //        Destroy(child.gameObject);

    //    // Create new items
    //    foreach (var item in InventoryManager.Instance.inventory)
    //    {
    //        if (inventoryItemPrefab != null)
    //        {
    //            GameObject itemDisplay = Instantiate(inventoryItemPrefab, inventoryGrid);

    //            // Update display
    //            Image icon = itemDisplay.GetComponentInChildren<Image>();
    //            if (icon != null && item.resource.icon != null)
    //                icon.sprite = item.resource.icon;

    //            TextMeshProUGUI[] texts = itemDisplay.GetComponentsInChildren<TextMeshProUGUI>();
    //            if (texts.Length > 0)
    //            {
    //                texts[0].text = $"{item.resource.resourceName}\nx{item.quantity}";
    //            }
    //            // Set icon if available
    //            Image icon = itemDisplay.GetComponentInChildren<Image>();
    //            if (icon != null && resource.icon != null)
    //                icon.sprite = resource.icon;
    //        }
    //    }
    //}

    public void SetPause(bool pause)
    {
        IsPaused = pause;
        if (pausePanel != null)
            pausePanel.SetActive(pause);
        Time.timeScale = pause ? 0f : 1f;
    }

    public void TogglePause() => SetPause(!IsPaused);

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);
           // if (isActive) RefreshInventory();
        }
    }

    public void ToggleStats()
    {
        if (statsPanel != null)
        {
            bool isActive = !statsPanel.activeSelf;
            statsPanel.SetActive(isActive);
            if (isActive) RefreshCharacterStats();
        }
    }

    public void OnResumeButtonClicked() => SetPause(false);
    
    public void OnQuitButtonClicked() => Application.Quit();
}