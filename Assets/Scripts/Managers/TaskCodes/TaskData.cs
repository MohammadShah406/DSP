using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Task", menuName = "Tasks/TaskData")]
public class TaskData : ScriptableObject
{
    public string taskDescription;
    public int day;
    public int hour;
    public int minute;
    public List<StatEffect> statEffects;
    public string actionRequirement;
    
    
    [System.Serializable]
    public struct StatEffect
    {
        public string characterName;
        public CharacterStats.PrimaryAttribute attribute;
        public int amount;
    }
}

