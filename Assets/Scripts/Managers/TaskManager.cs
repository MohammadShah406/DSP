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
    public bool isFailed;
    public float startTime;
    public float activationTotalMinutes;
    public Interactable assignedInteractable;
    public List<string> completedByCharacters = new List<string>();
    
    public TaskInstance(TaskData data)
    {
        taskData = data;
        isActive = false;
        isCompleted = false;
        isFailed = false;
    }
    
    public void Activate(float currentTotalMinutes)
    {
        isActive = true;
        startTime = Time.time;
        activationTotalMinutes = currentTotalMinutes;
    }
    
    public void Complete()
    {
        isCompleted = true;
        isActive = false;
    }

    public void Fail()
    {
        isFailed = true;
        isActive = false;
        Debug.Log($"[TaskInstance] Task failed: {taskData.taskDescription}");
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
        TimeManager.Instance.DayChanged += UpdateDayTasks;
        TimeManager.Instance.MinuteChanged += HandleMinuteChanged;
        UpdateDayTasks(TimeManager.Instance.days);
    }

    private void Update()
    {
        if (TimeManager.Instance == null || TimeManager.Instance.IsPaused) return;

        CheckForRealTimeFailures();
    }

    private void CheckForRealTimeFailures()
    {
        float currentTotalMinutes = (TimeManager.Instance.days * 24 * 60) + (TimeManager.Instance.hours * 60) + TimeManager.Instance.minutes + TimeManager.Instance.MinuteAccumulator;
        bool anyUpdate = false;

        foreach (var taskInstance in currentDayTaskInstances)
        {
            if (!taskInstance.isActive || taskInstance.isCompleted || taskInstance.isFailed || !taskInstance.taskData.timedTask)
                continue;

            if (taskInstance.taskData.timeConditionType == TaskData.TimeCondition.FromActivation)
            {
                if (currentTotalMinutes >= taskInstance.activationTotalMinutes + taskInstance.taskData.completeIn)
                {
                    taskInstance.Fail();
                    anyUpdate = true;
                }
            }
            else if (taskInstance.taskData.timeConditionType == TaskData.TimeCondition.ByDayHourMinute)
            {
                int taskFailTotalMinutes = (taskInstance.taskData.completeByDay * 24 * 60) + (taskInstance.taskData.completeByHour * 60) + taskInstance.taskData.completeByMinute;

                if (currentTotalMinutes >= taskFailTotalMinutes)
                {
                    taskInstance.Fail();
                    anyUpdate = true;
                }
            }
        }

        if (anyUpdate)
        {
            OnTasksUpdated?.Invoke();
        }
    }

    private void OnDestroy()
    {
        TimeManager.Instance.DayChanged -= UpdateDayTasks;
        TimeManager.Instance.MinuteChanged -= HandleMinuteChanged;
    }
    
    private void HandleMinuteChanged(int h, int m, int d)
    {
        CheckForTaskStatus(d, h, m);
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
    
    private void CheckForTaskStatus(int day, int hour, int minute)
    {
        float currentTotalMinutes = (day * 24 * 60) + (hour * 60) + minute + TimeManager.Instance.MinuteAccumulator;

        foreach (var taskInstance in currentDayTaskInstances)
        {
            if (taskInstance.isCompleted || taskInstance.isFailed)
                continue;
            
            if (!taskInstance.isActive)
            {
                int taskActivationTotalMinutes = (taskInstance.taskData.day * 24 * 60) + (taskInstance.taskData.hour * 60) + taskInstance.taskData.minute;
                
                if (currentTotalMinutes >= taskActivationTotalMinutes)
                {
                    taskInstance.Activate(currentTotalMinutes);
                    Debug.Log($"[TaskManager] Activated task: {taskInstance.taskData.taskDescription}");
                }
            }
            
            if (taskInstance.isActive && taskInstance.taskData.timedTask)
            {
                if (taskInstance.taskData.timeConditionType == TaskData.TimeCondition.FromActivation)
                {
                    if (currentTotalMinutes >= taskInstance.activationTotalMinutes + taskInstance.taskData.completeIn)
                    {
                        taskInstance.Fail();
                    }
                }
                else if (taskInstance.taskData.timeConditionType == TaskData.TimeCondition.ByDayHourMinute)
                {
                    int taskFailTotalMinutes = (taskInstance.taskData.completeByDay * 24 * 60) + (taskInstance.taskData.completeByHour * 60) + taskInstance.taskData.completeByMinute;
                    
                    if (currentTotalMinutes >= taskFailTotalMinutes)
                    {
                        taskInstance.Fail();
                    }
                }
            }
        }
    }
    
    public List<TaskInstance> GetTasksForCurrentTime(int day, int hour, int minute)
    {
        if (allTaskData == null) return new List<TaskInstance>();
        
        int currentTotalMinutes = (day * 24 * 60) + (hour * 60) + minute;
        
        var tasks = currentDayTaskInstances
            .Where(t => !t.isCompleted && !t.isFailed && t.isActive)
            .ToList();

        Debug.Log($"[TaskManager] GetTasksForCurrentTime(day={day}, time={hour:00}:{minute:00}) returned {tasks.Count} tasks.");
        return tasks;
    }

    public void CompleteTask(string taskDescription, string characterName = "")
    {
        TaskInstance task = currentDayTaskInstances.Find(t => 
            t.taskData.taskDescription.Equals(taskDescription, StringComparison.OrdinalIgnoreCase) 
            && !t.isCompleted && !t.isFailed);
            
        if (task != null)
        {
            if (task.taskData.requiredCharacter == TaskData.CharacterName.All)
            {
                if (!string.IsNullOrEmpty(characterName) && !task.completedByCharacters.Contains(characterName))
                {
                    task.completedByCharacters.Add(characterName);
                    Debug.Log($"[TaskManager] Character '{characterName}' completed their part of task: {task.taskData.taskDescription}");
                }
                
                int totalCharacters = InteractionManager.Instance != null ? InteractionManager.Instance.sets.Count : 0;
                if (task.completedByCharacters.Count < totalCharacters)
                {
                    Debug.Log($"[TaskManager] Task '{task.taskData.taskDescription}' progress: {task.completedByCharacters.Count}/{totalCharacters}");
                    OnTasksUpdated?.Invoke();
                    return;
                }
            }

            task.Complete();
            ApplyStatEffects(task.taskData);
            ApplyUnlocks(task.taskData);
            OnTasksUpdated?.Invoke();
            Debug.Log($"Task Completed: {taskDescription}");
        }
    }
    
    public void CompleteTaskByRequirement(string requirement, string characterName = "")
    {
        if (string.IsNullOrEmpty(requirement)) return;
        string trimmedRequirement = requirement.Trim();
        
        List<TaskInstance> activeTasks = GetActiveTasks();
        
        TaskInstance task = activeTasks
            .Where(t => !t.isFailed && t.taskData.requirementTarget != null && 
                       t.taskData.requirementTarget.Trim().Equals(trimmedRequirement, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.taskData.hour)
            .ThenBy(t => t.taskData.minute)
            .FirstOrDefault();

        if (task != null)
        {
            if (!task.isCompleted)
            {
                if (task.taskData.requiredCharacter == TaskData.CharacterName.All)
                {
                    if (!string.IsNullOrEmpty(characterName) && !task.completedByCharacters.Contains(characterName))
                    {
                        task.completedByCharacters.Add(characterName);
                        Debug.Log($"[TaskManager] Character '{characterName}' completed their part of task: {task.taskData.taskDescription}");
                    }
                    
                    int totalCharacters = InteractionManager.Instance != null ? InteractionManager.Instance.sets.Count : 0;
                    if (task.completedByCharacters.Count < totalCharacters)
                    {
                        Debug.Log($"[TaskManager] Task '{task.taskData.taskDescription}' progress: {task.completedByCharacters.Count}/{totalCharacters}");
                        OnTasksUpdated?.Invoke();
                        return;
                    }
                }

                task.Complete();
                ApplyStatEffects(task.taskData);
                ApplyUnlocks(task.taskData);

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
            
            string available = string.Join(", ", activeTasks.Select(t => t.taskData.requirementTarget));
            Debug.Log($"[TaskManager] Active requirement targets were: [{available}]");
        }
    }

    public void CompleteTaskByProduct(string product, string characterName = "")
    {
        if (string.IsNullOrEmpty(product)) return;
        string trimmedProduct = product.Trim();

        List<TaskInstance> activeTasks = GetActiveTasks();
        
        TaskInstance task = activeTasks
            .Where(t => !t.isFailed && t.taskData.requirementProduct != null && 
                       t.taskData.requirementProduct.Trim().Equals(trimmedProduct, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.taskData.hour)
            .ThenBy(t => t.taskData.minute)
            .FirstOrDefault();

        if (task != null)
        {
            if (!task.isCompleted)
            {
                if (task.taskData.requiredCharacter == TaskData.CharacterName.All)
                {
                    if (!string.IsNullOrEmpty(characterName) && !task.completedByCharacters.Contains(characterName))
                    {
                        task.completedByCharacters.Add(characterName);
                    }

                    int totalCharacters = InteractionManager.Instance != null ? InteractionManager.Instance.sets.Count : 0;
                    if (task.completedByCharacters.Count < totalCharacters)
                    {
                        OnTasksUpdated?.Invoke();
                        return;
                    }
                }

                task.Complete();
                ApplyStatEffects(task.taskData);
                ApplyUnlocks(task.taskData);
                OnTasksUpdated?.Invoke();
                Debug.Log($"[TaskManager] Task Completed by Product: '{trimmedProduct}' (Task: {task.taskData.taskDescription})");
            }
        }
        else
        {
            Debug.LogWarning($"[TaskManager] No active task found for product: '{trimmedProduct}'");
        }
    }
    
    private void ApplyStatEffects(TaskData taskData)
    {
        if (GameManager.Instance == null) return;

        var characters = GameManager.Instance.GetCharacterComponents();

        foreach (var effect in taskData.statEffects)
        {
            if (effect.characterName == TaskData.CharacterName.None) continue;
            
            string targetName = effect.characterName.ToString();
            
            CharacterStats target = characters.Find(c => 
                c.characterName.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                
            if (target != null)
            {
                ApplyEffect(target, effect.attribute, effect.amount);
            }
        }
    }

    private void ApplyUnlocks(TaskData taskData)
    {
        if (taskData.unlockRecipes == null || taskData.unlockRecipes.Count == 0) return;

        if (RecipeDataBase.Instance != null)
        {
            foreach (var recipe in taskData.unlockRecipes)
            {
                if (recipe == null) continue;
                
                bool alreadyUnlocked = RecipeDataBase.Instance.activeRecipes.Any(r => r == recipe);
                if (!alreadyUnlocked)
                {
                    List<Recipes> activeList = new List<Recipes>(RecipeDataBase.Instance.activeRecipes);
                    activeList.Add(recipe);
                    RecipeDataBase.Instance.activeRecipes = activeList.ToArray();
                    Debug.Log($"[TaskManager] Unlocked Recipe: {recipe.recipeName}");
                }
            }
        }
        else
        {
            Debug.LogWarning("[TaskManager] RecipeManager instance not found. Cannot unlock recipes.");
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
    
    public List<TaskInstance> GetCurrentDayTaskInstances()
    {
        return currentDayTaskInstances;
    }
}

