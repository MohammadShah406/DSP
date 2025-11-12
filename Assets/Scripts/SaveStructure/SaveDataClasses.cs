using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int currentDay;
    public float timeOfDay;

    public List<CharacterSaveData> characters = new();
    public List<ResourceSaveData> resources = new();
}

[Serializable]
public class CharacterSaveData
{
    public string id;
    public Vector3 position;
}

[Serializable]
public class ResourceSaveData
{
    public string id;
    public int quantity;
}