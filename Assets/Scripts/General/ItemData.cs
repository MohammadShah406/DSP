using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    [TextArea] public string description;
}

public enum ItemType
{
    FoodIngredient,
    Material,
    Placement,
    Food,
    Other
}
