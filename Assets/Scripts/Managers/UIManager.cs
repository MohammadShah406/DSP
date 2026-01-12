using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Gets or sets the singleton instance of the class.
    public static UIManager Instance { get; private set; }

    /// <summary>
    /// Represents the UI panel displayed when the application or game is paused.
    /// Used to provide pause-related functionality such as displaying pause menu options
    /// or overlaying the screen to indicate a paused state.
    /// </summary>
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject statsPanel;
    public InventoryUI inventoryUI;
    public GameObject taskPanel;
    public GameObject topStatsHUD;
    public GameObject mainHUD;
    public CharacterCarousel characterCarousel;
    public GameObject DayEndUI;

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
    private bool IsPaused { get; set; }
    private CharacterStats _currentCharacter;
    private Transform _lastFocussedTarget;

    public enum UIState
    {
        Gameplay,
        Pause,
        Inventory,
        CharacterStats
    }

    public UIState CurrentState => _currentState;
    private UIState _currentState = UIState.Gameplay;
    private const float MaxStatValue = 100f;

    // Target values for smooth slider updates
    private float _targetHope;
    private float _targetHealth;
    private float _targetStability;
    private float _targetLearning;
    private float _targetWorkReadiness;
    private float _targetTrust;
    private float _targetNutrition;
    private float _targetHygiene;
    private float _targetEnergy;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Ensure inventory manager is active so it can initialize and listen for events
        inventoryUI.gameObject.SetActive(true);
        pausePanel.SetActive(false);
        statsPanel.SetActive(false);
        taskPanel.SetActive(true);
        topStatsHUD.SetActive(false);
        mainHUD.SetActive(true);
    }

    /// <summary>
    /// Initializes the UIManager by subscribing to events and setting up the default
    /// state of the UI elements, including timers and stat sliders.
    /// This method is automatically called by Unity when the script's GameObject
    /// becomes active in the scene.
    /// </summary>
    private void Start()
    {
        // Subscribe to events
        TimeManager.Instance.MinuteChanged += UpdateTimeDisplay;
        UpdateTimeDisplay(TimeManager.Instance.hours, TimeManager.Instance.minutes, TimeManager.Instance.days);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHopeChanged += OnHopeChanged;
            _targetHope = GameManager.Instance.Hope / MaxStatValue;
            hopeSlider.value = _targetHope;
            hopeText.text = $"Hope: {GameManager.Instance.Hope}%";
        }

        // Set slider ranges
        healthSlider.maxValue = MaxStatValue;
        stabilitySlider.maxValue = MaxStatValue;
        learningSlider.maxValue = MaxStatValue;
        workReadinessSlider.maxValue = MaxStatValue;
        trustSlider.maxValue = MaxStatValue;
        nutritionSlider.maxValue = MaxStatValue;
        hygieneSlider.maxValue = MaxStatValue;
        energySlider.maxValue = MaxStatValue;

        CharacterStats.OnAnyStatChanged += OnCharacterStatChanged;

        // Initialize UI State
        SwitchState(UIState.Gameplay);
    }

    /// <summary>
    /// Called when the object is destroyed. This method is typically used to release
    /// resources, unsubscribe from events, or perform cleanup operations before the
    /// object is removed from memory or the scene.
    /// </summary>
    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.MinuteChanged -= UpdateTimeDisplay;
        
        if (GameManager.Instance != null)
            GameManager.Instance.OnHopeChanged -= OnHopeChanged;
        
        CharacterStats.OnAnyStatChanged -= OnCharacterStatChanged;
    }

    private void Update()
    {
        // State-independent inputs
        if (InputManager.Instance.PauseInput || InputManager.Instance.DeselectInput)
        {
            HandleBackInput();
        }

        if (InputManager.Instance.InventoryInput)
        {
            ToggleInventory();
        }

        // Handle selection changes (replaces polling HandleStatsDisplay)
        UpdateSelectionState();

        // Smoothly update sliders if they haven't reached targets
        LerpSliders();
    }

    private void HandleBackInput()
    {
        if (_currentState == UIState.Inventory)
        {
            ToggleInventory();
        }
        else if (_currentState == UIState.CharacterStats)
        {
            CameraBehaviour.Instance.DeselectCharacter();
        }
        else if (_currentState == UIState.Pause)
        {
            SwitchState(UIState.Gameplay);
        }
        else
        {
            SwitchState(UIState.Pause);
        }
    }

    private void UpdateSelectionState()
    {
        Transform focussedTarget = CameraBehaviour.Instance.focussedTarget;

        if (focussedTarget != _lastFocussedTarget)
        {
            _lastFocussedTarget = focussedTarget;

            if (focussedTarget != null)
            {
                CharacterStats character = focussedTarget.GetComponent<CharacterStats>();
                if (character != null)
                {
                    _currentCharacter = character;

                    UpdateCharacterStatsDisplay(_currentCharacter, false); // Allow lerping when selection changes

                    if (characterCarousel != null)
                    {
                        characterCarousel.SetCurrentCharacter(_currentCharacter);
                    }

                    UpdateCharacterStatsDisplay(_currentCharacter, true); // Force immediate update of texts
                    SwitchState(UIState.CharacterStats);
                }
            }
            else
            {
                _currentCharacter = null;

                UpdateCharacterStatsDisplay(null);

                if (characterCarousel != null)
                {
                    characterCarousel.SetCurrentCharacter(null);
                }


                if (_currentState == UIState.CharacterStats)
                {
                    SwitchState(UIState.Gameplay);
                }
            }
        }
    }

    public void SwitchState(UIState newState)
    {
        UIState oldState = _currentState;
        _currentState = newState;

        // Panel visibility logic based on state
        pausePanel.SetActive(_currentState == UIState.Pause);
        statsPanel.SetActive(_currentState == UIState.CharacterStats);
        topStatsHUD.SetActive(_currentState == UIState.CharacterStats);

        // Ensure inventory panel is in sync with state
        if (inventoryUI != null && inventoryUI.inventoryPanel != null)
        {
            bool isInventory = _currentState == UIState.Inventory;
            
            // Ensure the manager script object is active if we are entering inventory state.
            // This prevents "Coroutine couldn't be started" errors.
            if (isInventory && !inventoryUI.gameObject.activeSelf)
            {
                inventoryUI.gameObject.SetActive(true);
            }

            inventoryUI.inventoryPanel.SetActive(isInventory);
            
            if (isInventory && oldState != UIState.Inventory)
            {
                inventoryUI.OnOpened();
            }
            else if (!isInventory && oldState == UIState.Inventory)
            {
                inventoryUI.OnClosed();
            }
        }
        
        // mainHUD and taskPanel might have more complex visibility rules
        // For now, keep them mostly on except in Pause or Inventory if that's the current behavior
        bool hideMainHUD = (_currentState == UIState.Pause || _currentState == UIState.Inventory);
        mainHUD.SetActive(!hideMainHUD);
        taskPanel.SetActive(!hideMainHUD);

        // Time management
        Time.timeScale = (_currentState == UIState.Pause) ? 0f : 1f;
        IsPaused = (_currentState == UIState.Pause);
    }

    private void LerpSliders()
    {
        float t = Time.deltaTime * sliderLerpSpeed;
        bool anyStatChanged = false;

        // Lerp Hope
        if (!Mathf.Approximately(hopeSlider.value, _targetHope))
        {
            hopeSlider.value = Mathf.Lerp(hopeSlider.value, _targetHope, t);
        }

        // Only lerp character stats if stats panel is visible
        if (statsPanel.activeSelf && _currentCharacter != null)
        {
            if (!Mathf.Approximately(healthSlider.value, _targetHealth))
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, _targetHealth, t);
                anyStatChanged = true;
            }
            
            if (!Mathf.Approximately(stabilitySlider.value, _targetStability))
            {
                stabilitySlider.value = Mathf.Lerp(stabilitySlider.value, _targetStability, t);
                anyStatChanged = true;
            }
            
            if (!Mathf.Approximately(learningSlider.value, _targetLearning))
            {
                learningSlider.value = Mathf.Lerp(learningSlider.value, _targetLearning, t);
                anyStatChanged = true;
            }
            
            if (!Mathf.Approximately(workReadinessSlider.value, _targetWorkReadiness))
            {
                workReadinessSlider.value = Mathf.Lerp(workReadinessSlider.value, _targetWorkReadiness, t);
                anyStatChanged = true;
            }
            
            if (!Mathf.Approximately(trustSlider.value, _targetTrust))
            {
                trustSlider.value = Mathf.Lerp(trustSlider.value, _targetTrust, t);
                anyStatChanged = true;
            }
            
            if (!Mathf.Approximately(nutritionSlider.value, _targetNutrition))
            {
                nutritionSlider.value = Mathf.Lerp(nutritionSlider.value, _targetNutrition, t);
                anyStatChanged = true;
            }
            
            if (!Mathf.Approximately(hygieneSlider.value, _targetHygiene))
            {
                hygieneSlider.value = Mathf.Lerp(hygieneSlider.value, _targetHygiene, t);
                anyStatChanged = true;
            }
            
            if (!Mathf.Approximately(energySlider.value, _targetEnergy))
            {
                energySlider.value = Mathf.Lerp(energySlider.value, _targetEnergy, t);
                anyStatChanged = true;
            }
            
            if (anyStatChanged)
            {
                UpdateStatTexts();
            }
        }
    }

    private void UpdateStatTexts()
    {
        healthPercentText.text = $"{Mathf.RoundToInt(healthSlider.value)}%";
        stabilityPercentText.text = $"{Mathf.RoundToInt(stabilitySlider.value)}%";
        learningPercentText.text = $"{Mathf.RoundToInt(learningSlider.value)}%";
        workReadinessPercentText.text = $"{Mathf.RoundToInt(workReadinessSlider.value)}%";
        trustPercentText.text = $"{Mathf.RoundToInt(trustSlider.value)}%";
        nutritionPercentText.text = $"{Mathf.RoundToInt(nutritionSlider.value)}%";
        hygienePercentText.text = $"{Mathf.RoundToInt(hygieneSlider.value)}%";
        energyPercentText.text = $"{Mathf.RoundToInt(energySlider.value)}%";
    }


    /// <summary>
    /// Updates the time display on the user interface to reflect the current time.
    /// </summary>
    private void UpdateTimeDisplay(int hours, int minutes, int days)
    {
        timeText.text = $"{hours:00}:{minutes:00}";
        dayText.text = $"Day {days}";
    }

    private void OnHopeChanged(int hopeValue)
    {
        _targetHope = hopeValue / MaxStatValue;
        hopeText.text = $"Hope: {hopeValue}%";
    }

    private void OnCharacterStatChanged(CharacterStats character)
    {
        if (_currentCharacter == character && statsPanel.activeSelf)
        {
            UpdateCharacterStatsDisplay(character);
        }
    }

    /// <summary>
    /// Updates the character's stats display on the user interface.
    /// </summary>
    public void UpdateCharacterStatsDisplay(CharacterStats character, bool immediate = false)
    {
        if (character == null)
        {
            ClearCharacterStatsDisplay();
            return;
        }
        _currentCharacter = character;

        // Ensure the panel is active if we have a character
        if (_currentState == UIState.Gameplay || _currentState == UIState.CharacterStats)
        {
            if (!statsPanel.activeSelf) SwitchState(UIState.CharacterStats);
        }
        // Update character info
        characterNameText.text = character.characterName;

        characterDescriptionText.text = character.description ?? "Refugee";

        // Set targets
        _targetHealth = character.Health;
        _targetStability = character.Stability;
        _targetLearning = character.Learning;
        _targetWorkReadiness = character.WorkReadiness;
        _targetTrust = character.Trust;
        _targetNutrition = character.Nutrition;
        _targetHygiene = character.Hygiene;
        _targetEnergy = character.Energy;

        if (immediate)
        {
            healthSlider.value = _targetHealth;
            stabilitySlider.value = _targetStability;
            learningSlider.value = _targetLearning;
            workReadinessSlider.value = _targetWorkReadiness;
            trustSlider.value = _targetTrust;
            nutritionSlider.value = _targetNutrition;
            hygieneSlider.value = _targetHygiene;
            energySlider.value = _targetEnergy;
            UpdateStatTexts();
        }
    }

    /// <summary>
    /// Toggles the inventory state between open and closed.
    /// </summary>
    private void ToggleInventory()
    {
        if (_currentState == UIState.Inventory)
            OnInventoryClosed();
        else
            OnInventoryOpened();
    }

    /// <summary>
    /// Triggered when the inventory is opened by the user or system.
    /// </summary>
    public void OnInventoryOpened()
    {
        SwitchState(UIState.Inventory);
    }

    /// <summary>
    /// Handles operations to perform when the inventory is closed.
    /// </summary>
    public void OnInventoryClosed()
    {
        if (CameraBehaviour.Instance.focussedTarget != null)
            SwitchState(UIState.CharacterStats);
        else
            SwitchState(UIState.Gameplay);
    }

    private void ClearCharacterStatsDisplay()
    {
        // Hide the stats panel completely
        statsPanel.SetActive(false);
        topStatsHUD.SetActive(false);

        // Reset current character reference
        _currentCharacter = null;

        // Reset all target values to 0
        _targetHealth = 0;
        _targetStability = 0;
        _targetLearning = 0;
        _targetWorkReadiness = 0;
        _targetTrust = 0;
        _targetNutrition = 0;
        _targetHygiene = 0;
        _targetEnergy = 0;
    }


    /// <summary>
    /// Toggles the state of a statistics feature.
    /// </summary>
    public void ToggleStats()
    {
        if (_currentState == UIState.CharacterStats)
            SwitchState(UIState.Gameplay);
        else if (CameraBehaviour.Instance.focussedTarget != null)
            SwitchState(UIState.CharacterStats);
    }

    /// <summary>
    /// Handles the click event for the Resume button.
    /// </summary>
    public void OnResumeButtonClicked()
    {
        SwitchState(UIState.Gameplay);
    }

    public void LoadMainMenu()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }

    public void ShowDayEndUI()
    {
        // Show the Day End UI and hide all other panels/HUDs
        Debug.Log("Day End UI is now visible.");
        DayEndUI.SetActive(true);

        // Hide other panels
        pausePanel.SetActive(false);
        statsPanel.SetActive(false);
        topStatsHUD.SetActive(false);

        // Hide inventory if present
        if (inventoryUI != null && inventoryUI.inventoryPanel != null)
        {
            inventoryUI.inventoryPanel.SetActive(false);
        }

        // Hide gameplay HUD panels
        mainHUD.SetActive(false);
        taskPanel.SetActive(false);
    }

    public void HideDayEndUI()
    {
        // Hide the Day End UI and re-enable the normal HUD
        Debug.Log("Day End UI is now hidden.");
        DayEndUI.SetActive(false);

        // Re-enable normal gameplay HUD panels
        mainHUD.SetActive(true);
        taskPanel.SetActive(true);
    }
}