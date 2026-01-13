using UnityEngine;
using System.Collections.Generic;

using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Task", menuName = "Tasks/TaskData")]
public class TaskData : ScriptableObject
{
    public enum TaskType
    {
        Interaction,
        ObjectActivation
    }
    public TaskType taskType;

    public enum CharacterName
    {
        None,
        Sahil,
        Bashir,
        Aisha,
        Sagar
    }
    public CharacterName requiredCharacter;

    [FormerlySerializedAs("actionRequirement")]
    [FormerlySerializedAs("requiredObjectName")]
    public string requirementTarget;

    public string taskDescription;
    public int day;
    public int hour;
    public int minute;
    public List<StatEffect> statEffects;
    

    
    [System.Serializable]
    public struct StatEffect
    {
        public CharacterName characterName;
        public CharacterStats.PrimaryAttribute attribute;
        public int amount;
    }
}

