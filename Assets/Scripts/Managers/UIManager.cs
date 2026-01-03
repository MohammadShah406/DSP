using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject statsPanel;
    public GameObject inventoryPanel;
    public GameObject taskPanel;
    public GameObject topStatsHUD; // Reference to the new HUD top stats panel

    [Header("HUD Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public Slider hopeSlider;
    public TextMeshProUGUI hopeText;

    [Header("Character Stats Display")]
    public Image characterPicture;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterDescriptionText;
    public TextMeshProUGUI primaryAttributeText;
    public TextMeshProUGUI growthRateText;

    [Header("Stat Sliders")]
    public Slider healthSlider;
    public TextMeshProUGUI healthPercentText;
    public Slider stabilitySlider;
    public TextMeshProUGUI stabilityPercentText;
    public Slider learningSlider;
    public TextMeshProUGUI learningPercentText;
    public Slider workReadinessSlider;
    public TextMeshProUGUI workReadinessPercentText;
    public Slider trustSlider;
    public TextMeshProUGUI trustPercentText;
    public Slider nutritionSlider;
    public TextMeshProUGUI nutritionPercentText;
    public Slider hygieneSlider;
    public TextMeshProUGUI hygienePercentText;
    public Slider energySlider;
    public TextMeshProUGUI energyPercentText;

    [Header("Inventory Display")]
    public Transform inventoryGrid;
    public GameObject inventoryItemPrefab;

    [Header("Item Detail Panel")]
    public GameObject itemDetailPanel;
    public TextMeshProUGUI detailItemNameText;
    public Image detailItemIcon;
    public TextMeshProUGUI detailItemQuantityText;
    public TextMeshProUGUI detailItemDescriptionText;

    public bool IsPaused { get; private set; }

    private CharacterStats currentCharacter;
    private Transform lastFocussedTarget;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
        if (taskPanel != null) taskPanel.SetActive(true);
        if (topStatsHUD != null) topStatsHUD.SetActive(false);
    }

    private void Start()
    {
        SetupInventoryGrid();
        // Subscribe to events
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.MinuteChanged += UpdateTimeDisplay;
            UpdateTimeDisplay(TimeManager.Instance.hours, TimeManager.Instance.minutes, TimeManager.Instance.days);
        }

        // Set slider ranges
        if (healthSlider != null) healthSlider.maxValue = 100;
        if (stabilitySlider != null) stabilitySlider.maxValue = 100;
        if (learningSlider != null) learningSlider.maxValue = 100;
        if (workReadinessSlider != null) workReadinessSlider.maxValue = 100;
        if (trustSlider != null) trustSlider.maxValue = 100;
        if (nutritionSlider != null) nutritionSlider.maxValue = 100;
        if (hygieneSlider != null) hygieneSlider.maxValue = 100;
        if (energySlider != null) energySlider.maxValue = 100;

        CharacterStats.OnAnyStatChanged += OnCharacterStatChanged;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourcesChanged += UpdateInventoryDisplay;
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.MinuteChanged -= UpdateTimeDisplay;
        }

        CharacterStats.OnAnyStatChanged -= OnCharacterStatChanged;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourcesChanged -= UpdateInventoryDisplay;
        }
    }

    private void Update()
    {
        // Toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CameraBehaviour.Instance != null && CameraBehaviour.Instance.focussedTarget != null)
            {
                // Character selected - let camera deselect it
                // (Camera already has deselect logic on Escape via DeselectInput)
                // Stats panel will auto-hide via HandleStatsDisplay()
            }
            else
            {
                // Nothing selected - open pause menu
                TogglePause();
            }
        }

        // Toggle inventory
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();

        // Toggle character stats
        HandleStatsDisplay();

        //  If stats panel is open, update with currently selected character
        if (statsPanel != null && statsPanel.activeSelf)
        {
            // Get currently selected character from camera
            if (CameraBehaviour.Instance != null && CameraBehaviour.Instance.focussedTarget != null)
            {
                CharacterStats selectedChar = CameraBehaviour.Instance.focussedTarget.GetComponent<CharacterStats>();

                // Update if character changed or stats changed
                if (selectedChar != null)
                {
                    currentCharacter = selectedChar;
                    UpdateCharacterStatsDisplay(currentCharacter);
                }
            }
            else if (currentCharacter != null)
            {
                // Still update current character even if not focused (for live stat changes)
                UpdateCharacterStatsDisplay(currentCharacter);
            }
        }

        // Update hope bar continuously
        UpdateHopeDisplay();
    }

    private void HandleStatsDisplay()
    {
        if (CameraBehaviour.Instance == null || statsPanel == null) return;

        Transform focussedTarget = CameraBehaviour.Instance.focussedTarget;

        // Check if selection changed
        if (focussedTarget != lastFocussedTarget)
        {
            lastFocussedTarget = focussedTarget;

            if (focussedTarget != null)
            {
                // Character selected - show stats
                CharacterStats character = focussedTarget.GetComponent<CharacterStats>();
                if (character != null)
                {
                    currentCharacter = character;
                    statsPanel.SetActive(true);
                    if (topStatsHUD != null) topStatsHUD.SetActive(true);
                    UpdateCharacterStatsDisplay(currentCharacter);
                }
            }
            else
            {
                // No character selected - hide stats
                statsPanel.SetActive(false);
                if (topStatsHUD != null) topStatsHUD.SetActive(false);
                currentCharacter = null;
            }
        }
        else if (focussedTarget != null && currentCharacter != null && statsPanel.activeSelf)
        {
            // Character still selected - update stats (for live changes)
            UpdateCharacterStatsDisplay(currentCharacter);
        }
    }

    private void SetupInventoryGrid()
    {
        if (inventoryGrid == null) return;

        // Ensure we have a GridLayoutGroup for "side-by-side then vertical"
        GridLayoutGroup grid = inventoryGrid.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = inventoryGrid.gameObject.AddComponent<GridLayoutGroup>();

        grid.cellSize = new Vector2(100, 100); // Default size, user can adjust in Inspector
        grid.spacing = new Vector2(10, 10);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal; // Side-by-side first
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraintCount = 0; // Flexible based on width

        // Ensure we have a ContentSizeFitter so the grid grows to fit items
        ContentSizeFitter fitter = inventoryGrid.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = inventoryGrid.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private void UpdateTimeDisplay(int hours, int minutes, int days)
    {
        timeText.text = $"{hours:00}:{minutes:00}";
        dayText.text = $"Day {days}";
    }

    private void UpdateHopeDisplay()
    {
        if (GameManager.Instance == null) return;

        float hopeValue = GameManager.Instance.hope;
        float normalizedHope = hopeValue / 100f;

        if (hopeSlider != null)
            hopeSlider.value = normalizedHope;

        if (hopeText != null)
            hopeText.text = $"Hope: {hopeValue}%";
    }

    private void UpdateCharacterStatsDisplay(CharacterStats character)
    {
        if (character == null) return;

        // Update character info
        characterNameText.text = character.characterName;

        if (characterPicture != null)
        {
            if (character.characterIcon != null)
            {
                characterPicture.sprite = character.characterIcon;
                characterPicture.enabled = true;
            }
            else
            {
                // Optionally hide or set a default if no icon
                characterPicture.enabled = false;
            }
        }

        characterDescriptionText.text = character.description ?? "Refugee";
        primaryAttributeText.text = $"Primary: {character.primaryAttribute}";
        growthRateText.text = $"Growth Rate: {character.growthRate:F1}x";

        // Update Health
        if (healthSlider != null)
            healthSlider.value = character.health;
        if (healthPercentText != null)
            healthPercentText.text = $"{character.health}%";

        // Update Stability
        if (stabilitySlider != null)
            stabilitySlider.value = character.stability;
        if (stabilityPercentText != null)
            stabilityPercentText.text = $"{character.stability}%";

        // Update Learning
        if (learningSlider != null)
            learningSlider.value = character.learning;
        if (learningPercentText != null)
            learningPercentText.text = $"{character.learning}%";

        // Update Work Readiness
        if (workReadinessSlider != null)
            workReadinessSlider.value = character.workReadiness;
        if (workReadinessPercentText != null)
            workReadinessPercentText.text = $"{character.workReadiness}%";

        // Update Trust
        if (trustSlider != null)
            trustSlider.value = character.trust;
        if (trustPercentText != null)
            trustPercentText.text = $"{character.trust}%";

        // Update Nutrition
        if (nutritionSlider != null)
            nutritionSlider.value = character.nutrition;
        if (nutritionPercentText != null)
            nutritionPercentText.text = $"{character.nutrition}%";

        // Update Hygiene
        if (hygieneSlider != null)
            hygieneSlider.value = character.hygiene;
        if (hygienePercentText != null)
            hygienePercentText.text = $"{character.hygiene}%";

        // Update Energy
        if (energySlider != null)
            energySlider.value = character.energy;
        if (energyPercentText != null)
            energyPercentText.text = $"{character.energy}%";
    }

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
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            // Hide other UI elements when inventory is opened
            if (taskPanel != null) taskPanel.SetActive(false);
            if (statsPanel != null) statsPanel.SetActive(false);
            if (topStatsHUD != null) topStatsHUD.SetActive(false);
            
            // Note: timeText and dayText are usually part of HUD but might be separate
            // If they are under a different parent, they might still be visible.
            // But usually topStatsHUD or taskPanel covers most HUD elements.

            // Force update when opening, even if it's not active in hierarchy yet
            UpdateInventoryDisplay(true);
        }
        else
        {
            // Restore UI elements when inventory is closed
            if (taskPanel != null) taskPanel.SetActive(true);
            
            // Re-show stats if a character is still focused
            if (currentCharacter != null)
            {
                if (statsPanel != null) statsPanel.SetActive(true);
                if (topStatsHUD != null) topStatsHUD.SetActive(true);
            }

            // Close details when inventory is closed
            if (itemDetailPanel != null) itemDetailPanel.SetActive(false);
        }
    }

    public void UpdateInventoryDisplay() => UpdateInventoryDisplay(false);

    public void UpdateInventoryDisplay(bool force)
    {
        if (inventoryGrid == null || GameManager.Instance == null) return;

        // If inventory is not open, don't bother updating (optional optimization)
        if (!force && inventoryPanel != null && !inventoryPanel.activeInHierarchy) return;

        // Clear existing items
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }

        // Spawn new items from GameManager resources
        foreach (var res in GameManager.Instance.resources)
        {
            if (res.quantity <= 0) continue;

            GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryGrid);
            InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
            ItemData data = GameManager.Instance.GetItemData(res.resourceName);

            if (itemUI != null)
            {
                itemUI.Setup(data, res.quantity);
            }
            else
            {
                // Fallback if no InventoryItemUI script is attached
                var text = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                var image = itemObj.GetComponentsInChildren<Image>().FirstOrDefault(img => img.gameObject != itemObj);

                if (text != null)
                {
                    string typeStr = data != null ? $" [{data.itemType}]" : "";
                    text.text = $"{res.resourceName}{typeStr}: {res.quantity}";
                }

                if (image != null && data != null && data.icon != null)
                {
                    image.sprite = data.icon;
                }
            }
        }
    }

    public void ShowItemDetails(ItemData data, int quantity)
    {
        if (itemDetailPanel == null) return;

        itemDetailPanel.SetActive(true);

        if (detailItemNameText != null)
            detailItemNameText.text = data.itemName;

        if (detailItemIcon != null)
        {
            if (data.icon != null)
            {
                detailItemIcon.sprite = data.icon;
                detailItemIcon.enabled = true;
            }
            else
            {
                detailItemIcon.enabled = false;
            }
        }

        if (detailItemQuantityText != null)
            detailItemQuantityText.text = $"Quantity: {quantity}";

        if (detailItemDescriptionText != null)
            detailItemDescriptionText.text = data.description;
    }

    public void ToggleStats()
    {
        bool isActive = !statsPanel.activeSelf;
        statsPanel.SetActive(isActive);

        if (isActive)
        {
            // Show stats of currently selected character, or first character
            if (CameraBehaviour.Instance != null && CameraBehaviour.Instance.focussedTarget != null)
            {
                CharacterStats selectedChar = CameraBehaviour.Instance.focussedTarget.GetComponent<CharacterStats>();
                if (selectedChar != null)
                {
                    currentCharacter = selectedChar;
                    UpdateCharacterStatsDisplay(currentCharacter);
                }
            }
            else
            {
                
            }
        }
    }

    public void OnCharacterStatChanged(CharacterStats character)
    {
        // Only update if it's the currently displayed character
        if (character == currentCharacter && statsPanel.activeSelf)
        {
            UpdateCharacterStatsDisplay(character);
        }
    }

    public void OnResumeButtonClicked()
    {
        Debug.Log("Resuming game from pause menu.");
        SetPause(false);
       
    }

    public void OnQuitButtonClicked() => Application.Quit();
}