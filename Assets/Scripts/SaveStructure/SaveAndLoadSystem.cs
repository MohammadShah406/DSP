using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveAndLoadSystem
{
    private static string saveFolder = Application.persistentDataPath + "/Saves";
    private static string saveFile = "save0.json";
    private static string FullPath => Path.Combine(saveFolder, saveFile);

    public static void Save(GameSaveData data)
    {
        // Ensure folder exists
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FullPath, json);

        Debug.Log($"Game saved at: {FullPath}");
    }

    public static GameSaveData Load()
    {
        if (!File.Exists(FullPath))
        {
            Debug.LogWarning($"No save file found at: {FullPath}");
            return null;
        }

        string json = File.ReadAllText(FullPath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        Debug.Log($"Game loaded from: {FullPath}");
        return data;
    }

    public static void DeleteSave()
    {
        if (File.Exists(FullPath))
        {
            File.Delete(FullPath);
            Debug.Log($"Deleted save at: {FullPath}");
        }
    }
}

