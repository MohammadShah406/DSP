using UnityEngine;

/// <summary>
/// This script handles the activation of objects in the level based on items in the inventory.
/// It searches for a child object within a specified parent that matches the item's name.
/// </summary>
public class ObjectPlacementActivator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The parent object containing all the level objects that can be activated.")]
    public Transform placementParent;

    /// <summary>
    /// Activates an object in the level by searching for a child within placementParent 
    /// that matches the provided itemName.
    /// </summary>
    /// <param name="itemName">The name of the item (and the object to activate).</param>
    /// <returns>True if a matching object was found and activated, otherwise false.</returns>
    public bool ActivateItem(string itemName)
    {
        if (placementParent == null)
        {
            Debug.LogError("[ObjectPlacementActivator] placementParent is not assigned!");
            return false;
        }

        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("[ObjectPlacementActivator] itemName is null or empty!");
            return false;
        }

        // Search through all children of the placementParent
        foreach (Transform child in placementParent)
        {
            if (child.name.Equals(itemName, System.StringComparison.OrdinalIgnoreCase))
            {
                if (child.gameObject.activeSelf)
                {
                    Debug.Log($"[ObjectPlacementActivator] Object '{itemName}' is already active.");
                    return true;
                }

                child.gameObject.SetActive(true);
                Debug.Log($"[ObjectPlacementActivator] Successfully activated object: {child.name}");
                
                // Additional logic for upgrades and feedback
                HandlePlacementSuccess();
                
                return true;
            }
        }

        Debug.LogWarning($"[ObjectPlacementActivator] No object found with name '{itemName}' under '{placementParent.name}'.");
        return false;
    }

    private void HandlePlacementSuccess()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.upgradesDone += 1;
        }

        if (AudioPlayer.Instance != null && AudioLibrary.Instance != null)
        {
            // Assuming "upgradedone" is the sound for placing items as per DonationManager
            AudioClip clip = AudioLibrary.Instance.GetSfx("upgradedone");
            if (clip != null)
            {
                AudioPlayer.Instance.Play(clip);
            }
        }
    }
}
