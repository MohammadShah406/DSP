using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Task", menuName = "Tasks/TaskData")]
public class TaskData : ScriptableObject
{
    public string taskDescription;
    public int day;
    public int hour;
    public int minute;

    [System.Serializable]
    public struct StatEffect
    {
        public string characterName;
        public CharacterStats.PrimaryAttribute attribute;
        public int amount;
    }

    public List<StatEffect> statEffects;
    public bool isCompleted;

    // Optional: Reference to a specific interactable or action type if we want auto-completion
    public string actionRequirement; 
}
