using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public class Interactable : MonoBehaviour
{
    [Header("Outline Controller")]
    private OutlineController outline;

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
    public UnityEvent OnInteracted;

    [Header("Interaction Settings")]
    public InteractionType interactionType = InteractionType.None;


    private void Awake()
    {
        if(outline != null) return;
        else
        {
            outline = GetComponent<OutlineController>();
            if (outline == null)
            {
                Debug.LogWarning($"Interactable {name} has no OutlineController!");
            }
        }   
    }

   

    private void Start()
    {
        timeManager = TimeManager.Instance;
        interactionManager = InteractionManager.Instance;
    }

    private void Update()
    {
        if (timeRestricted && !canInteract && timeManager != null)
        {
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
        if (outline == null) return;

        if (enabled)
        {
            outline.EnableOutlineInteractable();
        }
        else
        {
            outline.DisableOutline();
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
        OnInteracted?.Invoke();
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
    }

    public void AddToInteractionManager()
    {
        if (interactionManager != null)
        {
            interactionManager.AddInteractable(allowedCharacters,this);
        }
    }
}
