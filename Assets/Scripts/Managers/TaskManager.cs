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
    private Dictionary<string, TaskInstance> activeTasksByRequirement = new Dictionary<string, TaskInstance>();
    
    
    
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
            UpdateDayTasks(TimeManager.Instance.days);
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
        CheckAllActiveObjectTasks();
        OnTasksUpdated?.Invoke();
    }

    /// <summary>
    /// Checks all active tasks that have an object requirement and completes them if the object is active.
    /// </summary>
    private void CheckAllActiveObjectTasks()
    {
        if (DonationManager.instance == null) return;

        foreach (var task in currentDayTaskInstances)
        {
            if (task.isActive && !task.isCompleted && task.taskData.taskType == TaskData.TaskType.ObjectActivation && !string.IsNullOrEmpty(task.taskData.requirementTarget))
            {
                if (DonationManager.instance.IsObjectActive(task.taskData.requirementTarget))
                {
                    CompleteTask(task.taskData.taskDescription);
                }
            }
        }
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

        // Create new runtime instances for the day
        currentDayTaskInstances.Clear();
        activeTasksByRequirement.Clear();
        
        var dayTasks = allTaskData.Where(t => t != null && t.day == day).ToList();
        
        foreach (var taskData in dayTasks)
        {
            TaskInstance instance = new TaskInstance(taskData);
            currentDayTaskInstances.Add(instance);
            
            // Index by requirement for quick lookup
            if (taskData.taskType == TaskData.TaskType.Interaction && !string.IsNullOrEmpty(taskData.requirementTarget))
            {
                activeTasksByRequirement[taskData.requirementTarget] = instance;
            }
        }
        
        OnTasksUpdated?.Invoke();
    }

    private void CheckForTaskActivation(int day, int hour, int minute)
    {
        foreach (var taskInstance in currentDayTaskInstances)
        {
            // Only activate if not already active and not completed
            if (!taskInstance.isActive && !taskInstance.isCompleted)
            {
                // Activate if time matches
                if (taskInstance.taskData.day == day && 
                    taskInstance.taskData.hour == hour && 
                    taskInstance.taskData.minute == minute)
                {
                    taskInstance.Activate();
                    Debug.Log($"[TaskManager] Activated task: {taskInstance.taskData.taskDescription}");

                    // Immediately check if this task is an "Object Active" task and if it's already fulfilled
                    if (taskInstance.taskData.taskType == TaskData.TaskType.ObjectActivation && !string.IsNullOrEmpty(taskInstance.taskData.requirementTarget))
                    {
                        if (DonationManager.instance != null && 
                            DonationManager.instance.IsObjectActive(taskInstance.taskData.requirementTarget))
                        {
                            CompleteTask(taskInstance.taskData.taskDescription);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Checks all active tasks to see if any are waiting for a specific object to be activated.
    /// </summary>
    public void CheckObjectTasks(string objectName, string characterName = "")
    {
        if (string.IsNullOrEmpty(objectName)) return;

        // Find all active, incomplete tasks that require this object
        var tasksToComplete = currentDayTaskInstances
            .Where(t => t.isActive && !t.isCompleted && 
                       t.taskData.taskType == TaskData.TaskType.ObjectActivation &&
                       !string.IsNullOrEmpty(t.taskData.requirementTarget) && 
                       t.taskData.requirementTarget.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var task in tasksToComplete)
        {
            CompleteTask(task.taskData.taskDescription, characterName);
        }
    }
    
    public List<TaskInstance> GetTasksForCurrentTime(int day, int hour, int minute)
    {
        if (allTaskData == null) return new List<TaskInstance>();
        
        // Return instances that match the time criteria
        return currentDayTaskInstances
            .Where(t => t.taskData.day == day && 
                       !t.isCompleted && 
                       t.taskData.hour <= hour)
            .ToList();
    }

    public void CompleteTask(string taskDescription, string characterName = "")
    {
        TaskInstance task = currentDayTaskInstances.Find(t => 
            t.taskData.taskDescription.Equals(taskDescription, StringComparison.OrdinalIgnoreCase) 
            && !t.isCompleted);
            
        if (task != null)
        {
            // Check if there is a required character
            if (task.taskData.requiredCharacter != TaskData.CharacterName.None)
            {
                string requiredName = task.taskData.requiredCharacter.ToString();
                if (!requiredName.Equals(characterName, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[TaskManager] Task {taskDescription} requires {requiredName}, but was completed by {characterName}.");
                    return;
                }
            }

            task.Complete();
            ApplyStatEffects(task.taskData);
            OnTasksUpdated?.Invoke();
            Debug.Log($"Task Completed: {taskDescription}");
        }
    }

    public void CompleteTaskByRequirement(string requirement, string characterName = "")
    {
        if (string.IsNullOrEmpty(requirement)) return;

        if (activeTasksByRequirement.TryGetValue(requirement, out TaskInstance task))
        {
            if (!task.isCompleted)
            {
                // Check if there is a required character
                if (task.taskData.requiredCharacter != TaskData.CharacterName.None)
                {
                    string requiredName = task.taskData.requiredCharacter.ToString();
                    if (!requiredName.Equals(characterName, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log($"[TaskManager] Task {task.taskData.taskDescription} requires {requiredName}, but was completed by {characterName}.");
                        return;
                    }
                }

                task.Complete();
                ApplyStatEffects(task.taskData);
                OnTasksUpdated?.Invoke();
                Debug.Log($"Task Completed by Requirement: {requirement}");
            }
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

