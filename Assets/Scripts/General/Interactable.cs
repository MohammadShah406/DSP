using UnityEngine;

public class Interactable : MonoBehaviour
{
    private OutlineController outline;

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

    /// <summary>
    /// Optional: called when the character reaches this interactable
    /// </summary>
    public virtual void OnInteract()
    {
        Debug.Log($"{name} was interacted with!");
    }
}
