using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void OnItemClicked()
    { 
        InventoryUI.Instance.ShowItemDetails(_currentData, _currentQuantity);
    }
}
