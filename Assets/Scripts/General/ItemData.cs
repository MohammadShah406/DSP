using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int quantity;
    public ItemType itemType;
    public Sprite icon;
    [TextArea] public string description;
}
