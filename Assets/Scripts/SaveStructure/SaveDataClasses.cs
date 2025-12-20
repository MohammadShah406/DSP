using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int currentDay;
    public float timeOfDay;
    public int hope;
    public int upgradesDone;
    public int totalUpgrades;

    public List<CharacterSaveData> characters = new();
    public List<ResourceSaveData> resources = new();
}

[Serializable]
public class CharacterSaveData
{
    public string name;
    public string iconName;
    public float health;
    public int stability;
    public int learning;
    public int workReadiness;
    public int trust;
    public int nutrition;
    public int hygiene;
    public int energy;
    public string primaryAttribute;
    public float growthRate;
    public Vector3 position;
}

[Serializable]
public class ResourceSaveData
{
    public string id; // This is ItemData.itemName
    public int quantity;
}