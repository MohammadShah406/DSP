using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "New Task", menuName = "Tasks/TaskData")]
public class TaskData : ScriptableObject
{
    [Header("Time Settings")]
    public bool timedTask;
    public TimeCondition timeConditionType;
    public int completeByDay;
    public int completeByHour;
    public int completeByMinute;
    public int completeIn;
    
    [Header("Time")]
    public int day;
    public int hour;
    public int minute;
    
    [Header("Task Settings")]
    public TaskType taskType;
    public string requirementTarget;
    public string requirementProduct;
    
    [Header("Requirements")]
    public CharacterName requiredCharacter;
    
    [Header("Task Description")]
    public string taskDescription;
    
    [Header("Unlocks for Completion")]
    public List<Recipes> unlockRecipes;
    
    [Header("Rewards")]
    public List<StatEffect> statEffects;
    
    
    [System.Serializable]
    public struct StatEffect
    {
        public CharacterName characterName;
        public CharacterStats.PrimaryAttribute attribute;
        public int amount;
    }
    
    public enum TimeCondition
    {
        FromActivation,
        ByDayHourMinute
    }
    
    public enum CharacterName
    { 
        None, 
        Sahil, 
        Bashir, 
        Aisha, 
        Sagar, 
        All
    }
    
    public enum TaskType
    {
        Interaction,
        ObjectActivation,
        Crafting,
        Cooking,
        Scavenging
    }
}

