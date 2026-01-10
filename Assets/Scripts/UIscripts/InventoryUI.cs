using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The InventoryUI class is responsible for managing and displaying the user interface
/// components related to the inventory system within the application. It handles the
/// rendering, updating, and interaction logic for inventory-related UI elements.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    /// <summary>
    /// The singleton instance of the class, providing global access to its functionality.
    /// This property ensures that only one instance of the class exists throughout the application's lifecycle.
    /// </summary>
    public static InventoryUI Instance { get; private set; }

    /// Represents the user interface panel for displaying inventory items.
    [Header("Inventory Display")] public GameObject inventoryPanel;

    
    //The grid representing the layout structure of the inventory items in the UI.
    public Transform inventoryGrid;

    /// The prefab representing an individual inventory item within the inventory system.
    public GameObject inventoryItemPrefab;

    // A flag indicating whether the grid setup should be automatically handled.
    public bool autoSetupGrid = true;

    /// The panel responsible for displaying detailed information about a selected item.
    [Header("Item Detail Panel")] public GameObject itemDetailPanel;

    /// The text element displaying the name of the selected item in detail view.
    public TextMeshProUGUI detailItemNameText;

    /// The icon representing the visual depiction of a detailed item.
    public Image detailItemIcon;

    /// The text element displaying the quantity of a specific item in detail view.
    public TextMeshProUGUI detailItemQuantityText;

    /// The text element that displays the description of a selected item in detail view.
    public TextMeshProUGUI detailItemDescriptionText;

    /// A collection that holds the currently active items in the application or game.
    private Dictionary<string, InventoryItemUI> _activeItems = new Dictionary<string, InventoryItemUI>();

    /// A collection or pool of reusable item instances within the game.
    private Stack<InventoryItemUI> _itemPool = new Stack<InventoryItemUI>();

    /// A collection of keys that are pending processing by the system.
    private List<string> _keysToProcess = new List<string>();

    /// <summary>
    /// The coroutine responsible for handling update operations over a period of time.
    /// </summary>
    /// <remarks>
    /// This coroutine is used to execute tasks that require periodic or delayed
    /// updates, such as animations, behavioral changes, or timed events. It allows
    /// for asynchronous execution without blocking the main thread.
    /// </remarks>
    private Coroutine _updateCoroutine;

    /// Called when the script instance is being loaded.
    /// Initializes references and sets up any required data or state before the object becomes active.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        inventoryPanel.SetActive(false);
        itemDetailPanel.SetActive(false);
    }

    /// Initiates the process or logic associated with starting the specified operation.
    private void Start()
    {
        LayoutManager.SetupInventoryGrid(inventoryGrid, autoSetupGrid);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourcesChanged += UpdateInventoryDisplay;
            GameManager.Instance.OnResourceChanged += OnResourceChanged;
        }
    }

    /// Invoked when the object is destroyed. Handles cleanup operations or releasing resources
    /// associated with the object before it is removed from the game or application.
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourcesChanged -= UpdateInventoryDisplay;
            GameManager.Instance.OnResourceChanged -= OnResourceChanged;
        }
    }

    /// Handles the event triggered when a resource value changes.
    
    private void OnResourceChanged(string resourceName, int quantity)
    {
        if (!inventoryPanel.activeInHierarchy) return;
        UpdateSingleResourceDisplay(resourceName, quantity);
    }
   
    /// Toggles the state of the specified component or system between enabled and disabled.
    public void Toggle()
    {
        if (inventoryPanel == null) return;

        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            // When opening, hide other panels via UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnInventoryOpened();
            }
            
            // Force update when opening
            UpdateInventoryDisplay(true);
        }
        else
        {
            // When closing, restore panels via UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnInventoryClosed();
            }

            // Close details when inventory is closed
            itemDetailPanel.SetActive(false);
        }
    }

    /// Updates the display of a single resource in the user interface.
    
    private void UpdateSingleResourceDisplay(string resourceName, int quantity)
    {
        if (quantity <= 0)
        {
            if (_activeItems.TryGetValue(resourceName, out InventoryItemUI itemUI))
            {
                ReturnToPool(itemUI);
                _activeItems.Remove(resourceName);
                RebuildLayout();
            }
            return;
        }

        if (_activeItems.TryGetValue(resourceName, out InventoryItemUI existingItem))
        {
            ItemData data = GameManager.Instance.GetItemData(resourceName);
            existingItem.Setup(data, quantity);
        }
        else
        {
            // New item to show
            InventoryItemUI newItem = GetFromPool();
            ItemData data = GameManager.Instance.GetItemData(resourceName);
            newItem.Setup(data, quantity);
            _activeItems.Add(resourceName, newItem);
            RebuildLayout();
        }
    }
    
    /// Retrieves an available instance of the specified type from the object pool.
    private InventoryItemUI GetFromPool()
    {
        InventoryItemUI itemUI;
        if (_itemPool.Count > 0)
        {
            itemUI = _itemPool.Pop();
            itemUI.gameObject.SetActive(true);
        }
        else
        {
            GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryGrid);
            itemUI = itemObj.GetComponent<InventoryItemUI>();
        }
        
        itemUI.transform.SetParent(inventoryGrid, false);
        itemUI.transform.localScale = Vector3.one;
        
        // Ensure the item's pivot and anchors don't fight the layout group
        RectTransform itemRT = itemUI.GetComponent<RectTransform>();
        if (itemRT != null)
        {
            itemRT.pivot = new Vector2(0, 1);
            itemRT.anchorMin = new Vector2(0, 1);
            itemRT.anchorMax = new Vector2(0, 1);
        }

        return itemUI;
    }
    
    /// Returns the given object instance back to its respective pool for reuse.
    private void ReturnToPool(InventoryItemUI itemUI)
    {
        itemUI.gameObject.SetActive(false);
        _itemPool.Push(itemUI);
    }
    
    /// Rebuilds the layout of the user interface element to reflect any changes made to its structure or content.
    private void RebuildLayout()
    {
        Canvas.ForceUpdateCanvases();
        if (inventoryGrid is RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }


    // Updates the inventory display to reflect the current state of the player's inventory.
    public void UpdateInventoryDisplay() => UpdateInventoryDisplay(false);

    // Updates the inventory display to reflect the current state of the player's inventory.    
    public void UpdateInventoryDisplay(bool force)
    {
        if (!force && !inventoryPanel.activeInHierarchy) 
        {
            return;
        }

        if (_updateCoroutine != null) StopCoroutine(_updateCoroutine);
        _updateCoroutine = StartCoroutine(UpdateDisplayCoroutine());
    }
    
    // Updates the user interface display over time as part of a coroutine.
    private System.Collections.IEnumerator UpdateDisplayCoroutine()
    {
        // Cache current resources to avoid modification during iteration
        var currentResources = GameManager.Instance != null ? new List<ResourceData>(GameManager.Instance.resources) : new List<ResourceData>();
        
        // Track which items are still valid
        HashSet<string> validResources = new HashSet<string>();

        foreach (var res in currentResources)
        {
            if (res.quantity > 0)
            {
                validResources.Add(res.resourceName);
                UpdateSingleResourceDisplay(res.resourceName, res.quantity);
                yield return null; // Spread over frames
            }
        }

        // Clean up items that are no longer in resources or have 0 quantity
        _keysToProcess.Clear();
        _keysToProcess.AddRange(_activeItems.Keys);
        
        foreach (var key in _keysToProcess)
        {
            if (!validResources.Contains(key))
            {
                ReturnToPool(_activeItems[key]);
                _activeItems.Remove(key);
            }
        }

        RebuildLayout();
        _updateCoroutine = null;
    }
    
    // Displays the details of a specified item to the user.
    public void ShowItemDetails(ItemData data, int quantity)
    {
        itemDetailPanel.SetActive(true);
        detailItemNameText.text = data.itemName;
        detailItemIcon.sprite = data.icon; 
        detailItemIcon.enabled = true;
        detailItemQuantityText.text = $"Quantity: {quantity}";
        detailItemDescriptionText.text = data.description;
    }
}
