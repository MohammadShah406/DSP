using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public class Interactable : MonoBehaviour
{
    private OutlineController outline;

    public enum InteractionType
    {
        None,
        Harvest,
        Cook
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
}
