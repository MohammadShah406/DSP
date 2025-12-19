using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Shared Stat")]
    [Range(0, 100)] public int hope = 50;

    [SerializeField] private TimeManager timeManager;

    public List<GameObject> characters = new List<GameObject>();
    public List<ResourceData> resources = new List<ResourceData>();
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
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TrySave();
        }

    }

    public void TrySave()
    {
        GameSaveData data = new GameSaveData();
        data.currentDay = timeManager.days;
        data.timeOfDay = timeManager.TimeOfDay;
        data.hope = hope;

        foreach (GameObject obj in characters)
        {
            if (obj == null) continue;
            CharacterStats stats = obj.GetComponent<CharacterStats>();
            data.characters.Add(new CharacterSaveData
            {
                name = obj.name,
                position = obj.transform.position,

                health = stats.health,
                stability = stats.stability,
                learning = stats.learning,
                workReadiness = stats.workReadiness,
                trust = stats.trust,
                nutrition = stats.nutrition,
                hygiene = stats.hygiene,
                energy = stats.energy,
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
                        character.transform.position = charData.position;
                        character.Health = charData.health;
                        character.Stability = charData.stability;
                        character.Learning = charData.learning;
                        character.WorkReadiness = charData.workReadiness;
                        character.Trust = charData.trust;
                        character.Nutrition = charData.nutrition;
                        character.Hygiene = charData.hygiene;
                        character.Energy = charData.energy;
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
            }
            PendingGameLoad.resources = null;
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
