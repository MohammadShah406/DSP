using UnityEngine;
using System.Collections.Generic;

public enum RecipeType
{
    Crafting,
    Cooking
}

[CreateAssetMenu(fileName = "Recipes", menuName = "Recipes")]
public class Recipes : ScriptableObject
{
    [Header("Recipe Name")]
    public string recipeName;
    
    [Header("Recipe Type")]
    public RecipeType type;
    
    [Header("Items Needed")]
    public List<ResourceData> resourcesNeeded;
    public int timeToComplete;
    public string description;
    
    [Header("Output Items")]
    public string outputItem;
    public int outputAmount;
}
