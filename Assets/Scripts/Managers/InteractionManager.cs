using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class InteractionManager : MonoBehaviour
{
    [Header("Interaction Sets")]
    [Tooltip("Assign character + the interactable they can interact with.")]
    public List<CharacterInteractionSet> sets;

    [Header("Runtime State (Read Only)")]
    public CharacterInteractionSet activeSet;
    
    [Header("Available Interactable")]
    [Tooltip("Parent GameObject containing all interactable objects in the scene")]
    public GameObject interactableParent;

    //References
    private CameraBehaviour _cam;
    private Camera _mainCam;
    private Transform _selectedCharacter;
    private Transform _lastSelectedCharacter;


    public static InteractionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (_cam == null)
        {
            _cam = CameraBehaviour.Instance;
        }

        if (_mainCam == null)
        {
            _mainCam = Camera.main;
        }
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTasksUpdated += UpdateInteractablesFromTasks;
            UpdateInteractablesFromTasks(); // Initial update
        }
    }

    void Update()
    {
        HandleOutlineAndIcon();
    }

    private void HandleOutlineAndIcon()
    {
        // Update selected character from camera
        _selectedCharacter = (_cam != null) ? _cam.focussedTarget : null;

        // If selection changed, disable old outlines
        if (_lastSelectedCharacter != _selectedCharacter)
        {
            // Disable outlines of previous active set
            if (_lastSelectedCharacter != null)
            {
                var prevSet = GetSetForCharacter(_lastSelectedCharacter);
                if (prevSet != null)
                {
                    foreach (var inter in prevSet.interactables)
                    {
                        inter.SetOutline(false);
                        inter.SetIconActive(false);
                    }
                }
            }

            _lastSelectedCharacter = _selectedCharacter;
        }

        if (_selectedCharacter == null)
        {
            activeSet = null;
            return;
        }

        // Get active set for new selected character
        activeSet = GetSetForCharacter(_selectedCharacter);
        if (activeSet == null) return;

        // Enable outlines for current character's interactables
        foreach (var inter in activeSet.interactables)
        {
            inter.SetOutline(true);
            inter.SetIconActive(true);
        }
    }

    private CharacterInteractionSet GetSetForCharacter(Transform character)
    {
        foreach (var set in sets)
        {
            if (set.character == character)
                return set;
        }
        return null;
    }

    public void TryInteract(Transform selectedChar, bool runFlag, Interactable interactable)
    {
        // Ensure the interactable belongs to THIS character
        if (!activeSet.interactables.Contains(interactable))
            return;
        Debug.Log($"Interacting with {interactable.name}");

        CharacterMovement mover = selectedChar.GetComponent<CharacterMovement>();
        if (mover == null)
            return;

        mover.SetTarget(interactable.transform.position, runFlag: runFlag, interactable: interactable);

        Debug.Log($"{selectedChar.name} moving to {interactable.name}");
    }

    public bool IsInteractableForCharacter(Transform interactable, Transform character)
    {
        var set = GetSetForCharacter(character);
        return set != null && set.interactables.Contains(interactable.GetComponent<Interactable>());
    }

    public void AddInteractable(List<Transform> characters, Interactable interactable)
    {
        foreach (var character in characters)
        {
            var set = GetSetForCharacter(character);
            if (set != null && !set.interactables.Contains(interactable))
            {
                set.interactables.Add(interactable);
            }
        }
    }

    public void RemoveInteractable(Interactable interactable)
    {
        foreach (var set in sets)
        {
            set.interactables.Remove(interactable);
        }
    }
    
    private void ClearAllInteractables()
    {
        foreach (var set in sets)
        {
            set.interactables.Clear();
        }
    }
    
    private void UpdateInteractablesFromTasks()
    {
        if (TaskManager.Instance == null) return;
        
        
        // Get active tasks
        List<TaskInstance> activeTasks = TaskManager.Instance.GetActiveTasks();
        
        foreach (var taskInstance in activeTasks)
        {
            if (taskInstance.isActive && !taskInstance.isCompleted)
            {
                string requirement = taskInstance.taskData.actionRequirement;
                
                if (string.IsNullOrEmpty(requirement)) continue;
                
                // Find interactable with matching requirement
                Interactable foundInteractable = FindInteractableByRequirement(requirement);
                
                if (foundInteractable != null)
                {
                    // Store the actual object reference in the task
                    taskInstance.assignedInteractable = foundInteractable;
                    
                    // Determine which characters should have access
                    List<Transform> eligibleCharacters = GetCharactersForTask(taskInstance);
                    
                    // Add the actual interactable OBJECT to character's list
                    AddInteractable(eligibleCharacters, foundInteractable);
                    
                    Debug.Log($"[InteractionManager] Found and added '{foundInteractable.name}' for requirement '{requirement}'");
                }
                else
                {
                    Debug.LogWarning($"[InteractionManager] Could not find interactable for requirement: {requirement}");
                }
            }
        }
    }

    private Interactable FindInteractableByRequirement(string requirement)
    {
        if (interactableParent == null)
        {
            Debug.LogError("[InteractionManager] interactableParent is not assigned!");
            return null;
        }
        
        foreach (Transform child in interactableParent.GetComponentsInChildren<Transform>())
        {
            if (child == interactableParent.transform) continue;
        
            // Compare GameObject NAME to requirement string
            if (child.gameObject.name.Equals(requirement, System.StringComparison.OrdinalIgnoreCase))
            {
                Interactable interactable = child.GetComponent<Interactable>();
                if (interactable != null)
                {
                    return interactable;
                }
            }
        }
        
        return null;
    }
    
    private List<Transform> GetCharactersForTask(TaskInstance taskInstance)
    {
        List<Transform> characters = new List<Transform>();
        
        // Option 1: If task stat effects tell us which character
        if (taskInstance.taskData.statEffects.Count > 0)
        {
            foreach (var effect in taskInstance.taskData.statEffects)
            {
                Transform character = FindCharacterByName(effect.characterName);
                if (character != null && !characters.Contains(character))
                {
                    characters.Add(character);
                }
            }
        }
        
        // Option 2: If no specific character, add to all characters
        if (characters.Count == 0)
        {
            foreach (var set in sets)
            {
                if (set.character != null)
                {
                    characters.Add(set.character);
                }
            }
        }
        
        return characters;
    }
    
    private Transform FindCharacterByName(string characterName)
    {
        foreach (var set in sets)
        {
            if (set.character != null)
            {
                CharacterStats stats = set.character.GetComponent<CharacterStats>();
                if (stats != null && stats.characterName.Equals(characterName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return set.character;
                }
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTasksUpdated -= UpdateInteractablesFromTasks;
        }
    }
}
