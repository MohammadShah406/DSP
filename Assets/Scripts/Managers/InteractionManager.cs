using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InteractionManager : MonoBehaviour
{
    [Header("Interaction Sets")]
    [Tooltip("Assign character + the interactables they can interact with.")]
    public List<CharacterInteractionSet> sets;

    [Header("Runtime State (Read Only)")]
    public CharacterInteractionSet activeSet; 

    //References
    private CameraBehaviour cam;
    private Camera mainCam;
    private Transform selectedCharacter;
    private Transform lastSelectedCharacter;


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
        if (cam == null)
        {
            cam = CameraBehaviour.Instance;
        }

        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
    }

    void Update()
    {
        HandleOutline();
    }

    private void HandleOutline()
    {
        // Update selected character from camera
        selectedCharacter = (cam != null) ? cam.focussedTarget : null;

        // If selection changed, disable old outlines
        if (lastSelectedCharacter != selectedCharacter)
        {
            // Disable outlines of previous active set
            if (lastSelectedCharacter != null)
            {
                var prevSet = GetSetForCharacter(lastSelectedCharacter);
                if (prevSet != null)
                {
                    foreach (var inter in prevSet.interactables)
                        inter.SetOutline(false);
                }
            }

            lastSelectedCharacter = selectedCharacter;
        }

        if (selectedCharacter == null)
        {
            activeSet = null;
            return;
        }

        // Get active set for new selected character
        activeSet = GetSetForCharacter(selectedCharacter);
        if (activeSet == null) return;

        // Enable outlines for current character's interactables
        foreach (var inter in activeSet.interactables)
            inter.SetOutline(true);
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

}
