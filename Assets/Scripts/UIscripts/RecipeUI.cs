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
    public Image itemImage;
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
        
        ConfigureVerticalLayout(ingredientsContainer);
        ConfigureGridLayout(recipeContainer, 2);

        PopulateRecipeList();
    }

    private void ConfigureVerticalLayout(Transform container)
    {
        if (container == null) return;

        VerticalLayoutGroup vlg = container.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = container.gameObject.AddComponent<VerticalLayoutGroup>();

        vlg.spacing = 10;
        vlg.padding = new RectOffset(25, 5, 15, 5);
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;

        ContentSizeFitter csf = container.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = container.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private void ConfigureGridLayout(Transform container, int columns)
    {
        if (container == null) return;
        
        VerticalLayoutGroup vlg = container.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) DestroyImmediate(vlg);

        GridLayoutGroup glg = container.GetComponent<GridLayoutGroup>();
        if (glg == null) glg = container.gameObject.AddComponent<GridLayoutGroup>();

        glg.spacing = new Vector2(20, 20);
        glg.padding = new RectOffset(20, 5, 20, 5);
        
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = columns;
        
        if (recipeItemPrefab != null)
        {
            RectTransform prefabRect = recipeItemPrefab.GetComponent<RectTransform>();
            if (prefabRect != null)
            {
                glg.cellSize = prefabRect.sizeDelta;
            }
        }

        ContentSizeFitter csf = container.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = container.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
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
                
                Image icon = null;
                TextMeshProUGUI nameText = null;
                
                Transform iconTransform = item.transform.Find("icon");
                if (iconTransform == null) iconTransform = item.transform.Find("itemicon");
                if (iconTransform != null) icon = iconTransform.GetComponent<Image>();
                
                Transform nameTransform = item.transform.Find("name");
                if (nameTransform == null) nameTransform = item.transform.Find("recipename");
                if (nameTransform != null) nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                
                if (icon == null)
                {
                    Image[] images = item.GetComponentsInChildren<Image>(true);
                    foreach (var img in images)
                    {
                        if (img.gameObject != item)
                        {
                            icon = img;
                            break;
                        }
                    }
                }
                
                if (nameText == null)
                {
                    nameText = item.GetComponentInChildren<TextMeshProUGUI>();
                }
                
                if (nameText != null)
                {
                    nameText.text = recipe.recipeName;
                }
                
                if (icon != null)
                {
                    Sprite sprite = null;
                    if (!string.IsNullOrEmpty(recipe.outputItem))
                    {
                        var data = GameManager.Instance != null ? GameManager.Instance.GetItemData(recipe.outputItem) : null;
                        if (data != null)
                            sprite = data.icon;
                    }
                    icon.sprite = sprite;
                    icon.enabled = sprite != null;
                }
                
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
        
        if (itemImage != null)
        {
            Sprite sprite = null;
            if (!string.IsNullOrEmpty(recipe.outputItem))
            {
                var data = GameManager.Instance != null ? GameManager.Instance.GetItemData(recipe.outputItem) : null;
                if (data != null)
                    sprite = data.icon;
            }
            itemImage.sprite = sprite;
            itemImage.enabled = sprite != null;
        }
        
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
            
            Image icon = null;
            Transform iconTransform = item.transform.Find("IngredientImage");
            if (iconTransform != null)
            {
                icon = iconTransform.GetComponent<Image>();
            }
            else
            {
                Image[] images = item.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img.gameObject != item)
                    {
                        icon = img;
                        break;
                    }
                }
            }

            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI nameText = null;
            TextMeshProUGUI qtyText = null;
            
            Transform nameTransform = item.transform.Find("ingredientname");
            if (nameTransform != null) nameText = nameTransform.GetComponent<TextMeshProUGUI>();
            Transform countTransform = item.transform.Find("count");
            if (countTransform != null) qtyText = countTransform.GetComponent<TextMeshProUGUI>();
            
            if (nameText == null && texts != null && texts.Length > 0) nameText = texts[0];
            if (qtyText == null && texts != null && texts.Length > 1) qtyText = texts[1];
            
            if (nameText != null)
            {
                nameText.text = ingredient.resourceName;
            }
            
            if (icon != null)
            {
                Sprite sprite = null;
                var data = GameManager.Instance != null ? GameManager.Instance.GetItemData(ingredient.resourceName) : null;
                if (data != null)
                    sprite = data.icon;
                icon.sprite = sprite;
                icon.enabled = sprite != null;
            }
            
            if (qtyText != null)
            {
                int currentQty = 0;
                var res = GameManager.Instance.resources.Find(r => r.resourceName == ingredient.resourceName);
                if (res != null) currentQty = res.quantity;
                
                qtyText.text = $"{ingredient.quantity}/{currentQty}";
                bool hasEnough = currentQty >= ingredient.quantity;
                qtyText.color = hasEnough ? Color.green : Color.red;
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