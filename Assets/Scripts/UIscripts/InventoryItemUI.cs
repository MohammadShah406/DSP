using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Represents the user interface logic for an inventory item.
/// This class is used to display and manage the visual representation
/// of an individual item within the inventory UI.
public class InventoryItemUI : MonoBehaviour
{
    /// <summary>
    /// Represents the UI image component used to display the icon of an inventory item.
    /// </summary>
    public Image itemIcon;

    /// <summary>
    /// Represents a TextMeshProUGUI component used to display the name of an item
    /// in the inventory UI. This text element is updated dynamically by the InventoryItemUI
    /// to reflect the name of the currently assigned item or a default placeholder if no item is assigned.
    /// </summary>
    public TextMeshProUGUI itemNameText;

    /// <summary>
    /// Represents the button associated with an inventory item in the UI.
    /// This button is used to trigger interactions, such as displaying item details or performing item-specific actions.
    /// </summary>
    public Button itemButton;

    /// <summary>
    /// Holds the reference to the current item data being represented by the UI.
    /// This variable is used to store information about the item, such as its name,
    /// type, icon, and description, which are needed for displaying and interacting
    /// with the inventory system.
    /// </summary>
    private ItemData _currentData;

    /// <summary>
    /// Represents the quantity of the current inventory item being managed by this UI component.
    /// This value is updated during the setup process and used to display item details
    /// and handle inventory-related interactions.
    /// </summary>
    private int _currentQuantity;

    /// <summary>
    /// Configures the InventoryItemUI instance with the specified item data and quantity.
    /// </summary>
    /// <param name="data">The item data to display in the UI.</param>
    /// <param name="quantity">The quantity of the item to be displayed.</param>
    public void Setup(ItemData data, int quantity)
    {
        _currentData = data;
        _currentQuantity = quantity;

        if (data != null)
        {
            if (itemNameText != null) itemNameText.text = data.itemName;
            if (itemIcon != null)
            {
                if (data.icon != null)
                {
                    itemIcon.sprite = data.icon;
                    itemIcon.enabled = true;
                }
                else
                {
                    itemIcon.enabled = false;
                }
            }
            
        }
        else
        {
            itemNameText.text = "Unknown Item";
            itemIcon.enabled = false;
        }
        
        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(OnItemClicked);
    }

    /// <summary>
    /// Handles the click event for an inventory item within the user interface.
    /// This method is triggered when the associated item's button is clicked,
    /// and it displays the details of the clicked item in the inventory UI.
    /// </summary>
    private void OnItemClicked()
    { 
        InventoryUI.Instance.ShowItemDetails(_currentData, _currentQuantity);
    }
}
