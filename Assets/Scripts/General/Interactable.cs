using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;

public class Interactable : MonoBehaviour
{
    [Header("Outline Controller")]
    private OutlineController _outline;

    [Header("Icon Controller")]
    [SerializeField] private GameObject iconGameObject;

    [Header("Singleton Reference")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private InteractionManager interactionManager; 

    [Header("Time Restrictions")]
    [SerializeField] private bool timeRestricted = false;
    [SerializeField] private int afterHour = 10;
    [SerializeField] private int afterMinutes = 0;
    [SerializeField] private bool canInteract = true;

    [Header("Who can Interact")]
    [SerializeField] private List<Transform> allowedCharacters;
    
    [Header("Task Integration")]
    [SerializeField] private string interactionRequirement;
    
    public string InteractionRequirement => interactionRequirement;

    [System.Serializable]
    public class AtrributeList
    {
        public CharacterStats.PrimaryAttribute characterAttribute;
        public int amount;
    }

    [Header("Effect on Character")]
    public List<AtrributeList> characterEffectsList = new List<AtrributeList>();


    public enum InteractionType
    {
        None,
        Harvest,
        Cook,
        Scavenge,
        Rest,
        Talk,
        Paint,
        Watering,
    }

    [Header("Runtime State (Read Only)")]
    [SerializeField] private GameObject interactedBy;

    [Header("Interaction Event")]
    public UnityEvent onInteracted;

    [Header("Interaction Settings")]
    public InteractionType interactionType = InteractionType.None;
    
    private void Awake()
    {
        if(_outline != null) return;
        else
        {
            _outline = GetComponent<OutlineController>();
            if (_outline == null)
            {
                Debug.LogWarning($"Interactable {name} has no OutlineController!");
            }
        }   
    }

   

    private void Start()
    {
        timeManager = TimeManager.Instance;
        interactionManager = InteractionManager.Instance;

        if(!timeRestricted)
        {
            canInteract = true;
            AddToInteractionManager();
        }
    }

    private void Update()
    {

        if (timeRestricted && !canInteract && timeManager != null)
        {
            Debug.Log($"Checking time for {name} interaction availability...");
            if (timeManager.hours >= afterHour && timeManager.minutes >= afterMinutes)
            {
                canInteract = true;
                Debug.Log($"{name} is now able to be interacted with after time check.");
                AddToInteractionManager();
            }
        }
    }

    /// <summary>
    /// Enable or disable the outline.
    /// </summary>
    public void SetOutline(bool enabled)
    {
        if (_outline == null) return;

        if (enabled)
        {
            _outline.EnableOutlineInteractable();
        }
        else
        {
            _outline.DisableOutline();
        }
    }

    public void OnInteract()
    {
        Debug.Log($"{name} was interacted with!");
        CallInteractEvent();
    }

    public void OnInteract(GameObject character)
    {
        SetInteractedBy(character);
        Debug.Log($"{name} was interacted with!");
        Debug.Log($"{character.name} interacted with {name}!");
        DoInteractionAnimation();
        CallInteractEvent();
    }

    private void DoInteractionAnimation()
    {
        Debug.Log("Playing interaction animation...");
        // Trigger animation on character
        var movement = interactedBy.GetComponent<CharacterMovement>();
        if (movement != null)
        {
            movement.PlayInteractionAnimation(interactionType);
        }
    }

    public void CallInteractEvent()
    {
        onInteracted?.Invoke();
    }

    public void SetInteractedBy(GameObject character)
    {
        interactedBy = character;
    }

    public void ClearInteractedBy()
    {
        interactedBy = null;
    }

    public void DebugMessage()
    {
        Debug.Log("Debug message called");
    }

    public void InteractComplete()
    {
        Debug.Log($"{name} interaction complete.");
        if (TaskManager.Instance != null && !string.IsNullOrEmpty(interactionRequirement))
        {
            TaskManager.Instance.CompleteTaskByRequirement(interactionRequirement);
        }
        ApplyEffect();
    }

    public void AddToInteractionManager()
    {
        if (interactionManager != null)
        {
            Debug.Log($"Adding {name} to InteractionManager for allowed characters.");
            interactionManager.AddInteractable(allowedCharacters,this);
        }
    }

    public void IncreaseStats()
    {
        var characterStats = interactedBy.GetComponent<CharacterStats>();
        switch (interactionType)
        {
            case InteractionType.Harvest:
                Debug.Log("Harvest interaction completed.");
                break;
            case InteractionType.Cook:
                characterStats.ChangeWorkReadiness(5);
                Debug.Log("Cook interaction completed.");
                break;
            case InteractionType.Scavenge:
                Debug.Log("Scavenge interaction completed.");
                break;
            case InteractionType.Rest:
                Debug.Log("Rest interaction completed.");
                break;
            case InteractionType.Talk:
                Debug.Log("Talk interaction completed.");
                break;
            case InteractionType.Paint:
                Debug.Log("Paint interaction completed.");
                break;
            case InteractionType.Watering:
                Debug.Log("Watering interaction completed.");
                break;
            default:
                // Default
                break;
        }
    }


    private void ApplyEffect()
    {
        var characterStats = interactedBy.GetComponent<CharacterStats>();

        foreach (var attribute in characterEffectsList)
        {
            switch (attribute.characterAttribute)
            {
                case CharacterStats.PrimaryAttribute.Stability:
                    characterStats.ChangeStability(attribute.amount);
                    break;
                case CharacterStats.PrimaryAttribute.Learning:
                    characterStats.ChangeLearning(attribute.amount);
                    break;
                case CharacterStats.PrimaryAttribute.WorkReadiness:
                    characterStats.ChangeWorkReadiness(attribute.amount);
                    break;
                case CharacterStats.PrimaryAttribute.Trust:
                    characterStats.ChangeTrust(attribute.amount);
                    break;
                case CharacterStats.PrimaryAttribute.Nutrition:
                    characterStats.ChangeNutrition(attribute.amount);
                    break;
                case CharacterStats.PrimaryAttribute.Hygiene:
                    characterStats.ChangeHygiene(attribute.amount);
                    break;
                case CharacterStats.PrimaryAttribute.Energy:
                    characterStats.ChangeEnergy(attribute.amount);
                    break;
            }

        }
    }

    public void SetIconActive(bool active)
    {
        if (iconGameObject != null)
        {
            iconGameObject.SetActive(active);
        }
    }
}
