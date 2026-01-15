using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine.TextCore.Text;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Singleton Instance
    public static GameManager Instance { get; private set; }

    // Reference to TimeManager
    [SerializeField] private TimeManager timeManager;

    // Characters and Resources
    public List<GameObject> characters = new List<GameObject>();
    public List<ResourceData> resources = new List<ResourceData>();
    List<CharacterStats> charStatsList = new List<CharacterStats>();

    // Item Database
    [Header("Item Database")]
    public List<ItemData> itemDatabase = new List<ItemData>();

    // Shared Game State
    [Header("Shared Stat")]
    [Range(0, 100)] [SerializeField] private int hope = 50;
    public int Hope
    {
        get => hope;
        private set
        {
            if (hope != value)
            {
                hope = Mathf.Clamp(value, 0, 100);
                OnHopeChanged?.Invoke(hope);
            }
        }
    }

    public event Action<int> OnHopeChanged;

    // Upgrade Progress
    [Header("Upgrade Progress")]
    public int upgradesDone = 0;
    public int totalUpgrades = 0;

    private float upgradeValue = 0;
    private float totalAttributeSum = 0;
    private int characterCount = 0;
    // Average attribute score across all characters
    private float averageAttributeScore = 0; // 0-100
    private float a = 0; // 0-10

    // Event for resource changes
    public event Action OnResourcesChanged;
    public event Action<string, int> OnResourceChanged; // name, newQuantity

    // Whether to load saved game on start
    public bool loadsavedgame = true;

    public bool cheatHopeEnabled = false;

    /// <summary>
    /// Retrieves item data by name from the item database.
    /// </summary>
    public ItemData GetItemData(string name)
    {
        return itemDatabase.Find(i => i.itemName == name);
    }

    /// <summary>
    /// Adds a resource to the resource list or updates its quantity.
    /// </summary>
    public void AddResource(string name, int amount)
    {
        ResourceData res = resources.Find(r => r.resourceName == name);
        int newQuantity = 0;
        if (res != null)
        {
            res.quantity += amount;
            newQuantity = res.quantity;
        }
        else
        {
            newQuantity = amount;
            resources.Add(new ResourceData { resourceName = name, quantity = amount });
        }
        OnResourcesChanged?.Invoke();
        OnResourceChanged?.Invoke(name, newQuantity);
    }

    /// <summary>
    /// Retrieves a list of CharacterStats components from the character GameObjects.
    /// </summary>
    public List<CharacterStats> GetCharacterComponents()
    {
        List<CharacterStats> characterList = new List<CharacterStats>();
        foreach (GameObject obj in characters)
        {
            if (obj != null)
            {
                CharacterStats c = obj.GetComponent<CharacterStats>();
                if (c != null)
                    characterList.Add(c);
            }
        }
        return characterList;
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GameManager instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Attempt to load any pending game state
        if (loadsavedgame)
            TryLoad();
    }

    private void Start()
    {
        if (timeManager == null)
        {
            Debug.LogWarning("GameManager: No TimeManager assigned, searching for an instance in the scene.");
            timeManager = TimeManager.Instance;
        }

        CameraBehaviour.Instance.focussedTarget = characters[0].gameObject.transform;
        charStatsList = GetCharacterComponents();
        SetUpgrades();
    }

    private void Update()
    {
        // Calculate hope based on current game state
        CalculateHope();

        if (Input.GetKeyDown(KeyCode.F2))
        {
            //  Save the game
            TrySave();
        }

    }

    /// <summary>
    /// Sets up the upgrade system by counting available upgrades.
    /// </summary>
    public void SetUpgrades()
    {
        if(DonationManager.Instance != null)
        {
            totalUpgrades = DonationManager.Instance.allDonationsItemData.Count;
        }
    }

    /// <summary>
    /// Calculates the hope value based on upgrades and character attributes.
    /// </summary>
    private void CalculateHope()
    {
        if(cheatHopeEnabled)
        {
            return;
        }

        // Formula:
        // Upgrade Value (U) = (Upgrades Done / Total Available Upgrades)
        // Attribute Value (A) = Avg of each resident's attributes
        // Hope = (U * 60) + (A * 4)

        upgradeValue = 0;
        totalAttributeSum = 0;
        characterCount = 0;

        if (totalUpgrades > 0)
        {
            upgradeValue = (float)upgradesDone / totalUpgrades;
        }
        else
        {
            upgradeValue = 1;
        }

        // Calculate average attribute score across all characters
        if (charStatsList.Count > 0)
        {
            foreach (var stats in charStatsList)
            {
                // Averaging all major stats: Health, Stability, Learning, WorkReadiness, Trust, Nutrition, Hygiene, Energy
                // Since they are 0-100, we'll sum them and then normalize to 0-10 as per (A * 4) logic
                totalAttributeSum += (stats.Health + stats.Stability + stats.Learning + stats.WorkReadiness +
                                     stats.Trust + stats.Nutrition + stats.Hygiene + stats.Energy) / 8f;
                characterCount++;
            }

            // Average attribute score across all characters
            averageAttributeScore = totalAttributeSum / characterCount; // 0-100
            a = averageAttributeScore / 10f; // 0-10

            Hope = Mathf.RoundToInt((upgradeValue * 60f) + (a * 4f));
        }
        else
        {
            // If no characters, hope only depends on upgrades or stays at a base
            Hope = Mathf.RoundToInt(upgradeValue * 60f);
        }
    }

    public void TrySave()
    {
        GameSaveData data = new GameSaveData();
        data.currentDay = timeManager.days;
        data.timeOfDay = timeManager.TimeOfDay;
        data.hope = Hope;
        data.upgradesDone = upgradesDone;
        data.totalUpgrades = totalUpgrades;

        foreach (GameObject obj in characters)
        {
            if (obj == null) continue;
            CharacterStats stats = obj.GetComponent<CharacterStats>();
            data.characters.Add(new CharacterSaveData
            {
                name = obj.name,
                iconName = stats.characterIcon != null ? stats.characterIcon.name : "",
                position = obj.transform.position,

                health = stats.Health,
                stability = stats.Stability,
                learning = stats.Learning,
                workReadiness = stats.WorkReadiness,
                trust = stats.Trust,
                nutrition = stats.Nutrition,
                hygiene = stats.Hygiene,
                energy = stats.Energy,
                primaryAttribute = stats.primaryAttribute.ToString(),
                growthRate = stats.growthRate,
            });
        }

        foreach (ResourceData res in resources)
        {
            if (res == null) continue;

            data.resources.Add(new ResourceSaveData
            {
                id = res.resourceName,
                quantity = res.quantity,
            });
        }

        SaveAndLoadSystem.Save(data);
    }

    public void TryLoad()
    {
        GameSaveData data = SaveAndLoadSystem.Load();
        if (data == null)
            return;

        // Store everything in the pending buffer
        PendingGameLoad.day = data.currentDay;
        PendingGameLoad.timeOfDay = data.timeOfDay;
        PendingGameLoad.characters = data.characters;
        PendingGameLoad.resources = data.resources;
        Hope = data.hope;
        upgradesDone = data.upgradesDone;
        totalUpgrades = data.totalUpgrades;

        // Try immediate apply for anything that exists
        TryApplyPending();
    }

    public void DeleteSave()
    {
        SaveAndLoadSystem.DeleteSave();
    }

    private void TryApplyPending()
    {
        // Apply time if TimeManager exists
        if (PendingGameLoad.day.HasValue && PendingGameLoad.timeOfDay.HasValue && TimeManager.Instance != null)
        {
            TimeManager.Instance.SetTime(PendingGameLoad.day.Value, PendingGameLoad.timeOfDay.Value);
            PendingGameLoad.day = null;
            PendingGameLoad.timeOfDay = null;
        }

        // Apply character positions if they exist
        if (PendingGameLoad.characters != null && characters != null)
        {
            foreach (var charData in PendingGameLoad.characters)
            {
                GameObject obj = characters.Find(c => c.name == charData.name);
                if (obj != null)
                {
                    obj.transform.position = charData.position;

                    // Get Character component
                    CharacterStats character = obj.GetComponent<CharacterStats>();
                    if (character != null)
                    {
                        character.name = charData.name;
                        // For characterIcon, we usually rely on the prefab/inspector setting 
                        // as loading sprites by name from Resources at runtime is specific to project structure.
                        // We save it for metadata purposes, but won't force a load if not using Resources folder.
                        character.transform.position = charData.position;
                        character.Health = Mathf.RoundToInt(charData.health);
                        character.Stability = charData.stability;
                        character.Learning = charData.learning;
                        character.WorkReadiness = charData.workReadiness;
                        character.Trust = charData.trust;
                        character.Nutrition = charData.nutrition;
                        character.Hygiene = charData.hygiene;
                        character.Energy = charData.energy;

                        if (Enum.TryParse(charData.primaryAttribute, out CharacterStats.PrimaryAttribute attr))
                        {
                            character.primaryAttribute = attr;
                        }
                        character.growthRate = charData.growthRate;
                    }
                    

                }
            }
            PendingGameLoad.characters = null;
        }

        // Apply resources if they exist
        if (PendingGameLoad.resources != null && resources != null)
        {
            foreach (var resData in PendingGameLoad.resources)
            {
                ResourceData res = resources.Find(r => r.resourceName == resData.id);
                int newQuantity = resData.quantity;
                if (res != null)
                {
                    res.quantity = resData.quantity;
                }
                else
                {
                    resources.Add(new ResourceData { resourceName = resData.id, quantity = resData.quantity });
                }
                OnResourceChanged?.Invoke(resData.id, newQuantity);
            }
            PendingGameLoad.resources = null;
            OnResourcesChanged?.Invoke();
        }
    }

    /// <summary>
    /// Updates the list of character stats.
    /// </summary>
    public void UpdateCharacterStatsList()
    {
        charStatsList = GetCharacterComponents();
    }

    public void EnableCharacter()
    {
        foreach (GameObject character in characters)
        {
            if (character != null)
            {
                if (!character.activeSelf)
                {
                    character.SetActive(true);
                    return;
                }
            }
        }
    }

    public void ToggleCheatHope()
    {
        cheatHopeEnabled = !cheatHopeEnabled;
        if (cheatHopeEnabled)
        {
            StartCoroutine(IncreaseHope());
        }
    }

    private IEnumerator IncreaseHope()
    {
        while (cheatHopeEnabled)
        {
            Hope += 1;
            
            yield return new WaitForSeconds(0.1f);
        }
    }
}

public static class PendingGameLoad
{
    public static int? day;
    public static float? timeOfDay;
    public static List<CharacterSaveData> characters;
    public static List<ResourceSaveData> resources;
}
