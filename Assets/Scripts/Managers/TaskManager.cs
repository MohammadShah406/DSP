using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    [SerializeField] private List<TaskData> allTasks;
    [SerializeField] private List<TaskData> currentDayTasks = new List<TaskData>();

    public event Action OnTasksUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.DayChanged += UpdateDayTasks;
            UpdateDayTasks(TimeManager.Instance.days);
        }
    }

    public void SetAllTasks(List<TaskData> tasks)
    {
        allTasks = tasks;
    }

    public void UpdateDayTasks(int day)
    {
        if (allTasks == null)
        {
            Debug.LogWarning("[TaskManager] allTasks list is null!");
            return;
        }

        currentDayTasks = allTasks.Where(t => t != null && t.day == day).ToList();
        foreach (var task in currentDayTasks)
        {
            if (task != null) task.isCompleted = false; // Reset for the day if needed, though SOs persist
        }
        OnTasksUpdated?.Invoke();
    }

    public List<TaskData> GetTasksForCurrentTime(int day, int hour, int minute)
    {
        if (allTasks == null) return new List<TaskData>();
        // Filter by the provided day and ensure it's at or before the current hour
        return allTasks.Where(t => t != null && t.day == day && !t.isCompleted && (t.hour <= hour)).ToList();
    }

    public void CompleteTask(string taskDescription)
    {
        TaskData task = currentDayTasks.Find(t => t.taskDescription.Equals(taskDescription, StringComparison.OrdinalIgnoreCase) && !t.isCompleted);
        if (task != null)
        {
            task.isCompleted = true;
            ApplyStatEffects(task);
            OnTasksUpdated?.Invoke();
            Debug.Log($"Task Completed: {taskDescription}");
        }
    }

    public void CompleteTaskByRequirement(string requirement)
    {
        if (string.IsNullOrEmpty(requirement)) return;

        TaskData task = currentDayTasks.Find(t => t.actionRequirement != null && t.actionRequirement.Equals(requirement, StringComparison.OrdinalIgnoreCase) && !t.isCompleted);
        if (task != null)
        {
            task.isCompleted = true;
            ApplyStatEffects(task);
            OnTasksUpdated?.Invoke();
            Debug.Log($"Task Completed by Requirement: {requirement}");
        }
    }

    private void ApplyStatEffects(TaskData task)
    {
        if (GameManager.Instance == null) return;

        var characters = GameManager.Instance.GetCharacterComponents();

        foreach (var effect in task.statEffects)
        {
            CharacterStats target = characters.Find(c => c.characterName.Equals(effect.characterName, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                ApplyEffect(target, effect.attribute, effect.amount);
            }
        }
    }

    private void ApplyEffect(CharacterStats character, CharacterStats.PrimaryAttribute attribute, int amount)
    {
        switch (attribute)
        {
            case CharacterStats.PrimaryAttribute.Stability:
                character.ChangeStability(amount);
                break;
            case CharacterStats.PrimaryAttribute.Learning:
                character.ChangeLearning(amount);
                break;
            case CharacterStats.PrimaryAttribute.WorkReadiness:
                character.ChangeWorkReadiness(amount);
                break;
            case CharacterStats.PrimaryAttribute.Trust:
                character.ChangeTrust(amount);
                break;
            case CharacterStats.PrimaryAttribute.Nutrition:
                character.ChangeNutrition(amount);
                break;
            case CharacterStats.PrimaryAttribute.Hygiene:
                character.ChangeHygiene(amount);
                break;
            case CharacterStats.PrimaryAttribute.Energy:
                character.ChangeEnergy(amount);
                break;
        }
    }

    public List<TaskData> GetActiveTasks()
    {
        if (TimeManager.Instance == null) return new List<TaskData>();
        return GetTasksForCurrentTime(TimeManager.Instance.days, TimeManager.Instance.hours, TimeManager.Instance.minutes);
    }
}
