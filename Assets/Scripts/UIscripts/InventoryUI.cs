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
    
    public static InventoryUI Instance { get; private set; }
    
    [Header("Inventory Display")] public GameObject inventoryPanel;
    public Transform inventoryGrid;
    public GameObject inventoryItemPrefab;
    public bool autoSetupGrid = true;
    
    [Header("Item Detail Panel")] public GameObject itemDetailPanel;
    public TextMeshProUGUI detailItemNameText;
    public Image detailItemIcon;
    public TextMeshProUGUI detailItemQuantityText;
    public TextMeshProUGUI detailItemDescriptionText;
    public Button PlaceButton;
    private Dictionary<string, InventoryItemUI> _activeItems = new Dictionary<string, InventoryItemUI>();
    private Stack<InventoryItemUI> _itemPool = new Stack<InventoryItemUI>();
    private List<string> _keysToProcess = new List<string>();
    private Coroutine _updateCoroutine;
    
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
    
    private void Start()
    {
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourcesChanged += UpdateInventoryDisplay;
            GameManager.Instance.OnResourceChanged += OnResourceChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourcesChanged -= UpdateInventoryDisplay;
            GameManager.Instance.OnResourceChanged -= OnResourceChanged;
        }
    }
    
    
    private void OnResourceChanged(string resourceName, int quantity)
    {
        if (!inventoryPanel.activeInHierarchy) return;
        UpdateSingleResourceDisplay(resourceName, quantity);
    }
    
    public void Toggle()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.CurrentState == UIManager.UIState.Inventory)
                UIManager.Instance.OnInventoryClosed();
            else
                UIManager.Instance.OnInventoryOpened();
        }
    }
    
    public void OnOpened()
    {
        UpdateInventoryDisplay(true);
    }
    
    public void OnClosed()
    {
        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(false);
    }
    
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
    
    private void ReturnToPool(InventoryItemUI itemUI)
    {
        itemUI.gameObject.SetActive(false);
        _itemPool.Push(itemUI);
    }
    
    private void RebuildLayout()
    {
        Canvas.ForceUpdateCanvases();
        if (inventoryGrid is RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    
    public void UpdateInventoryDisplay() => UpdateInventoryDisplay(false);

    // Updates the inventory display to reflect the current state of the player's inventory.    
    public void UpdateInventoryDisplay(bool force)
    {
        if (!force && !inventoryPanel.activeInHierarchy) 
        {
            return;
        }

        // Coroutines cannot be started on inactive GameObjects.
        if (!gameObject.activeInHierarchy) return;

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
        PlaceButton.gameObject.SetActive(data.itemType == ItemType.Placement);
    }
}
