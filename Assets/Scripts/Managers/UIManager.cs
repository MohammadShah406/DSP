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
    public InventoryUI inventoryUI;
    public GameObject taskPanel;
    public GameObject topStatsHUD; // Reference to the new HUD top stats panel
    public GameObject mainHUD; // Main HUD elements (time, day, hope)

    [Header("HUD Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public Slider hopeSlider;
    public TextMeshProUGUI hopeText;

    [Header("Character Stats Display")]
    public Image characterPicture;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterDescriptionText;

    [Header("Stat Sliders")]
    public float sliderLerpSpeed = 5f;
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

        pausePanel.SetActive(false);
        statsPanel.SetActive(false);
        taskPanel.SetActive(true);
        topStatsHUD.SetActive(false);
        mainHUD.SetActive(true);
    }

    private void Start()
    {
        // Subscribe to events
        TimeManager.Instance.MinuteChanged += UpdateTimeDisplay;
        UpdateTimeDisplay(TimeManager.Instance.hours, TimeManager.Instance.minutes, TimeManager.Instance.days);
        

        // Set slider ranges
        healthSlider.maxValue = 100;
        stabilitySlider.maxValue = 100;
        learningSlider.maxValue = 100;
        workReadinessSlider.maxValue = 100;
        trustSlider.maxValue = 100;
        nutritionSlider.maxValue = 100;
        hygieneSlider.maxValue = 100;
        energySlider.maxValue = 100;

        CharacterStats.OnAnyStatChanged += OnCharacterStatChanged;
    }

    private void OnDestroy()
    {
        TimeManager.Instance.MinuteChanged -= UpdateTimeDisplay;
        
        CharacterStats.OnAnyStatChanged -= OnCharacterStatChanged;
    }

    private void Update()
    {
        // Toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Close inventory first if it's open
            if (inventoryUI.inventoryPanel.activeSelf)
            {
                inventoryUI.Toggle();
                return;
            }

            if (CameraBehaviour.Instance.focussedTarget != null)
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
        if (statsPanel.activeSelf)
        {
            // Get currently selected character from camera
            if (CameraBehaviour.Instance.focussedTarget != null)
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

        hopeSlider.value = Mathf.Lerp(hopeSlider.value, normalizedHope, Time.deltaTime * sliderLerpSpeed);
        hopeText.text = $"Hope: {hopeValue}%";
    }

    private void UpdateCharacterStatsDisplay(CharacterStats character)
    {
        if (character == null) return;

        // Update character info
        characterNameText.text = character.characterName;
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

        characterDescriptionText.text = character.description ?? "Refugee";

        // Smoothly update all sliders
        float t = Time.deltaTime * sliderLerpSpeed;

        // Update Health
        healthSlider.value = Mathf.Lerp(healthSlider.value, character.Health, t); 
        healthPercentText.text = $"{Mathf.RoundToInt(healthSlider.value)}%";

        // Update Stability
        stabilitySlider.value = Mathf.Lerp(stabilitySlider.value, character.Stability, t);
        stabilityPercentText.text = $"{Mathf.RoundToInt(stabilitySlider.value)}%";

        // Update Learning
        learningSlider.value = Mathf.Lerp(learningSlider.value, character.Learning, t);
        learningPercentText.text = $"{Mathf.RoundToInt(learningSlider.value)}%";

        // Update Work Readiness
        workReadinessSlider.value = Mathf.Lerp(workReadinessSlider.value, character.WorkReadiness, t);
        workReadinessPercentText.text = $"{Mathf.RoundToInt(workReadinessSlider.value)}%";

        // Update Trust
        trustSlider.value = Mathf.Lerp(trustSlider.value, character.Trust, t);
        trustPercentText.text = $"{Mathf.RoundToInt(trustSlider.value)}%";

        // Update Nutrition
        nutritionSlider.value = Mathf.Lerp(nutritionSlider.value, character.Nutrition, t);
        nutritionPercentText.text = $"{Mathf.RoundToInt(nutritionSlider.value)}%";

        // Update Hygiene
        hygieneSlider.value = Mathf.Lerp(hygieneSlider.value, character.Hygiene, t);
        hygienePercentText.text = $"{Mathf.RoundToInt(hygieneSlider.value)}%";

        // Update Energy
        energySlider.value = Mathf.Lerp(energySlider.value, character.Energy, t);
        energyPercentText.text = $"{Mathf.RoundToInt(energySlider.value)}%";
    }

    public void SetPause(bool pause)
    {
        IsPaused = pause;
        pausePanel.SetActive(pause);
        Time.timeScale = pause ? 0f : 1f;
    }

    public void TogglePause() => SetPause(!IsPaused);

    public void ToggleInventory()
    {
        inventoryUI.Toggle();
    }

    public void OnInventoryOpened()
    {
        // Hide other UI elements when inventory is opened
        taskPanel.SetActive(false);
        statsPanel.SetActive(false);
        topStatsHUD.SetActive(false);
        mainHUD.SetActive(false);
    }

    public void OnInventoryClosed()
    {
        // Restore UI elements when inventory is closed
        taskPanel.SetActive(true);
        mainHUD.SetActive(true);
        
        // Re-show stats if a character is still focused
        if (CameraBehaviour.Instance.focussedTarget != null)
        {
            statsPanel.SetActive(true);
            topStatsHUD.SetActive(true);
        }
    }

    public void ToggleStats()
    {
        bool isActive = !statsPanel.activeSelf;
        statsPanel.SetActive(isActive);

        if (isActive)
        {
            // Show stats of currently selected character, or first character
            if (CameraBehaviour.Instance.focussedTarget != null)
            {
                CharacterStats selectedChar = CameraBehaviour.Instance.focussedTarget.GetComponent<CharacterStats>();
                if (selectedChar != null)
                {
                    currentCharacter = selectedChar;
                    UpdateCharacterStatsDisplay(currentCharacter);
                }
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
        SetPause(false);
       
    }

    public void OnQuitButtonClicked() => Application.Quit();
}