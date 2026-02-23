using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeUI : MonoBehaviour
{
    [Header("UI Mode Settings")]
    public TextMeshProUGUI titleText;
    
    [Header("Action Button Settings")]
    public TextMeshProUGUI actionButtonText;

    [Header("Recipe List")]
    public Transform recipeContainer;
    public GameObject recipeItemPrefab;

    [Header("Details Panel")]
    public GameObject detailsPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public Transform ingredientsContainer;
    public GameObject ingredientItemPrefab;
    public Button craftButton;
    public Button exitButton;

    private RecipeType _currentMode;
    private Recipes _selectedRecipe;

    public void Setup(RecipeType mode)
    {
        _currentMode = mode;
        
        if (titleText != null)
            titleText.text = (mode == RecipeType.Crafting) ? "Crafting Station" : "Cooking Station";
            
        if (actionButtonText != null)
            actionButtonText.text = (mode == RecipeType.Crafting) ? "Craft" : "Cook";
        
        if (detailsPanel != null)
            detailsPanel.SetActive(false);

        PopulateRecipeList();
    }

    private void PopulateRecipeList()
    {
        foreach (Transform child in recipeContainer)
        {
            Destroy(child.gameObject);
        }

        if (RecipeDataBase.Instance == null) return;
        
        foreach (var recipe in RecipeDataBase.Instance.recipesDataBase)
        {
            if (recipe.type == _currentMode)
            {
                GameObject item = Instantiate(recipeItemPrefab, recipeContainer);
                var text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = recipe.recipeName;
                
                var btn = item.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnRecipeSelected(recipe));
                }
            }
        }
    }

    public void OnRecipeSelected(Recipes recipe)
    {
        _selectedRecipe = recipe;
        if (detailsPanel != null)
            detailsPanel.SetActive(true);

        if (itemNameText != null) itemNameText.text = recipe.recipeName;
        if (itemDescriptionText != null) itemDescriptionText.text = recipe.description;

        UpdateIngredients(recipe);
        
        if (craftButton != null)
        {
            craftButton.interactable = RecipeDataBase.Instance.CanCraft(recipe);
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() => OnCraftButtonClicked());
        }
    }

    private void UpdateIngredients(Recipes recipe)
    {
        foreach (Transform child in ingredientsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var ingredient in recipe.resourcesNeeded)
        {
            GameObject item = Instantiate(ingredientItemPrefab, ingredientsContainer);
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                int currentQty = 0;
                var res = GameManager.Instance.resources.Find(r => r.resourceName == ingredient.resourceName);
                if (res != null) currentQty = res.quantity;
                
                text.text = $"{ingredient.resourceName}: {currentQty}/{ingredient.quantity}";
                text.color = (currentQty >= ingredient.quantity) ? Color.white : Color.red;
            }
        }
    }

    private void OnCraftButtonClicked()
    {
        if (_selectedRecipe != null && RecipeDataBase.Instance.CanCraft(_selectedRecipe))
        {
            RecipeDataBase.Instance.Craft(_selectedRecipe);
            OnRecipeSelected(_selectedRecipe);
        }
    }

    public void OnexitButtonClicked()
    {
        UIManager.Instance.SwitchState(UIManager.UIState.Gameplay);
    }
}