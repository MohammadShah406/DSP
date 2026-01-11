using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Represents the user interface logic for an inventory item.
/// This class is used to display and manage the visual representation
/// of an individual item within the inventory UI.
public class InventoryItemUI : MonoBehaviour
{
    
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public Button itemButton;
    private ItemData _currentData;
    private int _currentQuantity;
    
    
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
