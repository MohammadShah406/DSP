using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject statsPanel;
    public InventoryUI inventoryUI;
    public GameObject taskPanel;
    public GameObject topStatsHUD;
    public GameObject mainHUD;
    public CharacterCarousel characterCarousel;
    public GameObject dayEndUI;
    public GameObject gameEndUI;
    public GameObject settingsUI;
    public CraftingUI craftingUI;
    public CraftingUI cookingUI;
    

    [Header("HUD Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;
    public Slider hopeSlider;
    public TextMeshProUGUI hopeText;
    
    [Header("Character Stats Display")]
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
        Crafting,
        Cooking,
        CharacterStats,
        Settings
    }

    public UIState CurrentState => _currentState;
    private UIState _currentState = UIState.Gameplay;
    private const float MaxStatValue = 100f;
    
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
        
        inventoryUI.gameObject.SetActive(true);
        pausePanel.SetActive(false);
        statsPanel.SetActive(false);
        taskPanel.SetActive(true);
        topStatsHUD.SetActive(false);
        mainHUD.SetActive(true);
    }
    
    private void Start()
    {
        TimeManager.Instance.MinuteChanged += UpdateTimeDisplay;
        UpdateTimeDisplay(TimeManager.Instance.hours, TimeManager.Instance.minutes, TimeManager.Instance.days);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHopeChanged += OnHopeChanged;
            _targetHope = GameManager.Instance.Hope / MaxStatValue;
            hopeSlider.value = _targetHope;
            hopeText.text = $"Hope: {GameManager.Instance.Hope}%";
        }

        healthSlider.maxValue = MaxStatValue;
        stabilitySlider.maxValue = MaxStatValue;
        learningSlider.maxValue = MaxStatValue;
        workReadinessSlider.maxValue = MaxStatValue;
        trustSlider.maxValue = MaxStatValue;
        nutritionSlider.maxValue = MaxStatValue;
        hygieneSlider.maxValue = MaxStatValue;
        energySlider.maxValue = MaxStatValue;

        CharacterStats.OnAnyStatChanged += OnCharacterStatChanged;

        SwitchState(UIState.Gameplay);
    }
    
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
        if (InputManager.Instance.PauseInput || InputManager.Instance.DeselectInput)
        {
            HandleBackInput();
        }

        if (InputManager.Instance.InventoryInput)
        {
            ToggleInventory();
        }

        UpdateSelectionState();

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
        else if (settingsUI.activeSelf)
        {
            settingsUI.SetActive(false);
            SwitchState(UIState.Pause);
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

                    UpdateCharacterStatsDisplay(_currentCharacter, false);
                    if (characterCarousel != null)
                    {
                        characterCarousel.SetCurrentCharacter(_currentCharacter);
                    }

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

        pausePanel.SetActive(_currentState == UIState.Pause);
        statsPanel.SetActive(_currentState == UIState.CharacterStats);
        topStatsHUD.SetActive(_currentState == UIState.CharacterStats);
        settingsUI.SetActive(_currentState == UIState.Settings);
        
        if (craftingUI != null)
        {
            bool isCrafting = _currentState == UIState.Crafting;
            craftingUI.gameObject.SetActive(isCrafting);
            if (isCrafting) craftingUI.Setup(RecipeType.Crafting);
        }
        if (cookingUI != null)
        {
            bool isCooking = _currentState == UIState.Cooking;
            cookingUI.gameObject.SetActive(isCooking);
            if (isCooking) cookingUI.Setup(RecipeType.Cooking);
        }

        if (inventoryUI != null && inventoryUI.inventoryPanel != null)
        {
            bool isInventory = _currentState == UIState.Inventory;
            
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
        
        bool hideMainHUD = (_currentState == UIState.Pause || _currentState == UIState.Settings);
        if (mainHUD != null)
        {
            mainHUD.SetActive(!hideMainHUD);
        }
        Time.timeScale = (_currentState == UIState.Pause || _currentState == UIState.Settings) ? 0f : 1f;
        IsPaused = (_currentState == UIState.Pause);
    }
    
    private void LerpSliders()
    {
        float t = Time.deltaTime * sliderLerpSpeed;
        bool anyStatChanged = false;

        if (!Mathf.Approximately(hopeSlider.value, _targetHope))
        {
            hopeSlider.value = Mathf.Lerp(hopeSlider.value, _targetHope, t);
        }
        
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
    
    public void UpdateCharacterStatsDisplay(CharacterStats character, bool immediate = false)
    {
        if (character == null)
        {
            ClearCharacterStatsDisplay();
            return;
        }
        _currentCharacter = character;

        if (_currentState == UIState.Gameplay || _currentState == UIState.CharacterStats)
        {
            if (!statsPanel.activeSelf) SwitchState(UIState.CharacterStats);
        }
        
        characterNameText.text = character.characterName;

        characterDescriptionText.text = character.description ?? "Refugee";

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
    
    public void ToggleInventory()
    {
        if (_currentState == UIState.Inventory)
            OnInventoryClosed();
        else
            OnInventoryOpened();
    }
    
    
    
    private void ClearCharacterStatsDisplay()
    {
        statsPanel.SetActive(false);
        topStatsHUD.SetActive(false);

        _currentCharacter = null;

        _targetHealth = 0;
        _targetStability = 0;
        _targetLearning = 0;
        _targetWorkReadiness = 0;
        _targetTrust = 0;
        _targetNutrition = 0;
        _targetHygiene = 0;
        _targetEnergy = 0;

        healthSlider.value = 0;
        stabilitySlider.value = 0;
        learningSlider.value = 0;
        workReadinessSlider.value = 0;
        trustSlider.value = 0;
        nutritionSlider.value = 0;
        hygieneSlider.value = 0;
        energySlider.value = 0;
        UpdateStatTexts();
    }


    
    public void ToggleStats()
    {
        if (_currentState == UIState.CharacterStats)
            SwitchState(UIState.Gameplay);
        else if (CameraBehaviour.Instance.focussedTarget != null)
            SwitchState(UIState.CharacterStats);
    }

    
    public void OnResumeButtonClicked()
    {
        SwitchState(UIState.Gameplay);
    }

    public void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }

    public void ShowDayEndUI()
    {
        Debug.Log("Day End UI is now visible.");
        dayEndUI.SetActive(true);

        pausePanel.SetActive(false);
        statsPanel.SetActive(false);
        topStatsHUD.SetActive(false);

        if (inventoryUI != null && inventoryUI.inventoryPanel != null)
        {
            inventoryUI.inventoryPanel.SetActive(false);
        }

        mainHUD.SetActive(false);
        taskPanel.SetActive(false);
    }

    public void ShowGameEndUI()
    {
        gameEndUI.SetActive(true);

        pausePanel.SetActive(false);
        statsPanel.SetActive(false);
        topStatsHUD.SetActive(false);

        if (inventoryUI != null && inventoryUI.inventoryPanel != null)
        {
            inventoryUI.inventoryPanel.SetActive(false);
        }

        mainHUD.SetActive(false);
        taskPanel.SetActive(false);
    }

    public void RestartGame()
    {
      SceneManager.LoadScene("GameScene");
    }

    public void HideDayEndUI()
    {
        dayEndUI.SetActive(false);

        mainHUD.SetActive(true);
        taskPanel.SetActive(true);
    }

    public void HideGameEndUI()
    {
        gameEndUI.SetActive(false);
        
        mainHUD.SetActive(true);
        taskPanel.SetActive(true);
    }
    public void OnInventoryOpened() => SwitchState(UIState.Inventory); 
        
    public void OnInventoryClosed()
    {
        if (CameraBehaviour.Instance.focussedTarget != null)
            SwitchState(UIState.CharacterStats);
        else
            SwitchState(UIState.Gameplay);
    }
    public void SettingState() => SwitchState(UIState.Settings);

    public void CraftingState() => SwitchState(UIState.Crafting);
    
    public void CookingState() => SwitchState(UIState.Cooking);

    public void ApplyForSettings() => SwitchState(UIState.Pause);
    
}

