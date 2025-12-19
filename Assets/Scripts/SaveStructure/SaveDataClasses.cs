using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int currentDay;
    public float timeOfDay;
    public int hope;

    public List<CharacterSaveData> characters = new();
    public List<ResourceSaveData> resources = new();
}

[Serializable]
public class CharacterSaveData
{
    public string name;
    public int health;
    public int stability;
    public int learning;
    public int workReadiness;
    public int trust;
    public int nutrition;
    public int hygiene;
    public int energy;
    public Vector3 position;
}

[Serializable]
public class ResourceSaveData
{
    public string id;
    public int quantity;
}