using System.Net.Mail;
using UnityEngine;

public class RecipeDataBase : MonoBehaviour
{
    public static RecipeDataBase Instance { get; private set; }

    [Header("Recipes")] public Recipes[] activeRecipes;
    public Recipes[] recipesDataBase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool CanCraft(Recipes recipe)
    {
        foreach (var ingredient in recipe.resourcesNeeded)
        {
            var resource = GameManager.Instance.resources.Find(r => r.resourceName == ingredient.resourceName);

            if (resource == null || resource.quantity < ingredient.quantity)
                return false;
        }

        return true;
    }

    public void Craft(Recipes recipe)
    {
        if (!CanCraft(recipe)) return;

        foreach (var ingredient in recipe.resourcesNeeded)
        {
            GameManager.Instance.resources.Find(r => r.resourceName == ingredient.resourceName).quantity -= ingredient.quantity;
        }
        
        GameManager.Instance.AddResource(recipe.outputItem, recipe.outputAmount);
        
        TaskManager.Instance.CompleteTaskByProduct(recipe.outputItem);
    }
}