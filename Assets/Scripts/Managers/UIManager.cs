using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Gets or sets the singleton instance of the class.
    /// </summary>
    /// <remarks>
    /// This property provides access to the single, shared instance of the class,
    /// ensuring that only one instance is created and used throughout the application.
    /// Typically used in the Singleton design pattern.
    /// </remarks>
    public static UIManager Instance { get; private set; }

    /// <summary>
    /// Represents the UI panel displayed when the application or game is paused.
    /// Used to provide pause-related functionality such as displaying pause menu options
    /// or overlaying the screen to indicate a paused state.
    /// </summary>
    [Header("Panels")]
    public GameObject pausePanel;

    /// <summary>
    /// Represents the UI panel that displays statistical information in the application.
    /// </summary>
    public GameObject statsPanel;

    /// <summary>
    /// Represents the user interface component responsible for displaying and managing the inventory system.
    /// This variable encapsulates the functionality required to interact with and update
    /// the visual representation of the inventory in the application.
    /// </summary>
    public InventoryUI inventoryUI;

    /// <summary>
    /// Represents a user interface panel designed to display and manage tasks.
    /// This panel serves as a container for task-related elements such as controls,
    /// lists, or interactive components that allow users to view or modify tasks.
    /// </summary>
    public GameObject taskPanel;

    /// <summary>
    /// Represents the top-level Heads-Up Display (HUD) for displaying statistical information.
    /// </summary>
    /// <remarks>
    /// This variable is typically used to manage and render the visual representation
    /// of critical game or application statistics on the screen, such as scores, health,
    /// performance metrics, or other relevant data points.
    /// </remarks>
    public GameObject topStatsHUD; // Reference to the new HUD top stats panel

    /// <summary>
    /// Represents the primary Heads-Up Display (HUD) interface within the application.
    /// </summary>
    /// <remarks>
    /// The mainHUD variable is responsible for managing and displaying critical visual
    /// information to the user, typically used in gaming or simulation contexts. It acts
    /// as the primary interface for providing feedback, notifications, and status updates
    /// during runtime.
    /// </remarks>
    public GameObject mainHUD; // Main HUD elements (time, day, hope)

    /// <summary>
    /// Represents the textual representation of time.
    /// </summary>
    [Header("HUD Elements")]
    public TextMeshProUGUI timeText;

    /// <summary>
    /// Represents the textual description or name of a specific day.
    /// </summary>
    public TextMeshProUGUI dayText;

    /// <summary>
    /// Represents a slider control that allows users to select a value or position
    /// within a defined range. The hopeSlider can be used in various UI components
    /// to provide interactive value input.
    /// </summary>
    public Slider hopeSlider;

    /// <summary>
    /// Represents a textual string containing a message of hope or positivity.
    /// </summary>
    public TextMeshProUGUI hopeText;

    /// <summary>
    /// Represents the visual representation of a character.
    /// </summary>
    /// <remarks>
    /// This variable is used to store data related to how a character appears,
    /// which could include image paths, sprites, or graphical elements.
    /// </remarks>
    [Header("Character Stats Display")]
    public Image characterPicture;

    /// <summary>
    /// Represents the text associated with the name of a character.
    /// This variable is typically used to store or display the name of a character in a game or application.
    /// </summary>
    public TextMeshProUGUI characterNameText;

    /// <summary>
    /// Represents a descriptive string providing detailed information
    /// about the attributes, traits, or background of a character.
    /// </summary>
    public TextMeshProUGUI characterDescriptionText;

    /// <summary>
    /// Represents the speed at which the slider interpolates between values.
    /// Controls how quickly the slider moves when transitioning from its current value
    /// to the target value, with higher values indicating faster transitions.
    /// </summary>
    [Header("Stat Sliders")]
    public float sliderLerpSpeed = 5f;

    /// <summary>
    /// Represents a UI component that visually displays the health of an entity as a slider.
    /// The slider's value typically corresponds to the entity's current health,
    /// providing a graphical representation of a health proportion in relation to
    /// its maximum value.
    /// </summary>
    public Slider healthSlider;

    /// <summary>
    /// Represents the textual display of the health percentage.
    /// This variable is typically used to show the player's current health
    /// as a percentage in the user interface.
    /// </summary>
    public TextMeshProUGUI healthPercentText;

    /// <summary>
    /// Represents a UI control element that allows the user to adjust the stability setting
    /// within a predefined range, typically as a slider component.
    /// </summary>
    /// <remarks>
    /// The stabilitySlider variable is commonly used in applications where user-controlled
    /// stability adjustments are required, such as image processing, simulations, or audio manipulation.
    /// </remarks>
    public Slider stabilitySlider;

    /// <summary>
    /// Represents a textual value that denotes the stability percentage.
    /// This variable is used to store or display the stability of a specific entity
    /// or process as a percentage in string format.
    /// </summary>
    public TextMeshProUGUI stabilityPercentText;

    /// <summary>
    /// Represents a UI slider control used for adjusting the learning rate in an application.
    /// </summary>
    /// <remarks>
    /// The learningSlider is typically used in scenarios involving machine learning
    /// or similar applications where a numerical value representing the learning rate
    /// needs to be fine-tuned interactively by the user. The slider allows the user
    /// to select a value within a predefined range.
    /// </remarks>
    public Slider learningSlider;

    /// <summary>
    /// Represents the text or string value that displays the percentage of learning progress.
    /// </summary>
    public TextMeshProUGUI learningPercentText;

    /// <summary>
    /// Represents a slider control that measures or adjusts the user's work readiness level.
    /// </summary>
    /// <remarks>
    /// This slider can be used in various contexts, such as user interfaces for
    /// performance assessments, readiness tracking, or progress updates.
    /// </remarks>
    public Slider workReadinessSlider;

    /// <summary>
    /// Represents the textual description or formatted representation
    /// of the work readiness percentage. This variable is typically
    /// used to convey the readiness status as a human-readable string.
    /// </summary>
    public TextMeshProUGUI workReadinessPercentText;
    public Slider trustSlider;

    /// <summary>
    /// Represents a text value displaying the level of trust as a percentage.
    /// This variable is typically used to convey trustworthiness in a readable format,
    /// often accompanied by a numerical percentage value.
    /// </summary>
    public TextMeshProUGUI trustPercentText;

    /// <summary>
    /// Represents a slider component used to adjust or display
    /// nutritional values such as calories, macronutrients, or other
    /// dietary metrics in a user interface.
    /// </summary>
    public Slider nutritionSlider;

    /// <summary>
    /// Represents the text displaying the percentage of nutritional value, typically used to indicate
    /// the proportion of a specific nutrient in relation to a recommended daily intake or total amount.
    /// </summary>
    public TextMeshProUGUI nutritionPercentText;

    /// <summary>
    /// Represents a slider control used to adjust or display the hygiene value in the user interface.
    /// </summary>
    public Slider hygieneSlider;

    /// <summary>
    /// Represents the text value or label associated with the hygiene percentage.
    /// This typically conveys information about the level or status
    /// of hygiene as a percentage in a readable text format.
    /// </summary>
    public TextMeshProUGUI hygienePercentText;

    /// <summary>
    /// Represents a slider control that allows the user to adjust energy levels.
    /// </summary>
    public Slider energySlider;

    /// <summary>
    /// Represents the text indicating the percentage of energy.
    /// </summary>
    /// <remarks>
    /// This variable is typically used to display or store the energy level
    /// in a human-readable percentage format.
    /// </remarks>
    public TextMeshProUGUI energyPercentText;

    /// <summary>
    /// Gets or sets a value indicating whether the operation or process is currently paused.
    /// </summary>
    /// <remarks>
    /// This property can be used to determine or control whether an operation is in a paused state.
    /// When set to true, the process is paused; when false, the process is active or running.
    /// </remarks>
    private bool IsPaused { get; set; }

    /// <summary>
    /// Represents the current character being processed or evaluated within the scope
    /// of the application logic. This variable is typically used to track the specific
    /// character in workflows such as parsing, iteration, or text analysis operations.
    /// </summary>
    private CharacterStats _currentCharacter;

    /// <summary>
    /// Stores a reference to the most recently focused target within the application.
    /// </summary>
    /// <remarks>
    /// This variable is typically used to track the UI element or application component
    /// that had the user's focus before it shifted to another target.
    /// </remarks>
    private Transform _lastFocussedTarget;

    /// <summary>
    /// Called when the script instance is being loaded. This method is invoked
    /// during the initialization phase of the object lifecycle, before the Start
    /// method is called.
    /// </summary>
    /// <remarks>
    /// Use this method to perform any setup or initialization required for the script
    /// before it becomes active. This includes allocating resources, subscribing to
    /// events, or setting initial state values.
    /// Avoid accessing other game objects or components in this method, as they may
    /// not yet be fully initialized.
    /// </remarks>
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

    /// <summary>
    /// Retrieves a customer's order history based on their customer ID.
    /// </summary>
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

    /// <summary>
    /// Called when the object is destroyed. This method is typically used to release
    /// resources, unsubscribe from events, or perform cleanup operations before the
    /// object is removed from memory or the scene.
    /// </summary>
    private void OnDestroy()
    {
        TimeManager.Instance.MinuteChanged -= UpdateTimeDisplay;
        
        CharacterStats.OnAnyStatChanged -= OnCharacterStatChanged;
    }

    private void Update()
    {
        // Toggle pause
        if (InputManager.Instance.PauseInput || InputManager.Instance.DeselectInput)
        {
            // Close inventory first if it's open
            if (inventoryUI.inventoryPanel.activeSelf)
            {
                inventoryUI.Toggle();
                return;
            }
            else if (CameraBehaviour.Instance.focussedTarget != null)
            {
                // Character selected - let the camera deselect it
                // (Camera already has deselected logic on Escape via DeselectInput)
                // a Stats panel will auto-hide via HandleStatsDisplay()

                CameraBehaviour.Instance.DeselectCharacter();
            }
            else
            {
                // Nothing selected - open a pause menu
                TogglePause();
            }
        }

        // Toggle inventory
        if (InputManager.Instance.InventoryInput)
            ToggleInventory();

        // Toggle character stats
        HandleStatsDisplay();

        //  If a stats panel is open, update with the currently selected character
        if (statsPanel.activeSelf)
        {
            // Get currently selected character from camera
            if (CameraBehaviour.Instance.focussedTarget != null)
            {
                CharacterStats selectedChar = CameraBehaviour.Instance.focussedTarget.GetComponent<CharacterStats>();

                // Update if character changed or stats changed
                if (selectedChar != null)
                {
                    _currentCharacter = selectedChar;
                    UpdateCharacterStatsDisplay(_currentCharacter);
                }
            }
            else if (_currentCharacter != null)
            {
                // Still update the current character even if not focused (for live stat changes)
                UpdateCharacterStatsDisplay(_currentCharacter);
            }
        }

        // Update hope bar continuously
        UpdateHopeDisplay();
    }

    /// <summary>
    /// Updates and displays the player's statistical information in the game's user interface.
    /// </summary>
    private void HandleStatsDisplay()
    {
        Transform focussedTarget = CameraBehaviour.Instance.focussedTarget;

        // Check if selection changed
        if (focussedTarget != _lastFocussedTarget)
        {
            _lastFocussedTarget = focussedTarget;

            if (focussedTarget != null)
            {
                // Character selected - show stats
                CharacterStats character = focussedTarget.GetComponent<CharacterStats>();
                if (character != null)
                {
                    _currentCharacter = character;
                    statsPanel.SetActive(true);
                    if (topStatsHUD != null) topStatsHUD.SetActive(true);
                    UpdateCharacterStatsDisplay(_currentCharacter);
                }
            }
            else
            {
                // No character selected - hide stats
                statsPanel.SetActive(false);
                if (topStatsHUD != null) topStatsHUD.SetActive(false);
                _currentCharacter = null;
            }
        }
        else if (focussedTarget != null && _currentCharacter != null && statsPanel.activeSelf)
        {
            // Character still selected - update stats (for live changes)
            UpdateCharacterStatsDisplay(_currentCharacter);
        }
    }


    /// <summary>
    /// Updates the time display on the user interface to reflect the current time.
    /// </summary>
    private void UpdateTimeDisplay(int hours, int minutes, int days)
    {
        timeText.text = $"{hours:00}:{minutes:00}";
        dayText.text = $"Day {days}";
    }

    /// <summary>
    /// Updates the display elements related to hope indicators in the application's user interface.
    /// </summary>
    private void UpdateHopeDisplay()
    {
        if (GameManager.Instance == null) return;

        float hopeValue = GameManager.Instance.hope;
        float normalizedHope = hopeValue / 100f;

        hopeSlider.value = Mathf.Lerp(hopeSlider.value, normalizedHope, Time.deltaTime * sliderLerpSpeed);
        hopeText.text = $"Hope: {hopeValue}%";
    }

    /// <summary>
    /// Updates the character's stats display on the user interface.
    /// This includes refreshing the displayed values for health, stamina,
    /// mana, and other relevant stats to match the current character data.
    /// </summary>
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

    /// <summary>
    /// Pauses the current operation or process, allowing it to resume later.
    /// </summary>
    private void SetPause(bool pause)
    {
        IsPaused = pause;
        pausePanel.SetActive(pause);
        Time.timeScale = pause ? 0f : 1f;
    }

    /// <summary>
    /// Toggles the paused state of the application or process.
    /// </summary>
    private void TogglePause()
    {
        SetPause(!IsPaused);
        ToggleHUD();
    }

    /// <summary>
    /// Toggles the inventory state between open and closed.
    /// </summary>
    private void ToggleInventory()
    {
        inventoryUI.Toggle();
    }

    /// <summary>
    /// Triggered when the inventory is opened by the user or system.
    /// This method allows the implementation of logic to handle actions or updates
    /// required when the inventory UI becomes visible or active.
    /// </summary>
    public void OnInventoryOpened()
    {
        // Hide other UI elements when inventory is opened
        ToggleHUD();
    }

    /// <summary>
    /// Handles operations to perform when the inventory is closed.
    /// </summary>
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

    /// <summary>
    /// Toggles the state of a statistics feature.
    /// </summary>
    public void ToggleStats()
    {
        bool isActive = !statsPanel.activeSelf;
        statsPanel.SetActive(isActive);

        if (isActive)
        {
            // Show stats of the currently selected character, or first character
            if (CameraBehaviour.Instance.focussedTarget != null)
            {
                CharacterStats selectedChar = CameraBehaviour.Instance.focussedTarget.GetComponent<CharacterStats>();
                if (selectedChar != null)
                {
                    _currentCharacter = selectedChar;
                    UpdateCharacterStatsDisplay(_currentCharacter);
                }
            }
        }
    }

    /// <summary>
    /// Toggles the state of the Heads-Up Display (HUD) in the application.
    /// This method switches the visibility or active state of the HUD,
    /// enabling it if it is currently disabled and disabling it if it is currently enabled.
    /// </summary>
    private void ToggleHUD()
    {
        bool isActive = !mainHUD.activeSelf;
        mainHUD.SetActive(isActive);
    }

    /// <summary>
    /// Triggered whenever a character's stat changes.
    /// </summary>
    private void OnCharacterStatChanged(CharacterStats character)
    {
        // Only update if it's the currently displayed character
        if (character == _currentCharacter && statsPanel.activeSelf)
        {
            UpdateCharacterStatsDisplay(character);
        }
    }

    /// <summary>
    /// Handles the click event for the Resume button.
    /// </summary>
    public void OnResumeButtonClicked()
    {
        SetPause(false);
       
    }

    /// <summary>
    /// Handles the event triggered when the "Quit" button is clicked.
    /// </summary>
    public void OnQuitButtonClicked() => Application.Quit();
}