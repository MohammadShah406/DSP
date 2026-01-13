using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TaskInstance
{
    public TaskData taskData;
    public bool isActive;
    public bool isCompleted;
    public float startTime;
    public Interactable assignedInteractable;
    
    public TaskInstance(TaskData data)
    {
        taskData = data;
        isActive = false;
        isCompleted = false;
    }
    
    public void Activate()
    {
        isActive = true;
        startTime = Time.time;
    }
    
    public void Complete()
    {
        isCompleted = true;
        isActive = false;
    }
}
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    
    [SerializeField] private List<TaskData> allTaskData; 
    private List<TaskInstance> currentDayTaskInstances = new List<TaskInstance>(); 
    
    
    
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
            TimeManager.Instance.MinuteChanged += HandleMinuteChanged;
            Debug.Log($"[TaskManager] Initializing tasks for day {TimeManager.Instance.days}");
            UpdateDayTasks(TimeManager.Instance.days);
        }
        else
        {
            Debug.LogError("[TaskManager] TimeManager.Instance is null in Start!");
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.DayChanged -= UpdateDayTasks;
            TimeManager.Instance.MinuteChanged -= HandleMinuteChanged;
        }
    }

    private void HandleMinuteChanged(int h, int m, int d)
    {
        CheckForTaskActivation(d, h, m);
        OnTasksUpdated?.Invoke();
    }

    public void SetAllTasks(List<TaskData> tasks)
    {
        allTaskData = tasks;
    }

    public void UpdateDayTasks(int day)
    {
        if (allTaskData == null)
        {
            Debug.LogWarning("[TaskManager] allTaskData list is null!");
            return;
        }

        Debug.Log($"[TaskManager] Updating tasks for Day {day}. Total tasks in database: {allTaskData.Count}");

        // Create new runtime instances for the day
        currentDayTaskInstances.Clear();
        
        var dayTasks = allTaskData.Where(t => t != null && t.day == day).ToList();
        Debug.Log($"[TaskManager] Found {dayTasks.Count} tasks for Day {day}");
        
        foreach (var taskData in dayTasks)
        {
            TaskInstance instance = new TaskInstance(taskData);
            currentDayTaskInstances.Add(instance);
        }
        
        OnTasksUpdated?.Invoke();
    }

    private void CheckForTaskActivation(int day, int hour, int minute)
    {
        int currentTotalMinutes = hour * 60 + minute;

        foreach (var taskInstance in currentDayTaskInstances)
        {
            // Only activate if not already active and not completed
            if (!taskInstance.isActive && !taskInstance.isCompleted)
            {
                int taskTotalMinutes = taskInstance.taskData.hour * 60 + taskInstance.taskData.minute;

                // Activate if time has reached or passed
                if (taskInstance.taskData.day == day && currentTotalMinutes >= taskTotalMinutes)
                {
                    taskInstance.Activate();
                    Debug.Log($"[TaskManager] Activated task: {taskInstance.taskData.taskDescription}");
                }
            }
        }
    }
    
    public List<TaskInstance> GetTasksForCurrentTime(int day, int hour, int minute)
    {
        if (allTaskData == null) return new List<TaskInstance>();
        
        int currentTotalMinutes = hour * 60 + minute;

        // Return instances that match the time criteria
        var tasks = currentDayTaskInstances
            .Where(t => t.taskData.day == day && 
                       !t.isCompleted && 
                       (t.taskData.hour * 60 + t.taskData.minute) <= currentTotalMinutes)
            .ToList();

        Debug.Log($"[TaskManager] GetTasksForCurrentTime(day={day}, time={hour:00}:{minute:00}) returned {tasks.Count} tasks.");
        return tasks;
    }

    public void CompleteTask(string taskDescription)
    {
        TaskInstance task = currentDayTaskInstances.Find(t => 
            t.taskData.taskDescription.Equals(taskDescription, StringComparison.OrdinalIgnoreCase) 
            && !t.isCompleted);
            
        if (task != null)
        {
            task.Complete();
            ApplyStatEffects(task.taskData);
            OnTasksUpdated?.Invoke();
            Debug.Log($"Task Completed: {taskDescription}");
        }
    }

    public void CompleteTaskByRequirement(string requirement)
    {
        if (string.IsNullOrEmpty(requirement)) return;
        string trimmedRequirement = requirement.Trim();

        // Get currently active tasks (based on time)
        List<TaskInstance> activeTasks = GetActiveTasks();
        
        // Find the best match among active tasks for this requirement
        TaskInstance task = activeTasks
            .Where(t => t.taskData.requirementTarget != null && 
                       t.taskData.requirementTarget.Trim().Equals(trimmedRequirement, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.taskData.hour) // Oldest/earliest first
            .ThenBy(t => t.taskData.minute)
            .FirstOrDefault();

        if (task != null)
        {
            if (!task.isCompleted)
            {
                task.Complete();
                ApplyStatEffects(task.taskData);

                if (MaterialManager.instance != null)
                {
                    if (task.taskData.requirementTarget == "KitchenWall")
                    {
                        MaterialManager.instance.UpgradeMaterials(3);
                    }
                    else if (task.taskData.requirementTarget == "LivingroomWall")
                    {
                        MaterialManager.instance.UpgradeMaterials(2);
                    }
                }

                OnTasksUpdated?.Invoke();
                Debug.Log($"[TaskManager] Task Completed by Requirement: '{trimmedRequirement}' (Task: {task.taskData.taskDescription})");
            }
        }
        else
        {
            Debug.LogWarning($"[TaskManager] No active task found for requirement target: '{trimmedRequirement}'");
            
            // Log what WAS available for debugging
            string available = string.Join(", ", activeTasks.Select(t => t.taskData.requirementTarget));
            Debug.Log($"[TaskManager] Active requirement targets were: [{available}]");
        }
    }

    private void ApplyStatEffects(TaskData taskData)
    {
        if (GameManager.Instance == null) return;

        var characters = GameManager.Instance.GetCharacterComponents();

        foreach (var effect in taskData.statEffects)
        {
            // Skip if no character is assigned
            if (effect.characterName == TaskData.CharacterName.None) continue;

            // Convert enum to string (e.g., CharacterName.Sahil -> "Sahil")
            string targetName = effect.characterName.ToString();
            
            CharacterStats target = characters.Find(c => 
                c.characterName.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                
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

    public List<TaskInstance> GetActiveTasks()
    {
        if (TimeManager.Instance == null) return new List<TaskInstance>();
        return GetTasksForCurrentTime(TimeManager.Instance.days, 
                                      TimeManager.Instance.hours, 
                                      TimeManager.Instance.minutes);
    }
    
    // For UI systems that need task instances
    public List<TaskInstance> GetCurrentDayTaskInstances()
    {
        return currentDayTaskInstances;
    }
}

