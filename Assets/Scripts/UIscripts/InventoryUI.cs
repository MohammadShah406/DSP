using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Inventory Display")]
    public GameObject inventoryPanel;
    public Transform inventoryGrid;
    public GameObject inventoryItemPrefab;
    public bool autoSetupGrid = true;

    [Header("Item Detail Panel")]
    public GameObject itemDetailPanel;
    public TextMeshProUGUI detailItemNameText;
    public Image detailItemIcon;
    public TextMeshProUGUI detailItemQuantityText;
    public TextMeshProUGUI detailItemDescriptionText;

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
        SetupInventoryGrid();
        
        GameManager.Instance.OnResourcesChanged += UpdateInventoryDisplay;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnResourcesChanged -= UpdateInventoryDisplay;
    }

    public void Toggle()
    {
        if (inventoryPanel == null) return;

        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            // When opening, hide other panels via UIManager
            UIManager.Instance.OnInventoryOpened();
            
            // Force update when opening
            UpdateInventoryDisplay(true);
        }
        else
        {
            // When closing, restore panels via UIManager
            UIManager.Instance.OnInventoryClosed();

            // Close details when inventory is closed
            itemDetailPanel.SetActive(false);
        }
    }

    public void SetupInventoryGrid()
    {
        if (inventoryGrid == null || !autoSetupGrid) return;

        RectTransform gridRT = inventoryGrid as RectTransform;
        if (gridRT != null)
        {
            gridRT.anchorMin = new Vector2(0, 1);
            gridRT.anchorMax = new Vector2(0, 1);
            gridRT.pivot = new Vector2(0, 1);
            gridRT.anchoredPosition = Vector2.zero;
            gridRT.sizeDelta = Vector2.zero;
        }

        // We no longer modify GridLayoutGroup properties (CellSize, Spacing, Padding).
        // You now have full control over these in the Unity Inspector.

        ContentSizeFitter fitter = inventoryGrid.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = inventoryGrid.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.enabled = true;
    }

    public void UpdateInventoryDisplay() => UpdateInventoryDisplay(false);

    public void UpdateInventoryDisplay(bool force)
    {
        if (!force && !inventoryPanel.activeInHierarchy) return;

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in inventoryGrid) children.Add(child.gameObject);
        
        foreach (GameObject child in children)
        {
            child.transform.SetParent(null);
            Destroy(child);
        }

        foreach (var res in GameManager.Instance.resources)
        {
            if (res.quantity <= 0) continue;

            GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryGrid);
            itemObj.transform.SetParent(inventoryGrid, false);
            itemObj.transform.localPosition = Vector3.zero;
            itemObj.transform.localScale = Vector3.one;
            itemObj.transform.localRotation = Quaternion.identity;
            
            // Ensure the item's pivot and anchors don't fight the layout group
            RectTransform itemRT = itemObj.GetComponent<RectTransform>();
            if (itemRT != null)
            {
                itemRT.pivot = new Vector2(0, 1);
                itemRT.anchorMin = new Vector2(0, 1);
                itemRT.anchorMax = new Vector2(0, 1);
                itemRT.localScale = Vector3.one;
            }

            itemObj.layer = inventoryGrid.gameObject.layer;
            foreach (Transform t in itemObj.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = itemObj.layer;

            InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
            ItemData data = GameManager.Instance.GetItemData(res.resourceName);

            itemUI.Setup(data, res.quantity);
        }

        Canvas.ForceUpdateCanvases();
        if (inventoryGrid is RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

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
