using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class CameraBehaviour : MonoBehaviour
{
    [Header("References")]
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


        SelectCharacter();
        HighlightSelectedTarget();
        HandleDrag();
        HandleEdgeScroll();
        HandleReset();
        HandleZoom();

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

    private void SelectCharacter()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 2000f);
            Debug.DrawRay(ray.origin, ray.direction * 50f, Color.red, 4f);

            foreach (var hit in hits)
            {
                Debug.Log($"Hit: {hit.collider.gameObject.name} at {hit.point}");
                GameObject clicked = hit.collider.gameObject;

                if (IsCharacter(clicked))
                {
                    focussedTarget = hit.collider.transform;
                    vcam.Follow = focussedTarget;
                    ActivateReset();
                    isManual = false;
                    return; // Stop after first character hit
                }

                if (focussedTarget != null && IsMovementSurface(clicked))
                {
                    var mover = focussedTarget.GetComponent<CharacterMovement>();
                    if (mover != null)
                    {
                        mover.SetTarget(hit.point, runFlag: Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
                    }
                    return;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (focussedTarget != null)
            {
                var mover = focussedTarget.GetComponent<CharacterMovement>();
                if (mover != null) mover.ClearTarget();
            }

            MoveDefaultTarget();
            vcam.Follow = defaultTarget;
            
            
            focussedTarget = null;
            isManual = true;


            manualOffset = new Vector3(0, 0, manualOffset.z);
            transposer.m_FollowOffset = manualOffset;
        }
    }

    private void MoveDefaultTarget()
    {
        defaultTarget.position = focussedTarget.position;
    }

    private bool IsCharacter(GameObject go)
    {
        if (go == null) return false;
        if (!string.IsNullOrEmpty(characterTag) && go.CompareTag(characterTag))
            return true;
        return false;
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

    private void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            resetZoom = true;
            ActivateReset();
        }
    }

    private void ActivateReset()
    {
        if (isDragging ||
                Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f ||
                Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f)
            return;

        if (focussedTarget == null)
            vcam.Follow = defaultTarget;
        else
            vcam.Follow = focussedTarget;

        isManual = false;
        isResetting = true;
        resetVelocity = Vector3.zero;
    }

    private void SmoothResetMotion()
    {
        Vector3 targetOffset = new Vector3(0, 0, 0);
        if (resetZoom == true)
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
        if (Input.GetMouseButtonDown(1))
        {
            if (isResetting) isResetting = false;
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
            isDragging = false;

        if (isDragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePos;
            Vector3 dragMove = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * dragSensitivity;

            if (isResetting) isResetting = false;

            isManual = true;
            manualOffset += dragMove;
            manualOffset.z = Mathf.Clamp(manualOffset.z, minZOffset, maxZOffset);
            ApplyBoundsAndPushToTransposer();

            lastMousePos = Input.mousePosition;
        }
    }

    private void HandleZoom()
    {
        if (transposer == null)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float keyInput = 0f;
        if (Input.GetKey(KeyCode.E)) keyInput += 1f;
        if (Input.GetKey(KeyCode.Q)) keyInput -= 1f;

        float deltaZ = scroll * zoomSpeed + keyInput * zoomSpeed * Time.deltaTime;
        if (Mathf.Abs(deltaZ) < 0.00001f)
            return;

        float newZ = Mathf.Clamp(manualOffset.z + deltaZ, minZOffset, maxZOffset);
        if (Mathf.Approximately(newZ, manualOffset.z))
            return;

        if (isResetting) isResetting = false;

        Camera cam = Camera.main;
        if (cam == null)
        {
            manualOffset.z = newZ;
            isManual = true;
            ApplyBoundsAndPushToTransposer();
            return;
        }

        float planeZ = (vcam != null && vcam.Follow != null) ? vcam.Follow.position.z : 0f;
        Ray rayBefore = cam.ScreenPointToRay(Input.mousePosition);

        if (Mathf.Abs(rayBefore.direction.z) < 1e-5f)
        {
            manualOffset.z = newZ;
            isManual = true;
            ApplyBoundsAndPushToTransposer();
            return;
        }

        float tBefore = (planeZ - rayBefore.origin.z) / rayBefore.direction.z;
        Vector3 worldBefore = rayBefore.GetPoint(tBefore);

        Vector3 camPosBefore = cam.transform.position;
        Vector3 followPos = (vcam != null && vcam.Follow != null) ? vcam.Follow.position : Vector3.zero;
        Vector3 camPosBeforeExpected = followPos + manualOffset;
        Vector3 camPosAfterExpected = followPos + new Vector3(manualOffset.x, manualOffset.y, newZ);
        if ((camPosBeforeExpected - camPosBefore).sqrMagnitude < 0.01f)
            camPosBefore = camPosBeforeExpected;
        Vector3 camPosAfter = camPosAfterExpected;

        Ray rayAfter = new Ray(camPosAfter, rayBefore.direction);
        if (Mathf.Abs(rayAfter.direction.z) < 1e-5f)
        {
            manualOffset.z = newZ;
            isManual = true;
            ApplyBoundsAndPushToTransposer();
            return;
        }

        float tAfter = (planeZ - rayAfter.origin.z) / rayAfter.direction.z;
        Vector3 worldAfter = rayAfter.GetPoint(tAfter);
        Vector3 deltaWorld = worldBefore - worldAfter;

        manualOffset.x += deltaWorld.x;
        manualOffset.y += deltaWorld.y;
        manualOffset.z = newZ;

        isManual = true;
        ApplyBoundsAndPushToTransposer();
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
        ApplyGreen(focussedTarget);
        lastHighlightedTarget = focussedTarget;
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
                var mats = r.materials; // uses instantiated material instances for safe color edits
                var colors = new Color[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    colors[i] = (m != null && m.HasProperty("_Color")) ? m.color : Color.white;
                }
                _originalColors[r] = colors;
            }

            // Set to green
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
}
