using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Shared Stat")]
    [Range(0, 100)] public int hope = 50;

    [Header("Upgrade Progress")]
    public int upgradesDone = 0;
    public int totalUpgrades = 10;

    [SerializeField] private TimeManager timeManager;

    public List<GameObject> characters = new List<GameObject>();
    public List<ResourceData> resources = new List<ResourceData>();
    
    [Header("Item Database")]
    public List<ItemData> itemDatabase = new List<ItemData>();

    public ItemData GetItemData(string name)
    {
        return itemDatabase.Find(i => i.itemName == name);
    }

    public event Action OnResourcesChanged;

    public void AddResource(string name, int amount)
    {
        ResourceData res = resources.Find(r => r.resourceName == name);
        if (res != null)
        {
            res.quantity += amount;
        }
        else
        {
            resources.Add(new ResourceData { resourceName = name, quantity = amount });
        }
        OnResourcesChanged?.Invoke();
    }
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




        TryLoad();
    }

    private void Start()
    {
        if (timeManager == null)
        {
            Debug.LogWarning("GameManager: No TimeManager assigned, searching for an instance in the scene.");
            timeManager = TimeManager.Instance;
        }
    }

    private void Update()
    {
        CalculateHope();

        if (Input.GetKeyDown(KeyCode.F2))
        {
            TrySave();
        }

    }

    private void CalculateHope()
    {
        // Formula:
        // Upgrade Value (U) = (Upgrades Done / Total Available Upgrades)
        // Attribute Value (A) = Avg of each resident's attributes
        // Hope = (U * 60) + (A * 4)

        float u = 0;
        if (totalUpgrades > 0)
        {
            u = (float)upgradesDone / totalUpgrades;
        }

        float totalAttributeSum = 0;
        int attributeCount = 0;
        List<CharacterStats> charStatsList = GetCharacterComponents();

        if (charStatsList.Count > 0)
        {
            foreach (var stats in charStatsList)
            {
                // Averaging all major stats: Health, Stability, Learning, WorkReadiness, Trust, Nutrition, Hygiene, Energy
                // Since they are 0-100, we'll sum them and then normalize to 0-10 as per (A * 4) logic
                totalAttributeSum += (stats.Health + stats.Stability + stats.Learning + stats.WorkReadiness + 
                                     stats.Trust + stats.Nutrition + stats.Hygiene + stats.Energy) / 8f;
                attributeCount++;
            }

            float averageAttributeScore = totalAttributeSum / attributeCount; // 0-100
            float a = averageAttributeScore / 10f; // 0-10

            hope = Mathf.RoundToInt((u * 60f) + (a * 4f));
        }
        else
        {
            // If no characters, hope only depends on upgrades or stays at a base
            hope = Mathf.RoundToInt(u * 60f);
        }
        
        hope = Mathf.Clamp(hope, 0, 100);
    }

    public void TrySave()
    {
        GameSaveData data = new GameSaveData();
        data.currentDay = timeManager.days;
        data.timeOfDay = timeManager.TimeOfDay;
        data.hope = hope;
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

                health = stats.health,
                stability = stats.stability,
                learning = stats.learning,
                workReadiness = stats.workReadiness,
                trust = stats.trust,
                nutrition = stats.nutrition,
                hygiene = stats.hygiene,
                energy = stats.energy,
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
        hope = data.hope;
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
                        character.health = charData.health;
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
                if (res != null)
                {
                    res.quantity = resData.quantity;
                }
                else
                {
                    resources.Add(new ResourceData { resourceName = resData.id, quantity = resData.quantity });
                }
            }
            PendingGameLoad.resources = null;
            OnResourcesChanged?.Invoke();
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
