using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemQuantityText;
    public Button itemButton;

    private ItemData _currentData;
    private int _currentQuantity;

    public void Setup(ItemData data, int quantity)
    {
        _currentData = data;
        _currentQuantity = quantity;

        if (data != null)
        {
            itemNameText.text = data.itemName;
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
            if (itemIcon != null) itemIcon.enabled = false;
        }

        itemQuantityText.text = quantity.ToString();

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClicked);
        }
    }

    private void OnItemClicked()
    {
        if (_currentData != null)
        {
            UIManager.Instance.ShowItemDetails(_currentData, _currentQuantity);
        }
    }
}
