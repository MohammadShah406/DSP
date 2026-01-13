using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject taskEntryPrefab;
    [SerializeField] private Transform taskListContainer;
    [SerializeField] private float completionDisplayTime = 2f;

    public static bool IsMouseOver { get; private set; }

    private RectTransform _rectTransform;
    private Dictionary<TaskInstance, GameObject> _activeTaskEntries = new Dictionary<TaskInstance, GameObject>();

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        Debug.Log("[TaskUI] Start called.");
        
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
            Debug.Log("[TaskUI] Subscribing to TaskManager.OnTasksUpdated.");
            TaskManager.Instance.OnTasksUpdated += OnTasksUpdated;
        }
        else
        {
            Debug.LogError("[TaskUI] TaskManager.Instance is null in Start!");
        }

        RefreshTaskList();
    }

    private void Update()
    {
        UpdateMouseOver();
    }

    private void UpdateMouseOver()
    {
        if (_rectTransform == null) return;

        // Check if mouse is within the bounds of this panel
        IsMouseOver = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition, null);
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
            TaskManager.Instance.OnTasksUpdated -= OnTasksUpdated;
        }
    }

    private void SetTextValue(Graphic textGraphic, string value)
    {
        if (textGraphic == null) return;
        if (textGraphic is Text t) t.text = value;
        else if (textGraphic is TextMeshProUGUI tmp) tmp.text = value;
    }

    private void ApplyStrikethrough(Graphic textGraphic)
    {
        if (textGraphic == null) return;
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

    private void OnTasksUpdated()
    {
        if (TaskManager.Instance == null) return;
        
        List<TaskInstance> currentTasks = TaskManager.Instance.GetActiveTasks();
        Debug.Log($"[TaskUI] OnTasksUpdated received {currentTasks.Count} active tasks.");
        
        // Check for newly completed tasks
        foreach (var kvp in new Dictionary<TaskInstance, GameObject>(_activeTaskEntries))
        {
            TaskInstance taskInstance = kvp.Key;
            GameObject entry = kvp.Value;
            
            // If task is now completed, show completion state
            if (taskInstance.isCompleted && entry != null)
            {
                UpdateTaskEntryToCompleted(entry, taskInstance);
                StartCoroutine(RemoveTaskEntryAfterDelay(taskInstance, entry));
            }
        }
        
        // Add new tasks that appeared
        foreach (var taskInstance in currentTasks)
        {
            if (!_activeTaskEntries.ContainsKey(taskInstance))
            {
                Debug.Log($"[TaskUI] Creating entry for task: {taskInstance.taskData.taskDescription}");
                CreateTaskEntry(taskInstance);
            }
        }
    }
    
    private void CreateTaskEntry(TaskInstance task)
    {
        if (taskEntryPrefab == null)
        {
            Debug.LogError("[TaskUI] Task Entry Prefab is missing!");
            return;
        }

        GameObject entry = Instantiate(taskEntryPrefab, taskListContainer);
        _activeTaskEntries[task] = entry; // Track this entry
        
        entry.SetActive(true);
        entry.transform.localPosition = Vector3.zero;
        entry.transform.localScale = Vector3.one;

        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer == -1) uiLayer = 5;
        
        entry.layer = uiLayer;
        foreach (Transform child in entry.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = uiLayer;
        }
        
        RectTransform entryRect = entry.GetComponent<RectTransform>();
        if (entryRect != null)
        {
            entryRect.anchorMin = new Vector2(0.5f, 1f);
            entryRect.anchorMax = new Vector2(0.5f, 1f);
            entryRect.pivot = new Vector2(0.5f, 1f);
            entryRect.anchoredPosition3D = Vector3.zero;
            entryRect.sizeDelta = new Vector2(428f, 80f);
            entryRect.localScale = Vector3.one;
            entryRect.localRotation = Quaternion.identity;
        }

        Canvas nestedCanvas = entry.GetComponent<Canvas>();
        if (nestedCanvas != null)
        {
            Destroy(nestedCanvas);
            CanvasScaler ns = entry.GetComponent<CanvasScaler>();
            if (ns != null) Destroy(ns);
            GraphicRaycaster gr = entry.GetComponent<GraphicRaycaster>();
            if (gr != null) Destroy(gr);
        }

        Graphic[] graphics = entry.GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
        {
            g.gameObject.SetActive(true);
            Color c = g.color;
            if (c.a < 0.01f) 
            {
                string n = g.gameObject.name.ToLower();
                if (g is TMPro.TextMeshProUGUI || n.Contains("label") || n.Contains("desc") || 
                    n.Contains("toggle") || n.Contains("background") || n.Contains("checkmark"))
                {
                    c.a = 1f;
                    g.color = c;
                }
            }
            g.SetAllDirty();
        }
        
        CanvasGroup cg = entry.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        LayoutElement le = entry.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = entry.AddComponent<LayoutElement>();
        }

        le.minHeight = 80f;
        le.preferredHeight = 80f;
        le.minWidth = 428f;
        le.preferredWidth = 428f;

        Toggle toggle = entry.GetComponentInChildren<Toggle>();
        
        List<Graphic> textComponents = new List<Graphic>();
        textComponents.AddRange(entry.GetComponentsInChildren<TextMeshProUGUI>(true));
        textComponents.AddRange(entry.GetComponentsInChildren<Text>(true));
        
        if (toggle != null)
        {
            toggle.isOn = false; // Always start unchecked
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
            if (lowerName.Contains("time") || lowerName.Contains("title") || lowerName.Contains("label")) 
                timeText = txt;
            else if (lowerName.Contains("description") || lowerName.Contains("desc") || lowerName.Contains("task")) 
                descText = txt;
        }

        if (timeText == null && textComponents.Count > 0) timeText = textComponents[0];
        if (descText == null && textComponents.Count > 1) descText = textComponents[1];
        if (descText == null && timeText != null) descText = timeText;

        SetTextValue(timeText, $"{task.taskData.hour:00}:{task.taskData.minute:00}");
        if (timeText != null) timeText.raycastTarget = false;

        string displayName = task.taskData.taskDescription;
        if (task.taskData.requiredCharacter != TaskData.CharacterName.None)
        {
            displayName = $"{task.taskData.requiredCharacter}: {displayName}";
        }
        SetTextValue(descText, displayName);
        if (descText != null) descText.raycastTarget = false;

        entry.transform.SetAsLastSibling();
    }
    
    private void UpdateTaskEntryToCompleted(GameObject entry, TaskInstance taskInstance)
    {
        if (entry == null) return;

        Debug.Log($"[TaskUI] Marking task as completed: {taskInstance.taskData.taskDescription}");

        Image backgroundImage = entry.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.5f, 1f, 0.5f, 0.3f);
        }
        
        // Toggle the checkmark
        Toggle toggle = entry.GetComponentInChildren<Toggle>();
        if (toggle != null)
        {
            toggle.isOn = true;
        }

        // Apply strikethrough to text
        List<Graphic> textComponents = new List<Graphic>();
        textComponents.AddRange(entry.GetComponentsInChildren<TextMeshProUGUI>(true));
        textComponents.AddRange(entry.GetComponentsInChildren<Text>(true));

        foreach (var txt in textComponents)
        {
            ApplyStrikethrough(txt);
        }

        Canvas.ForceUpdateCanvases();
    }

    private IEnumerator RemoveTaskEntryAfterDelay(TaskInstance taskInstance, GameObject entry)
    {
        yield return new WaitForSeconds(completionDisplayTime);

        // Entry may have been destroyed by RefreshTaskList or another update
        if (entry == null)
        {
            _activeTaskEntries.Remove(taskInstance);
            yield break;
        }

        CanvasGroup canvasGroup = entry.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = entry.AddComponent<CanvasGroup>();

        float fadeTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            if (entry == null) yield break; // destroyed mid-fade

            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }

        if (entry != null) Destroy(entry);

        _activeTaskEntries.Remove(taskInstance);

        Canvas.ForceUpdateCanvases();
        if (taskListContainer != null)
        {
            RectTransform containerRT = taskListContainer.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
        }
    }

    private void RefreshTaskList()
    {
        // Clear everything
        foreach (Transform child in taskListContainer)
        {
            Destroy(child.gameObject);
        }
        _activeTaskEntries.Clear();

        if (TaskManager.Instance == null) return;

        List<TaskInstance> activeTasks = TaskManager.Instance.GetActiveTasks();

        foreach (var task in activeTasks)
        {
            if (!task.isCompleted)
            {
                CreateTaskEntry(task);
            }
        }

        Canvas.ForceUpdateCanvases();
        if (taskListContainer != null)
        {
            RectTransform containerRT = taskListContainer.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRT);
        }
    }
}
