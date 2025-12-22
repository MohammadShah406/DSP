using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject taskEntryPrefab;
    [SerializeField] private Transform taskListContainer;

    public static bool IsMouseOver { get; private set; }

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Ensure EventSystem exists
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[TaskUI] No EventSystem found in scene. Creating one.");
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // Ensure GraphicRaycaster exists on parent Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
        {
            Debug.LogWarning($"[TaskUI] No GraphicRaycaster found on Canvas '{canvas.name}'. Adding one.");
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        // Ensure we can receive pointer events
        Image img = GetComponent<Image>();
        if (img == null)
        {
            img = gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // Transparent
        }
        img.raycastTarget = true;

        SetupContainer();

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTasksUpdated += RefreshTaskList;
        }

        if (TimeManager.Instance != null)
        {
            // Update when time changes to catch new tasks
            TimeManager.Instance.HourChanged += (h, m, d) => 
            {
                RefreshTaskList();
            };
        }

        RefreshTaskList();
    }

    private void Update()
    {
        UpdateMouseOver();
    }

    private void UpdateMouseOver()
    {
        if (rectTransform == null) return;

        // Check if mouse is within the bounds of this panel
        IsMouseOver = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Keep for compatibility but UpdateMouseOver is more reliable
        IsMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // UpdateMouseOver will handle the false state
    }

    private void OnDisable()
    {
        IsMouseOver = false;
    }

    private void SetupContainer()
    {
        // If we already have a ScrollRect, we just need to ensure its settings are correct
        ScrollRect existingScrollRect = GetComponent<ScrollRect>();
        if (existingScrollRect != null)
        {
            existingScrollRect.horizontal = false;
            existingScrollRect.vertical = true;
            existingScrollRect.scrollSensitivity = 100f; // Increased sensitivity
            
            // Link viewport and content if they are assigned or findable
            if (existingScrollRect.viewport == null)
            {
                Transform vp = transform.Find("Viewport");
                if (vp != null) existingScrollRect.viewport = vp.GetComponent<RectTransform>();
            }

            if (existingScrollRect.content == null && taskListContainer != null)
            {
                existingScrollRect.content = taskListContainer.GetComponent<RectTransform>();
            }
            else if (existingScrollRect.content != null)
            {
                taskListContainer = existingScrollRect.content;
            }

            // Ensure viewport has raycast target to catch scrolling
            if (existingScrollRect.viewport != null)
            {
                Image vpImg = existingScrollRect.viewport.GetComponent<Image>();
                if (vpImg == null) vpImg = existingScrollRect.viewport.gameObject.AddComponent<Image>();
                vpImg.color = new Color(0, 0, 0, 0.03f); // Increased alpha for Mask reliability (approx 8/255)
                vpImg.raycastTarget = true;
            }

            // Find and link vertical scrollbar if missing
            if (existingScrollRect.verticalScrollbar == null)
            {
                Scrollbar sb = GetComponentInChildren<Scrollbar>(true);
                if (sb != null) 
                {
                    existingScrollRect.verticalScrollbar = sb;
                    Debug.Log($"[TaskUI] Linked scrollbar to existing ScrollRect: {sb.name}");
                }
            }

            if (existingScrollRect.verticalScrollbar != null)
            {
                Scrollbar sb = existingScrollRect.verticalScrollbar;
                // Ensure scrollbar is interactive
                sb.interactable = true;
                Image[] sbImgs = sb.GetComponentsInChildren<Image>();
                foreach (var simg in sbImgs) simg.raycastTarget = true;
            }
            
            if (taskListContainer != null)
            {
                VerticalLayoutGroup vlg = taskListContainer.GetComponent<VerticalLayoutGroup>();
                if (vlg == null) vlg = taskListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                
                vlg.spacing = 5;
                vlg.padding = new RectOffset(10, 10, 10, 10);
                vlg.childControlHeight = true;
                vlg.childControlWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = false;
                vlg.childAlignment = TextAnchor.UpperCenter;
                
                ContentSizeFitter csf = taskListContainer.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = taskListContainer.gameObject.AddComponent<ContentSizeFitter>();
                
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

                // Ensure content and viewport scales are 1
                taskListContainer.localScale = Vector3.one;
                if (existingScrollRect.viewport != null) existingScrollRect.viewport.localScale = Vector3.one;
            }
            return;
        }

        if (taskListContainer == null) return;

        // 1. Setup ScrollRect on the main TaskUI panel
        ScrollRect scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = gameObject.AddComponent<ScrollRect>();
        }

        // Configuration for the scroll panel
        scrollRect.horizontal = false; 
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 100f; 
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent; // Avoid issues with missing scrollbar
        scrollRect.inertia = true; 
        scrollRect.decelerationRate = 0.135f;

        // Find and link a vertical scrollbar if it exists in the hierarchy
        if (scrollRect.verticalScrollbar == null)
        {
            Scrollbar sb = GetComponentInChildren<Scrollbar>(true);
            if (sb != null)
            {
                scrollRect.verticalScrollbar = sb;
                sb.interactable = true;
                Image[] sbImgs = sb.GetComponentsInChildren<Image>();
                foreach (var simg in sbImgs) simg.raycastTarget = true;
            }
        }

        // 2. Setup a Viewport if it doesn't exist
        Transform viewport = transform.Find("Viewport");
        if (viewport == null)
        {
            GameObject vpObj = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            vpObj.transform.SetParent(transform, false);
            viewport = vpObj.transform;
            
            RectTransform vpRect = vpObj.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.sizeDelta = Vector2.zero;
            vpRect.pivot = new Vector2(0.5f, 0.5f); // Center pivot for viewport is safer
            vpRect.localScale = Vector3.one; 
            
            Image vpImg = vpObj.GetComponent<Image>();
            vpImg.color = new Color(0, 0, 0, 0.03f); // Increased alpha for Mask reliability (approx 8/255)
            vpImg.raycastTarget = true; 
        }
        else
        {
            // Use standard Mask instead of RectMask2D for better compatibility if issues occur
            if (viewport.GetComponent<Mask>() == null)
            {
                RectMask2D rm = viewport.GetComponent<RectMask2D>();
                if (rm != null) Destroy(rm);
                
                viewport.gameObject.AddComponent<Mask>();
                Image vpImg = viewport.GetComponent<Image>();
                if (vpImg == null) vpImg = viewport.gameObject.AddComponent<Image>();
                vpImg.color = new Color(0, 0, 0, 0.03f); // Increased alpha for Mask reliability (approx 8/255)
                vpImg.raycastTarget = true;
            }
            viewport.localScale = Vector3.one; 
        }

        scrollRect.viewport = viewport.GetComponent<RectTransform>();

        // 3. Ensure the taskListContainer is inside the Viewport and configured correctly
        if (taskListContainer.parent != viewport)
        {
            taskListContainer.SetParent(viewport, false);
        }

        scrollRect.content = taskListContainer.GetComponent<RectTransform>();

        // 4. Setup Vertical Layout Group on the container
        VerticalLayoutGroup layout = taskListContainer.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = taskListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.spacing = 5;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = true; 
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.childAlignment = TextAnchor.UpperCenter;

        // Force positions to be managed by a layout group
        layout.enabled = false;
        layout.enabled = true;

        // 5. Setup Content Size Fitter
        ContentSizeFitter fitter = taskListContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = taskListContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // 6. Anchor the container to the TOP so it grows downward
            RectTransform containerRect = taskListContainer.GetComponent<RectTransform>();
            containerRect.localScale = Vector3.one; // Ensure scale is 1
            containerRect.anchorMin = new Vector2(0f, 1f); // Top-Left
            containerRect.anchorMax = new Vector2(1f, 1f); // Top-Right
            containerRect.pivot = new Vector2(0.5f, 1f);   // Pivot at top-center
            
            // Ensure width is stretched to viewport (sizeDelta.x = 0)
            containerRect.sizeDelta = new Vector2(0f, containerRect.sizeDelta.y);
            containerRect.anchoredPosition3D = Vector3.zero; // Force Z to 0 and position to 0

            // Important: Force the VerticalLayoutGroup to control the child sizes
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.childAlignment = TextAnchor.UpperCenter;

        // 7. Ensure the TaskUI panel itself has a transparent image to catch scroll events
        Image mainImg = GetComponent<Image>();
        if (mainImg == null)
        {
            mainImg = gameObject.AddComponent<Image>();
            mainImg.color = new Color(0, 0, 0, 0);
        }
        mainImg.raycastTarget = true;
    }

    private void OnDestroy()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTasksUpdated -= RefreshTaskList;
        }
    }

    private void SetTextValue(Graphic textGraphic, string value)
    {
        if (textGraphic is Text t) t.text = value;
        else if (textGraphic is TextMeshProUGUI tmp) tmp.text = value;
    }

    private void ApplyStrikethrough(Graphic textGraphic)
    {
        if (textGraphic is Text t)
        {
            t.color = Color.gray;
            // Standard UI Text doesn't have a native strikethrough property without a custom shader
        }
        else if (textGraphic is TextMeshProUGUI tmp)
        {
            tmp.color = Color.gray;
            tmp.fontStyle = FontStyles.Strikethrough;
        }
    }

    public void RefreshTaskList()
    {
        if (taskListContainer == null) return;

        // Clear existing entries
        foreach (Transform child in taskListContainer)
        {
            Destroy(child.gameObject);
        }

        if (TaskManager.Instance == null) return;

        List<TaskData> activeTasks = TaskManager.Instance.GetActiveTasks();

        foreach (var task in activeTasks)
        {
            if (taskEntryPrefab == null)
            {
                Debug.LogError("[TaskUI] Task Entry Prefab is missing!");
                break;
            }
            GameObject entry = Instantiate(taskEntryPrefab, taskListContainer);
            
            // Fix: Re-parenting or instantiation can sometimes reset local position or scale
            // which causes visibility issues or overlapping.
            entry.SetActive(true);
            entry.transform.localPosition = Vector3.zero;
            entry.transform.localScale = Vector3.one;

            // Ensure it's on the UI layer (usually layer 5)
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer == -1) uiLayer = 5; // Fallback
            
            entry.layer = uiLayer;
            foreach (Transform child in entry.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = uiLayer;
            }
            
            // Set Z to 0 to avoid being behind the canvas or viewport
            RectTransform entryRect = entry.GetComponent<RectTransform>();
            if (entryRect != null)
            {
                // Force anchors to match layout expectation
                entryRect.anchorMin = new Vector2(0.5f, 1f);
                entryRect.anchorMax = new Vector2(0.5f, 1f);
                entryRect.pivot = new Vector2(0.5f, 1f);
                
                entryRect.anchoredPosition3D = Vector3.zero;
                entryRect.sizeDelta = new Vector2(428f, 80f);
                
                // Ensure scale is forced on RectTransform as well
                entryRect.localScale = Vector3.one;
                
                // Ensure it's not rotated
                entryRect.localRotation = Quaternion.identity;
            }

            // Fix: Nested Canvases can break sorting and visibility in ScrollRects
            Canvas nestedCanvas = entry.GetComponent<Canvas>();
            if (nestedCanvas != null)
            {
                // If it was meant to override sorting, it might be behind the main HUD
                Destroy(nestedCanvas);
                CanvasScaler ns = entry.GetComponent<CanvasScaler>();
                if (ns != null) Destroy(ns);
                GraphicRaycaster gr = entry.GetComponent<GraphicRaycaster>();
                if (gr != null) Destroy(gr);
            }

            // Ensure all CanvasRenderers are active and color alpha is 1 where appropriate
            Graphic[] graphics = entry.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                g.gameObject.SetActive(true);
                // Force alpha to 1 if it's near zero, just in case
                Color c = g.color;
                if (c.a < 0.01f) 
                {
                    // If it's a text or has a non-transparent name, force alpha
                    string n = g.gameObject.name.ToLower();
                    if (g is TMPro.TextMeshProUGUI || n.Contains("label") || n.Contains("desc") || n.Contains("toggle") || n.Contains("background") || n.Contains("checkmark"))
                    {
                        c.a = 1f;
                        g.color = c;
                    }
                }
                // Wake up the graphic
                g.SetAllDirty();
            }
            
            // Check for CanvasGroup
            CanvasGroup cg = entry.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;

            // Ensure the entry has a LayoutElement to play nice with the VerticalLayoutGroup
            LayoutElement le = entry.GetComponent<LayoutElement>();
            if (le == null)
            {
                le = entry.AddComponent<LayoutElement>();
            }

            // Set the dimensions based on the user's specific prefab size
            le.minHeight = 80f;
            le.preferredHeight = 80f;
            le.minWidth = 428f;
            le.preferredWidth = 428f;

            // Find components in the prefab
            Toggle toggle = entry.GetComponentInChildren<Toggle>();
            
            // Search for both TMP and standard Text components
            List<Graphic> textComponents = new List<Graphic>();
            textComponents.AddRange(entry.GetComponentsInChildren<TextMeshProUGUI>(true));
            textComponents.AddRange(entry.GetComponentsInChildren<Text>(true));
            
            if (toggle != null)
            {
                toggle.isOn = task.isCompleted;
                toggle.interactable = false;
                Image[] toggleImgs = toggle.GetComponentsInChildren<Image>();
                foreach(var timg in toggleImgs) timg.raycastTarget = false;
            }

            Image[] entryImgs = entry.GetComponentsInChildren<Image>();
            foreach(var eimg in entryImgs)
            {
                if (eimg.gameObject == entry) eimg.raycastTarget = true;
                else eimg.raycastTarget = false;
            }

            HorizontalLayoutGroup entryLayout = entry.GetComponent<HorizontalLayoutGroup>();
            if (entryLayout != null)
            {
                entryLayout.childControlWidth = true;
                entryLayout.childControlHeight = true;
                entryLayout.childForceExpandWidth = false;
                entryLayout.childForceExpandHeight = false;
                entryLayout.spacing = 10;
                entryLayout.padding = new RectOffset(5, 5, 5, 5);
            }

            Graphic timeText = null;
            Graphic descText = null;
            foreach (var txt in textComponents)
            {
                string lowerName = txt.gameObject.name.ToLower();
                if (lowerName.Contains("time") || lowerName.Contains("title") || lowerName.Contains("label")) timeText = txt;
                else if (lowerName.Contains("description") || lowerName.Contains("desc") || lowerName.Contains("task")) descText = txt;
            }

            // Fallback to index if naming doesn't help
            if (timeText == null && textComponents.Count > 0) timeText = textComponents[0];
            if (descText == null && textComponents.Count > 1) descText = textComponents[1];
            if (descText == null && timeText != null) descText = timeText; // Use same for both if only one exists

            if (timeText != null)
            {
                SetTextValue(timeText, $"{task.hour:00}:{task.minute:00}");
                timeText.raycastTarget = false;
                if (task.isCompleted) ApplyStrikethrough(timeText);
            }

            if (descText != null)
            {
                SetTextValue(descText, task.taskDescription);
                descText.raycastTarget = false;
                if (task.isCompleted) ApplyStrikethrough(descText);
            }

            // Ensure the entry is visible above other UI siblings in the container
            entry.transform.SetAsLastSibling();
        }

        // Force layout update to ensure ScrollRect knows the new content size
        Canvas.ForceUpdateCanvases();
        if (taskListContainer != null)
        {
            RectTransform containerRT = taskListContainer.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
        }
    }
}
