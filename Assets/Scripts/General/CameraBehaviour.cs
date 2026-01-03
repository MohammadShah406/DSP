using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CameraBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _mainCamera;
    public CinemachineVirtualCamera vcam;
    public Transform focussedTarget;
    public Transform defaultTarget;

    [Header("Selection")]
    public string characterTag = "Character";
    [Tooltip("Tags considered valid movement surfaces when clicking (only X used).")]
    public string[] movementSurfaceTags = { "Ground", "Wall" };

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float edgeSize = 20f;
    public bool allowEdgeScroll = true;
    public float defaultZOffset = -10f;
    public float dragSensitivity = 0.01f;
    public float resetSmoothTime = 0.5f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 20f;
    public float minZOffset = -25f;
    public float maxZOffset = -5f;

    [Header("Map Bounds (world space)")]
    public Rect mapBounds = new Rect(-50, -10, 100, 20);
    public bool drawBoundsGizmo = true;
    public Color boundsGizmoColor = Color.green;

    [Header("Rotation Lock")]
    [Tooltip("If true the virtual camera never rotates (agent / target rotation ignored).")]
    public bool lockCameraRotation = true;

    [Header("Click Settings")]
    [Tooltip("Max time between two LMB clicks to count as a double-click (run / follow).")]
    public float doubleClickTime = 0.3f;
    [Tooltip("Max cursor movement (in pixels) between clicks to still count as a double-click.")]
    public float doubleClickMaxPixels = 12f;

    private CinemachineTransposer transposer;
    private Vector3 manualOffset;
    private bool isManual = false;

    private Vector3 lastMousePos;
    private bool isDragging = false;
    private Vector3 resetVelocity;
    private bool isResetting = false;
    private bool resetZoom = false;

    // Highlighting state
    private Transform lastHighlightedTarget;
    private readonly Dictionary<Renderer, Color[]> _originalColors = new Dictionary<Renderer, Color[]>();

    // Cached initial rotation
    private Quaternion lockedRotation;

    // Double-click tracking
    private float _lastLmbTime = -1f;
    private Vector2 _lastLmbScreenPos;
    private Vector3 _lastMoveClickPos;
    private float _lastMoveClickTime = -1f;
    private Transform _lastClickedCharacter;
    private Interactable _lastClickedInteractable;

    // Follow state (decoupled from manual pan state)
    private bool isFollowing = false;

    public static CameraBehaviour Instance { get; private set; }

    private void Awake()
    {
        _mainCamera = Camera.main;

        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    private void Start()
    {
        if (vcam == null)
            vcam = GetComponent<CinemachineVirtualCamera>();

        transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer == null)
        {
            Debug.LogError("CameraBehaviour: CinemachineTransposer not found. Set Body to 'Framing Transposer'.");
            enabled = false;
            return;
        }

        if (defaultTarget != null)
            vcam.Follow = defaultTarget;

        Vector3 startOffset = transposer.m_FollowOffset;
        startOffset.z = defaultZOffset;
        transposer.m_FollowOffset = startOffset;
        manualOffset = startOffset;

        // Ensure binding mode does not inherit target rotation (prevents camera yaw when agent rotates)
        transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;

        // Cache initial rotation for locking
        if (vcam != null)
            lockedRotation = vcam.transform.rotation;
    }

    private void Update()
    {
        FollowOrNot();
        HandleClick();
        HighlightSelectedTarget();
        HandleDrag();
        HandleEdgeScroll();
        HandleZoom();
        
        if (InputManager.Instance.NextCharacterInput)
            HandleCharacterScrollSelection(1);

        else if (InputManager.Instance.PreviousCharacterInput)
            HandleCharacterScrollSelection(-1);

        


        if (isResetting && (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f))
            isResetting = false;

        if (isResetting)
            SmoothResetMotion();
    }

    private void LateUpdate()
    {
        // After Cinemachine pipeline runs, force rotation back
        if (lockCameraRotation && vcam != null)
            vcam.transform.rotation = lockedRotation;
    }

    private void HandleClick()
    {
        // Block interaction if mouse is over UI
        if (IsMouseOverUI())
        {
            return;
        }

        if (InputManager.Instance.SelectInput)
        {
            bool isDoubleClick = false;
            float dt = Time.time - _lastLmbTime;
            float pixelDist = (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _lastLmbScreenPos).magnitude;

            _lastLmbTime = Time.time;
            _lastLmbScreenPos = Input.mousePosition;

            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 2000f);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            Transform clickedCharacter = null;
            Interactable clickedInteractable = null;
            RaycastHit? movementSurfaceHit = null;

            foreach (var hit in hits)
            {
                GameObject clicked = hit.collider.gameObject;

                Debug.Log($"Clicked on: {clicked.name} at distance {hit.distance}");

                // Priority 1: Interactable
                if (focussedTarget != null && InteractionManager.Instance != null)
                {
                    if (InteractionManager.Instance.IsInteractableForCharacter(clicked.transform, focussedTarget))
                    {
                        clickedInteractable = clicked.GetComponent<Interactable>();
                        break;
                    }
                }

                // Priority 2: Interactable
                if (IsCharacter(clicked))
                {
                    clickedCharacter = hit.collider.transform;
                    break; 
                }

                // Priority 3: Movement surface

                if (IsMovementSurface(clicked))
                {
                    movementSurfaceHit = hit;
                    // keep searching in case a nearer character exists; break later if none
                }
            }

            // Determine interactable double click
            bool interactableDoubleClick = false;
            if (clickedInteractable != null &&
                _lastClickedInteractable == clickedInteractable &&
                dt >= 0f && dt <= doubleClickTime &&
                pixelDist <= doubleClickMaxPixels)
            {
                interactableDoubleClick = true;
            }
            _lastClickedInteractable = clickedInteractable;

            // Determine character double click
            if (clickedCharacter != null &&
                _lastClickedCharacter == clickedCharacter &&
                dt >= 0f && dt <= doubleClickTime &&
                pixelDist <= doubleClickMaxPixels)
            {
                isDoubleClick = true;
            }

            // Determine movement surface double click
            bool moveDoubleClick = false;
            if (movementSurfaceHit.HasValue)
            {
                Vector3 clickedPos = movementSurfaceHit.Value.point;
                float dtMove = Time.time - _lastMoveClickTime;
                float dist = Vector3.Distance(clickedPos, _lastMoveClickPos);

                if (dtMove <= doubleClickTime && dist <= 0.5f) // small threshold in world units
                    moveDoubleClick = true;

                _lastMoveClickTime = Time.time;
                _lastMoveClickPos = clickedPos;
            }


            // Handle interactable click first
            if (clickedInteractable != null)
            {
                InteractionManager.Instance.TryInteract(focussedTarget, runFlag: interactableDoubleClick, clickedInteractable);
                return;
            }

            //Handle character click
            if (clickedCharacter != null)
            {
                SetFocussed(clickedCharacter.gameObject);
                _lastClickedCharacter = clickedCharacter;

                

                if (isDoubleClick)
                {
                    StartFollowing(resetZoom: false);
                }
                else
                {
                    // Single-click: select only, do NOT follow yet.
                    StopFollowing(keepSelection: true);
                }

                
                return;
            }

            // Handle movement surface click
            if (focussedTarget != null && movementSurfaceHit.HasValue)
            {
                var mover = focussedTarget.GetComponent<CharacterMovement>();
                if (mover != null)
                {
                    mover.SetTarget(movementSurfaceHit.Value.point, runFlag: moveDoubleClick);
                }

                return;
            }

            _lastClickedCharacter = null;
        }

        if (InputManager.Instance.ResetInput)
        {
            if (focussedTarget != null)
                StartFollowing(resetZoom: true);
        }

        if (InputManager.Instance.DeselectInput)
        {
            DeslectCharacter();
        }
    }

    private void DeslectCharacter()
    {
        ResetFocussed();
        StopFollowing(keepSelection: false);
        isManual = true;
        ActivateReset();
        manualOffset = new Vector3(0, 0, manualOffset.z);
        transposer.m_FollowOffset = manualOffset;
    }


    private void StartFollowing(bool resetZoom)
    {
        if (focussedTarget == null)
            return;

        this.resetZoom = resetZoom;
        isFollowing = true;
        isManual = false;
        ActivateReset();
        RecenterInstantly();
    }

    private void StopFollowing(bool keepSelection)
    {
        isFollowing = false;
        if (!keepSelection)
            ResetFocussed();
    }

    private void MoveDefaultTarget()
    {
        if (focussedTarget == null || isManual)
        {
            if (_mainCamera != null && defaultTarget != null)
            {
                Vector3 camForward = _mainCamera.transform.forward;
                Vector3 camPos = _mainCamera.transform.position;
                float planeZ = defaultTarget.position.z;
                float t = Mathf.Abs(camForward.z) > 1e-5f ? (planeZ - camPos.z) / camForward.z : 0f;
                Vector3 worldPos = camPos + camForward * t;
                defaultTarget.position = worldPos;
            }
        }
        else
        {
            defaultTarget.position = focussedTarget.position;
        }
    }

    private bool IsCharacter(GameObject go)
    {
        return go != null && !string.IsNullOrEmpty(characterTag) && go.CompareTag(characterTag);
    }

    private bool IsMovementSurface(GameObject go)
    {
        if (go == null || movementSurfaceTags == null) return false;
        foreach (var tag in movementSurfaceTags)
        {
            if (!string.IsNullOrEmpty(tag) && go.CompareTag(tag))
                return true;
        }
        return false;
    }

    private void HandleEdgeScroll()
    {
        if (isDragging || !allowEdgeScroll || transposer == null)
            return;

        Vector3 move = Vector3.zero;
        Vector2 mousePos = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (mousePos.x <= edgeSize) move.x = -1;
        else if (mousePos.x >= screenWidth - edgeSize) move.x = 1;

        if (mousePos.y <= edgeSize) move.y = -1;
        else if (mousePos.y >= screenHeight - edgeSize) move.y = 1;

        move.x += Input.GetAxisRaw("Horizontal");
        move.y += Input.GetAxisRaw("Vertical");

        if (move.sqrMagnitude > 0.01f)
        {
            if (isResetting) isResetting = false;
            isManual = true;
            manualOffset += move.normalized * moveSpeed * Time.deltaTime;
            manualOffset.z = Mathf.Clamp(manualOffset.z, minZOffset, maxZOffset);
            ApplyBoundsAndPushToTransposer();
        }
    }

    private void ActivateReset()
    {
        

        if (!isFollowing || focussedTarget == null)
        {
            MoveDefaultTarget();
            vcam.Follow = defaultTarget;
        }
        else
        {
            vcam.Follow = focussedTarget;
        }

        isResetting = true;
        resetVelocity = Vector3.zero;
    }

    private void SmoothResetMotion()
    {
        Vector3 targetOffset;
        if (resetZoom)
        {
            resetZoom = false;
            targetOffset = new Vector3(0, 0, defaultZOffset);
        }
        else
        {
            targetOffset = new Vector3(0, 0, manualOffset.z);
        }

        manualOffset = Vector3.SmoothDamp(
            manualOffset,
            targetOffset,
            ref resetVelocity,
            resetSmoothTime
        );

        transposer.m_FollowOffset = manualOffset;

        if (Vector3.Distance(manualOffset, targetOffset) < 0.01f)
        {
            manualOffset = targetOffset;
            transposer.m_FollowOffset = manualOffset;
            isResetting = false;
        }
    }

    private void HandleDrag()
    {
        if (InputManager.Instance.DragInput)
        {
            if(isManual == false)
            {
                DeslectCharacter();
            }
            isDragging = true;
        }
        else
        {
            isDragging = false;
        }

        if (isDragging)
        {

            Vector2 delta = InputManager.Instance.DragDeltaInput;
            Vector3 dragMove = new Vector3(-delta.x, -delta.y, 0) * dragSensitivity;

            manualOffset += dragMove;
            manualOffset.z = Mathf.Clamp(manualOffset.z, minZOffset, maxZOffset);
            ApplyBoundsAndPushToTransposer();
        }
    }


    private void HandleZoom()
    {
        if (transposer == null)
            return;

        // Combine scroll wheel and keys
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float keyInput = InputManager.Instance.ZoomInput;

        // Block scroll zoom if mouse is over UI
        if (IsMouseOverUI())
        {
            scroll = 0f;
        }

        float deltaZ = (scroll + keyInput * Time.deltaTime) * zoomSpeed;

        if (Mathf.Abs(deltaZ) < 0.0001f)
            return;

        if (isResetting) isResetting = false;

        // Adjust zoom normally along the Z offset
        manualOffset.z = Mathf.Clamp(manualOffset.z + deltaZ, minZOffset, maxZOffset);

        // Only mark manual if not following a character
        if (!isFollowing)
            isManual = true;

        transposer.m_FollowOffset = manualOffset;
    }

    private bool IsMouseOverUI()
    {
        if (EventSystem.current == null) return false;

        // Check if mouse is over ANY UI object
        if (!EventSystem.current.IsPointerOverGameObject()) return false;

        // If it is, check if it's an interactive element or a blocked area
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == null) continue;

            // 1. Check for specific interactive components
            if (result.gameObject.GetComponent<Button>() != null ||
                result.gameObject.GetComponent<Slider>() != null ||
                result.gameObject.GetComponent<Scrollbar>() != null ||
                result.gameObject.GetComponent<TMP_InputField>() != null)
            {
                return true;
            }

            // 2. Check for TaskUI items specifically
            if (result.gameObject.name.Contains("TaskEntry") || 
                result.gameObject.name.Contains("Handle"))
            {
                return true;
            }

            // 3. If it's part of a known menu that should block (Pause, Inventory, Stats, HUD, Tasks)
            if (UIManager.Instance != null)
            {
                if (IsChildOf(result.gameObject.transform, UIManager.Instance.pausePanel) ||
                    IsChildOf(result.gameObject.transform, UIManager.Instance.inventoryPanel) ||
                    IsChildOf(result.gameObject.transform, UIManager.Instance.statsPanel) ||
                    IsChildOf(result.gameObject.transform, UIManager.Instance.topStatsHUD) ||
                    IsChildOf(result.gameObject.transform, UIManager.Instance.taskPanel))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsChildOf(Transform child, GameObject parent)
    {
        if (parent == null) return false;
        return child.IsChildOf(parent.transform);
    }

    private void ApplyBoundsAndPushToTransposer()
    {
        Vector3 followPos = (vcam != null && vcam.Follow != null) ? vcam.Follow.position : Vector3.zero;
        Vector3 worldPos = (vcam != null && vcam.Follow != null) ? followPos + manualOffset : manualOffset;

        float clampedX = Mathf.Clamp(worldPos.x, mapBounds.xMin, mapBounds.xMax);
        float clampedY = Mathf.Clamp(worldPos.y, mapBounds.yMin, mapBounds.yMax);
        Vector3 clampedWorldPos = new Vector3(clampedX, clampedY, worldPos.z);

        if (vcam != null && vcam.Follow != null)
            manualOffset = clampedWorldPos - followPos;
        else
            manualOffset = clampedWorldPos;

        manualOffset.z = Mathf.Clamp(manualOffset.z, minZOffset, maxZOffset);
        transposer.m_FollowOffset = manualOffset;
    }

    private void OnDrawGizmos()
    {
        if (!drawBoundsGizmo) return;

        Gizmos.color = boundsGizmoColor;
        Vector3 bl = new Vector3(mapBounds.xMin, mapBounds.yMin, 0f);
        Vector3 br = new Vector3(mapBounds.xMax, mapBounds.yMin, 0f);
        Vector3 tl = new Vector3(mapBounds.xMin, mapBounds.yMax, 0f);
        Vector3 tr = new Vector3(mapBounds.xMax, mapBounds.yMax, 0f);

        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);

        if (transposer != null)
        {
            Vector3 followPos = (vcam != null && vcam.Follow != null) ? vcam.Follow.position : Vector3.zero;
            Vector3 camWorldPos = followPos + manualOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(camWorldPos.x, camWorldPos.y, 0f), 0.5f);
        }
    }

    public void HighlightSelectedTarget()
    {
        // If we had a highlighted target and focus cleared or changed, restore its colors
        if (lastHighlightedTarget != null && (focussedTarget == null || lastHighlightedTarget != focussedTarget))
        {
            RestoreColors(lastHighlightedTarget);
            lastHighlightedTarget = null;
        }

        // Nothing to highlight
        if (focussedTarget == null)
            return;

        // Already highlighted
        if (lastHighlightedTarget == focussedTarget)
            return;

        // Apply green highlight to new focussed target
        //ApplyGreen(focussedTarget);
        //lastHighlightedTarget = focussedTarget;
    }

    private void ApplyGreen(Transform target)
    {
        if (target == null) return;

        var renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            // Cache original colors once per renderer
            if (!_originalColors.ContainsKey(r))
            {
                var mats = r.materials; 
                var colors = new Color[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    colors[i] = (m != null && m.HasProperty("_Color")) ? m.color : Color.white;
                }
                _originalColors[r] = colors;
            }

            var highlightMats = r.materials;
            for (int i = 0; i < highlightMats.Length; i++)
            {
                var m = highlightMats[i];
                if (m != null && m.HasProperty("_Color"))
                    m.color = Color.green;
            }
        }
    }

    private void RestoreColors(Transform target)
    {
        if (target == null) return;

        var renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (_originalColors.TryGetValue(r, out var colors))
            {
                var mats = r.materials;
                int count = Mathf.Min(mats.Length, colors.Length);
                for (int i = 0; i < count; i++)
                {
                    var m = mats[i];
                    if (m != null && m.HasProperty("_Color"))
                        m.color = colors[i];
                }

                // Cleanup cached entry for this renderer
                _originalColors.Remove(r);
            }
        }
    }

    private void FollowOrNot()
    {
        if (isFollowing && focussedTarget != null)
            vcam.Follow = focussedTarget;
        else
            vcam.Follow = defaultTarget;
    }

    private void SetFocussed(GameObject go)
    {
        if (go == null) return;

        if (focussedTarget != null && focussedTarget != go.transform)
        {
            var prevOc = focussedTarget.GetComponent<OutlineController>();
            if (prevOc != null) prevOc.SetSelected(false);
        }

        focussedTarget = go.transform;

        var oc = focussedTarget.GetComponent<OutlineController>();
        if (oc != null) oc.SetSelected(true);
    }

    private void ResetFocussed()
    {
        if (focussedTarget != null)
        {
            var oc = focussedTarget.GetComponent<OutlineController>();
            if (oc != null) oc.SetSelected(false);
        }
        focussedTarget = null;
        _lastClickedCharacter = null;
    }

    private void HandleCharacterScrollSelection(int next)
    {
        if (GameManager.Instance == null || GameManager.Instance.characters.Count == 0)
            return;

        var characters = GameManager.Instance.characters;
        if (characters.Count == 0) return;

        // Determine current index
        int currentIndex = -1;
        if (focussedTarget != null)
        {
            currentIndex = characters.FindIndex(c => c != null && c.transform == focussedTarget);
            
        }

        int nextIndex;

        if (currentIndex == -1)
        {
            // If nothing is focused, start from first character
            nextIndex = 0;
        }
        else
        {
            nextIndex = (currentIndex + next) % characters.Count;
            if (nextIndex < 0)
                nextIndex += characters.Count; // wrap properly
        }

        // Select the next character
        GameObject nextCharacter = characters[nextIndex];
        if (nextCharacter == null)
            return;

       

        SetFocussed(nextCharacter);
        isManual = false;
        
    }

    private void RecenterInstantly()
    {
        if (vcam == null || transposer == null)
            return;

        Vector3 targetPos = (focussedTarget != null) ? focussedTarget.position : defaultTarget.position;
        Vector3 followOffset = new Vector3(0, 0, manualOffset.z);

        // Align instantly
        manualOffset = followOffset;
        transposer.m_FollowOffset = followOffset;
        vcam.Follow = focussedTarget != null ? focussedTarget : defaultTarget;

        // Ensure reset/transition flags are cleared
        isResetting = false;
    }
}
