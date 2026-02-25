using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScavengerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI headingText;
    public RectTransform listContainer;
    public GameObject listItemPrefab;

    public void Setup(List<ResourceData> items, bool isDonation)
    {
        if (headingText != null)
        {
            headingText.text = isDonation ? "Items Donated" : "Items Scavenged";
        }

        ConfigureLayout();
        PopulateList(items);
        
        gameObject.SetActive(true);
        
        // Force layout update next frame
        Canvas.ForceUpdateCanvases();
        if (listContainer != null)
        {
            var vlg = listContainer.GetComponent<VerticalLayoutGroup>();
            if (vlg != null) vlg.enabled = false;
            if (vlg != null) vlg.enabled = true;
        }
    }

    private void ConfigureLayout()
    {
        if (listContainer == null) return;

        VerticalLayoutGroup vlg = listContainer.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = listContainer.gameObject.AddComponent<VerticalLayoutGroup>();

        vlg.spacing = 10;
        vlg.padding = new RectOffset(10, 10, 50, 10); // Added 50 padding at the top
        vlg.childControlHeight = false; // User requested not to edit height
        vlg.childControlWidth = false;  // User requested not to edit width
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter csf = listContainer.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = listContainer.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        
        listContainer.localScale = Vector3.one;
        listContainer.gameObject.SetActive(true);
        
        CanvasGroup cg = listContainer.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;
    }

    private void PopulateList(List<ResourceData> items)
    {
        if (listContainer == null || listItemPrefab == null) return;

        // Clear existing items
        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var itemData in items)
        {
            if (itemData == null) continue;
            
            GameObject itemObj = Instantiate(listItemPrefab, listContainer);
            
            RectTransform itemRT = itemObj.GetComponent<RectTransform>();
            if (itemRT != null)
            {
                itemRT.localScale = Vector3.one;
            }

            // Find components: Image for icon, TextMeshProUGUI for name and quantity
            Image icon = null;
            TextMeshProUGUI nameText = null;
            TextMeshProUGUI qtyText = null;

            // Try finding by common names first
            Transform iconTransform = itemObj.transform.Find("icon");
            if (iconTransform == null) iconTransform = itemObj.transform.Find("itemimage");
            if (iconTransform == null) iconTransform = itemObj.transform.Find("ingredientimage");
            if (iconTransform != null) icon = iconTransform.GetComponent<Image>();

            Transform nameTransform = itemObj.transform.Find("name");
            if (nameTransform == null) nameTransform = itemObj.transform.Find("itemname");
            if (nameTransform == null) nameTransform = itemObj.transform.Find("ingredientname");
            if (nameTransform != null) nameText = nameTransform.GetComponent<TextMeshProUGUI>();

            Transform qtyTransform = itemObj.transform.Find("quantity");
            if (qtyTransform == null) qtyTransform = itemObj.transform.Find("count");
            if (qtyTransform != null) qtyText = qtyTransform.GetComponent<TextMeshProUGUI>();

            // Fallbacks if not found by name
            if (icon == null)
            {
                // Find any Image that is NOT on the root panel
                Image[] images = itemObj.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img.gameObject != itemObj)
                    {
                        icon = img;
                        break;
                    }
                }
            }
            
            TextMeshProUGUI[] texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (nameText == null && texts.Length > 0) nameText = texts[0];
            if (qtyText == null && texts.Length > 1) qtyText = texts[1];

            // Set Data
            if (nameText != null) nameText.text = itemData.resourceName;
            if (qtyText != null) qtyText.text = itemData.quantity.ToString();

            if (icon != null)
            {
                Sprite sprite = null;
                var dbData = GameManager.Instance != null ? GameManager.Instance.GetItemData(itemData.resourceName) : null;
                if (dbData != null)
                    sprite = dbData.icon;
                
                icon.sprite = sprite;
                icon.enabled = sprite != null;
            }
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
