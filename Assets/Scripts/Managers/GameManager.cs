using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TimeManager timeManager;

    public List<GameObject> characters = new List<GameObject>();
    public List<ResourceData> resources = new List<ResourceData>();


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
        if(Input.GetKeyDown(KeyCode.F2))
        {
            TrySave();
        }

    }

    public void TrySave()
    {
        GameSaveData data = new GameSaveData();
        data.currentDay = timeManager.days;
        data.timeOfDay = timeManager.TimeOfDay;

        foreach (GameObject obj in characters)
        {
            if (obj == null) continue;

            data.characters.Add(new CharacterSaveData
            {
                id = obj.name,
                position = obj.transform.position,
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
                GameObject obj = characters.Find(c => c.name == charData.id);
                if (obj != null)
                {
                    obj.transform.position = charData.position;
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
