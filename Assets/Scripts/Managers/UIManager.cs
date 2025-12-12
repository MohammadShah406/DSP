using System.Collections.Generic;
using System;
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

    [Header("HUD Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public Image hopeBar;
    public TextMeshProUGUI hopeText;

    [Header("Character Stats Display")]
    public Image characterPicture;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterDescriptionText;

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

    [Header("Inventory Display")]
    public Transform inventoryGrid;
    public GameObject inventoryItemPrefab;

    public bool IsPaused { get; private set; }

    private CharacterStats currentCharacter;

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

        // Set slider ranges
        if (healthSlider != null) healthSlider.maxValue = 100;
        if (stabilitySlider != null) stabilitySlider.maxValue = 100;
        if (learningSlider != null) learningSlider.maxValue = 100;
        if (workReadinessSlider != null) workReadinessSlider.maxValue = 100;
        if (trustSlider != null) trustSlider.maxValue = 100;

        CharacterStats.OnAnyStatChanged += OnCharacterStatChanged;
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.MinuteChanged -= UpdateTimeDisplay;
        }

        CharacterStats.OnAnyStatChanged -= OnCharacterStatChanged;
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

        // ✅ AUTO-UPDATE: If stats panel is open, update with currently selected character
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

    private void UpdateCharacterStatsDisplay(CharacterStats character)
    {
        if (character == null) return;

        // Update character info
        if (characterNameText != null)
            characterNameText.text = character.characterName;

        if (characterDescriptionText != null)
            characterDescriptionText.text = character.description ?? "Refugee";

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
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);
        }
    }

    public void ToggleStats()
    {
        if (statsPanel != null)
        {
            bool isActive = !statsPanel.activeSelf;
            statsPanel.SetActive(isActive);

            if (isActive)
            {
                // ✅ Show stats of currently selected character, or first character
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
    }

    private void OnCharacterStatChanged(CharacterStats character)
    {
        // Only update if it's the currently displayed character
        if (character == currentCharacter && statsPanel != null && statsPanel.activeSelf)
        {
            UpdateCharacterStatsDisplay(character);
        }
    }

    public void OnResumeButtonClicked() => SetPause(false);

    public void OnQuitButtonClicked() => Application.Quit();
}