using Cinemachine;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [Header("References")]
    public CinemachineVirtualCamera vcam;
    public Transform focussedTarget;
    public Transform defaultTarget;

    [Header("Selection")]
    public string characterTag = "Character"; // tag used to mark selectable characters

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float edgeSize = 20f; // pixels from edge before moving
    public bool allowEdgeScroll = true;
    public float defaultZOffset = -10f;
    public float dragSensitivity = 0.01f;
    public float resetSmoothTime = 0.5f; // time it takes to reset when pressing space

    [Header("Zoom Settings")]
    public float zoomSpeed = 20f;
    public float minZOffset = -25f;
    public float maxZOffset = -5f;

    [Header("Map Bounds (world space)")]
    public Rect mapBounds = new Rect(-50, -10, 100, 20); // x, y, width, height
    public bool drawBoundsGizmo = true;
    public Color boundsGizmoColor = Color.green;

    private CinemachineTransposer transposer;
    private Vector3 manualOffset;
    private bool isManual = false;

    private Vector3 lastMousePos;
    private bool isDragging = false;
    private Vector3 resetVelocity;
    private bool isResetting = false;

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

        // Ensure the camera starts at the correct Z distance
        Vector3 startOffset = transposer.m_FollowOffset;
        startOffset.z = defaultZOffset;
        transposer.m_FollowOffset = startOffset;
        manualOffset = startOffset;
    }

    private void Update()
    {

        SelectCharacter();
        HandleDrag();
        HandleEdgeScroll();
        HandleReset();
        HandleZoom();

        // If player starts moving while resetting, cancel the reset.
        if (isResetting && (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f))
        {
            isResetting = false;
        }

        if (isResetting)
            SmoothResetMotion();
    }

    private void SelectCharacter()
    {
        // Left click to select character
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // Try 3D physics
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                if (IsCharacter(hit.collider.gameObject))
                {
                    focussedTarget = hit.collider.transform;
                    isManual = false; // hand control back to follow
                    return;
                }
            }

            // If no 3D hit, try 2D physics (Not sure if we are going to use 2d colliders or not)

            //Vector3 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            //Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);
            //RaycastHit2D hit2D = Physics2D.Raycast(worldPoint2D, Vector2.zero);
            //if (hit2D.collider != null)
            //{
            //    if (IsCharacter(hit2D.collider.gameObject))
            //    {
            //        focussedTarget = hit2D.collider.transform;
            //        vcam.Follow = focussedTarget;
            //        isManual = false;
            //        return;
            //    }
            //}
        }

        // Escape to reset focus to default target
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            focussedTarget = null;
            //vcam.Follow = defaultTarget;
            isManual = true;
        }
    }

    // Simple check: object is a character if it has the configured tag
    private bool IsCharacter(GameObject go)
    {
        if (go == null) return false;

        if (!string.IsNullOrEmpty(characterTag))
        {
            // Safe tag check; user must ensure tag exists
            if (go.CompareTag(characterTag)) return true;
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

        // Edge detection
        if (mousePos.x <= edgeSize) move.x = -1;
        else if (mousePos.x >= screenWidth - edgeSize) move.x = 1;

        if (mousePos.y <= edgeSize) move.y = -1;
        else if (mousePos.y >= screenHeight - edgeSize) move.y = 1;

        // WASD movement
        move.x += Input.GetAxisRaw("Horizontal");
        move.y += Input.GetAxisRaw("Vertical");

        if (move.sqrMagnitude > 0.01f)
        {
            // Cancel any ongoing reset when user provides movement input
            if (isResetting) isResetting = false;

            isManual = true;
            // move in world units relative to follow target (we treat manualOffset as offset from follow/world origin)
            manualOffset += move.normalized * moveSpeed * Time.deltaTime;

            // Keep Z offset (respect current zoom) and clamp it
            manualOffset.z = Mathf.Clamp(manualOffset.z, minZOffset, maxZOffset);

            ApplyBoundsAndPushToTransposer();
        }
    }

    private void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Don't start reset if the user is dragging or has active WASD input
            if (isDragging ||
                Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f ||
                Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f)
            {
                // ignore reset request
                return;
            }

            if(focussedTarget == null)
            {
                // If no focussed target, reset to default target
                vcam.Follow = defaultTarget;
            }
            else
            {
                vcam.Follow = focussedTarget;
            }

            isManual = false;
            isResetting = true;
            resetVelocity = Vector3.zero;
        }
    }

    private void SmoothResetMotion()
    {
        Vector3 targetOffset = new Vector3(0, 0, defaultZOffset);

        // Smoothly move offset back
        manualOffset = Vector3.SmoothDamp(
            manualOffset,
            targetOffset,
            ref resetVelocity,
            resetSmoothTime
        );

        transposer.m_FollowOffset = manualOffset;

        // stop when close enough
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
            // starting a drag cancels any reset in progress
            if (isResetting) isResetting = false;

            isDragging = true;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePos;

            Vector3 dragMove = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * dragSensitivity;

            // Cancel reset if user actively drags
            if (isResetting) isResetting = false;

            isManual = true;
            manualOffset += dragMove;

            // Keep Z offset (respect current zoom) and clamp it
            manualOffset.z = Mathf.Clamp(manualOffset.z, minZOffset, maxZOffset);

            ApplyBoundsAndPushToTransposer();

            lastMousePos = Input.mousePosition;
        }
    }

    private void HandleZoom()
    {
        if (transposer == null)
            return;

        // Mouse wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Q/E key input: E = zoom in (closer), Q = zoom out (farther)
        float keyInput = 0f;
        if (Input.GetKey(KeyCode.E)) keyInput += 1f;
        if (Input.GetKey(KeyCode.Q)) keyInput -= 1f;

        // Compute requested new Z
        float deltaZ = scroll * zoomSpeed + keyInput * zoomSpeed * Time.deltaTime;
        if (Mathf.Abs(deltaZ) < 0.00001f)
            return; // no zoom input

        float newZ = Mathf.Clamp(manualOffset.z + deltaZ, minZOffset, maxZOffset);

        // If no actual change, nothing to do.
        if (Mathf.Approximately(newZ, manualOffset.z))
            return;

        // Cancel reset when zooming
        if (isResetting) isResetting = false;

        Camera cam = Camera.main;
        if (cam == null)
        {
            // Fallback: just change z if no main camera available
            manualOffset.z = newZ;
            isManual = true;
            ApplyBoundsAndPushToTransposer();
            return;
        }

        // Compute plane Z to keep stable under cursor. Use follow's Z if available, otherwise 0.
        float planeZ = (vcam != null && vcam.Follow != null) ? vcam.Follow.position.z : 0f;

        // Ray from current camera through mouse
        Ray rayBefore = cam.ScreenPointToRay(Input.mousePosition);

        // If ray direction nearly parallel with plane, skip focusing and just change z
        if (Mathf.Abs(rayBefore.direction.z) < 1e-5f)
        {
            manualOffset.z = newZ;
            isManual = true;
            ApplyBoundsAndPushToTransposer();
            return;
        }

        // Intersection with the plane (world point under cursor before zoom)
        float tBefore = (planeZ - rayBefore.origin.z) / rayBefore.direction.z;
        Vector3 worldBefore = rayBefore.GetPoint(tBefore);

        // Compute where camera WOULD be after changing z (world space).
        Vector3 camPosBefore = cam.transform.position;
        // Correct Z change only
        Vector3 camPosAfter = camPosBefore + new Vector3(0f, 0f, 0f) + ((new Vector3(0, 0, newZ) - new Vector3(0, 0, manualOffset.z)));

        Vector3 followPos = (vcam != null && vcam.Follow != null) ? vcam.Follow.position : Vector3.zero;
        Vector3 camPosBeforeExpected = followPos + manualOffset;
        Vector3 camPosAfterExpected = followPos + new Vector3(manualOffset.x, manualOffset.y, newZ);

        // Use expected positions if they differ significantly from current camera transform
        if ((camPosBeforeExpected - camPosBefore).sqrMagnitude < 0.01f)
            camPosBefore = camPosBeforeExpected;
        camPosAfter = camPosAfterExpected;

        // Ray from hypothetical new camera position with same direction (camera rotation unchanged by our zoom)
        Ray rayAfter = new Ray(camPosAfter, rayBefore.direction);

        // If rayAfter.direction.z is nearly zero, skip focus
        if (Mathf.Abs(rayAfter.direction.z) < 1e-5f)
        {
            manualOffset.z = newZ;
            isManual = true;
            ApplyBoundsAndPushToTransposer();
            return;
        }

        float tAfter = (planeZ - rayAfter.origin.z) / rayAfter.direction.z;
        Vector3 worldAfter = rayAfter.GetPoint(tAfter);

        // Delta to apply so the world point under cursor stays fixed
        Vector3 deltaWorld = worldBefore - worldAfter;

        manualOffset.x += deltaWorld.x;
        manualOffset.y += deltaWorld.y;
        manualOffset.z = newZ;

        isManual = true;
        ApplyBoundsAndPushToTransposer();
    }

    // Ensures manualOffset respects mapBounds (for x/y) and updates the transposer.
    private void ApplyBoundsAndPushToTransposer()
    {
        Vector3 followPos = (vcam != null && vcam.Follow != null) ? vcam.Follow.position : Vector3.zero;
        Vector3 worldPos = (vcam != null && vcam.Follow != null) ? followPos + manualOffset : manualOffset;

        // Clamp X/Y to mapBounds
        float clampedX = Mathf.Clamp(worldPos.x, mapBounds.xMin, mapBounds.xMax);
        float clampedY = Mathf.Clamp(worldPos.y, mapBounds.yMin, mapBounds.yMax);

        Vector3 clampedWorldPos = new Vector3(clampedX, clampedY, worldPos.z);

        // Recompute manualOffset relative to follow (or as world pos if no follow)
        if (vcam != null && vcam.Follow != null)
            manualOffset = clampedWorldPos - followPos;
        else
            manualOffset = clampedWorldPos;

        // Ensure Z is clamped
        manualOffset.z = Mathf.Clamp(manualOffset.z, minZOffset, maxZOffset);

        transposer.m_FollowOffset = manualOffset;
    }

    private void OnDrawGizmos()
    {
        if (!drawBoundsGizmo)
            return;

        Gizmos.color = boundsGizmoColor;

        // Draw rectangle in world XY plane at Z = 0
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
}
