using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class CharacterMovement : MonoBehaviour
{
    [Header("Mode")]
    [Tooltip("If true uses NavMeshAgent for pathfinding. If false falls back to simple X-axis Rigidbody movement.")]
    [SerializeField] private bool useNavMeshAgent = true;

    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    [Header("Arrival")]
    [SerializeField] private float stopDistance = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool drawTargetGizmo = true;

    [Header("Axis Lock")]
    [Tooltip("Locks world Z so character never drifts forward/back.")]
    [SerializeField] public bool lockZAxis = true;

    public Vector3 targetLocation;
    public bool run = false;

    private Rigidbody _rb;
    private NavMeshAgent _agent;
    private bool _hasTarget;
    private float _initialZ = 0f;

    [Header("Runtime State (Read Only)")]
    [SerializeField] private Interactable currentInteractable;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [SerializeField]private bool isInteracting = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;

        _initialZ = transform.position.z;

        // Optional rigidbody constraint (still enforce manually for safety)
        if (lockZAxis)
            _rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

        if (useNavMeshAgent)
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent == null)
                _agent = gameObject.AddComponent<NavMeshAgent>();

            _agent.speed = walkSpeed;
            _agent.angularSpeed = 720f;
            _agent.acceleration = 24f;
            _agent.stoppingDistance = stopDistance;
            _agent.autoBraking = true;
            _agent.updateRotation = true;
            _agent.updatePosition = true; // We still let the agent update then correct Z.
        }
    }

    void Update()
    {

        HandleInteraction(currentInteractable);

        if (!_hasTarget) {
            animator.SetFloat("Speed", 0f);
            return;
        }

        

        if (useNavMeshAgent && _agent != null)
        {
            HandleAgentMovement();
        }
        else
        {
            HandleFallbackHorizontalMove();
        }


        UpdateAnimatorSpeed();
    }

    // External call to set movement target (full world pos, Z will be locked).
    public void SetTarget(Vector3 worldPos, bool runFlag = false, Interactable interactable = null)
    {
        run = runFlag;
        currentInteractable = interactable;

        if (currentInteractable != null)
        {
            Transform t = currentInteractable.transform;
            if (t.childCount > 0)
            {
                worldPos = t.GetChild(0).position;
            }
            else
            {
                worldPos.z = _initialZ;
            }
        }
        else if(lockZAxis)
        {
            worldPos.z = _initialZ;
        }

        targetLocation = worldPos;
        _hasTarget = true;

        

        if (useNavMeshAgent && _agent != null)
        {
            NavMeshHit hit;
            // Sample navmesh (Locked Z)
            if (NavMesh.SamplePosition(worldPos, out hit, 2f, NavMesh.AllAreas))
            {
                Vector3 navPos = hit.position;
                worldPos = navPos;
            }

            _agent.isStopped = false;
            _agent.speed = run ? runSpeed : walkSpeed;
            _agent.stoppingDistance = stopDistance;

            if (!_agent.SetDestination(worldPos))
                _hasTarget = false;
        }
    }

    public void ClearTarget()
    {
        _hasTarget = false;
        if (_agent != null && useNavMeshAgent)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }
    }

    private void HandleAgentMovement()
    {
        

        if (_agent.pathPending) return;

        _agent.speed = run ? runSpeed : walkSpeed;

        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            ClearTarget();
            return;
        }

        // When agent is close enough
        if (_agent.remainingDistance <= Mathf.Max(_agent.stoppingDistance, stopDistance))
        {
            ClearTarget();
            if (currentInteractable != null)
            {
                StartInteraction();
            }
            animator.SetFloat("Speed", 0f); // stop locomotion blend
            
            return;
        }

        if (_agent.isOnOffMeshLink)
        {
            _agent.CompleteOffMeshLink();
        }

        if (_agent.isOnOffMeshLink)
        {
            _agent.CompleteOffMeshLink();
        }
    }


    private void StartInteraction()
    {
        if (currentInteractable == null || animator == null) 
        {
            Debug.LogWarning($"[CharacterMovement] Cannot start interaction. Interactable: {currentInteractable}, Animator: {animator}");
            return;
        }

        Debug.Log($"[CharacterMovement] Starting interaction with {currentInteractable.name}");
        currentInteractable.OnInteract(gameObject);

        Transform t = currentInteractable.transform;
        if (t.childCount > 0)
        {
            transform.rotation = t.GetChild(0).rotation;
        }


        isInteracting = true;

    }
    private void HandleFallbackHorizontalMove()
    {
        float dx = targetLocation.x - transform.position.x;
        float moveSpeed = run ? runSpeed : walkSpeed;

        if (Mathf.Abs(dx) <= stopDistance)
        {
            // Stop horizontal movement only
            Vector3 v = _rb.linearVelocity;
            v.x = 0f;
            _rb.linearVelocity = v;
            ClearTarget();
            return;
        }

        float direction = Mathf.Sign(dx);
        float vx = direction * moveSpeed;

        Vector3 vel = _rb.linearVelocity;
        vel.x = vx;
        // Preserve existing Y (gravity), zero any Z drift
        if (lockZAxis) vel.z = 0f;
        _rb.linearVelocity = vel;
    }

    private void ForceZLock()
    {
        // Enforce transform Z
        Vector3 p = transform.position;
        p.z = _initialZ;
        transform.position = p;

        // Ensure agent or rigidbody doesn't accumulate Z velocity
        if (_rb != null && !_rb.isKinematic)
        {
            Vector3 v = _rb.linearVelocity;
            v.z = 0f;
            _rb.linearVelocity = v;
        }
        if (_agent != null && useNavMeshAgent)
        {
            // Agent internally tracks nextPosition; align it
            Vector3 np = _agent.nextPosition;
            np.z = _initialZ;
            _agent.nextPosition = np;
        }

        // Ensure target Z also locked
        if (_hasTarget && Mathf.Abs(targetLocation.z - _initialZ) > 0.0001f)
        {
            targetLocation.z = _initialZ;
            if (_agent != null && useNavMeshAgent && _agent.hasPath)
            {
                // If destination changed in Z, re-set to locked version
                Vector3 dest = _agent.destination;
                dest.z = _initialZ;
                if ((dest - _agent.destination).sqrMagnitude > 0.000001f)
                    _agent.SetDestination(dest);
            }
        }
    }

    private void UpdateAnimatorSpeed()
    {
        float horizontalSpeed = 0f;

        if (_agent != null && _agent.enabled)
        {
            // Use actual movement delta
            horizontalSpeed = _agent.velocity.magnitude;
        }
        else if (_rb != null)
        {
            horizontalSpeed = Mathf.Abs(_rb.linearVelocity.x); // fallback for non-NavMesh
        }

        if (horizontalSpeed < 1f)
            horizontalSpeed = 0f;

        if (animator != null)
            animator.SetFloat("Speed", horizontalSpeed);
    }

    public void PlayInteractionAnimation(Interactable.InteractionType type)
    {
        if (animator == null) return;

        switch (type)
        {
            case Interactable.InteractionType.Harvest:
                animator.SetTrigger("Harvest");
                break;
            case Interactable.InteractionType.Cook:
                animator.SetTrigger("Cook");
                break;
            case Interactable.InteractionType.Scavenge:
                animator.SetTrigger("Scavenge");
                break;
            case Interactable.InteractionType.Rest:
                animator.SetTrigger("Rest");
                break;
            case Interactable.InteractionType.Talk:
                animator.SetTrigger("Talk");
                break;
            case Interactable.InteractionType.Paint:
                animator.SetTrigger("Paint");
                break;
            case Interactable.InteractionType.Watering:
                animator.SetTrigger("Watering");
                break;
            default:
                // Default
                break;
        }
    }

    public void OnInteractionComplete()
    {
        // complete current interaction if any
        if (currentInteractable != null)
        {
            currentInteractable.InteractComplete();
            // clear the "interactedBy" on the interactable so it no longer tracks this character
            currentInteractable.ClearInteractedBy();
            currentInteractable = null;
        }

        // reset interaction state
        isInteracting = false;

        // ensure locomotion resumes cleanly
        animator.SetFloat("Speed", 0f);

    }

    private void HandleInteraction(Interactable interactable)
    {
        // If not currently interacting, nothing to interrupt
        if (!isInteracting || currentInteractable == null)
        {
            isInteracting = false;
            return;
        }

        // 1) If a different interactable was selected while interacting, cancel current interaction
        if (interactable != null && interactable != currentInteractable)
        {
            // Clear previous interactable state
            currentInteractable.ClearInteractedBy();
            currentInteractable = interactable;
            isInteracting = false;
            // Allow movement towards the newly selected interactable (SetTarget likely already called)
            return;
        }

        // 2) If the player moves (agent or rigidbody gains velocity or a new target is set) cancel current interaction
        bool characterIsMoving = _hasTarget; // a new target implies intent to move

        if (characterIsMoving)
        {
            currentInteractable.ClearInteractedBy();
            isInteracting = false;
            Debug.Log("HandleInteraction: Character Moved during interaction");
            return;
        }

        // 3) If the interaction animation has finished, complete the interaction.
        // Assumes the interaction animation states are tagged "Interaction" in the Animator.
        if (animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool inInteractionState = stateInfo.IsTag("Interaction");

            // Only log if state changed or periodically? Let's log if inInteractionState is true to see progress
            if (inInteractionState)
            {
                Debug.Log($"[CharacterMovement] HandleInteraction: In 'Interaction' state. NormalizedTime: {stateInfo.normalizedTime:F2}");
            }

            // When we're in the interaction state and its progress is near completion, it's finished.
            // Using 0.95 to be more robust against frame rate and animation length.
            if (inInteractionState && (stateInfo.normalizedTime % 1.0f >= 0.95f || (stateInfo.normalizedTime >= 0.95f && !stateInfo.loop)))
            {
                Debug.Log($"[CharacterMovement] HandleInteraction: Interaction animation complete at {stateInfo.normalizedTime:F2}");
                
                Debug.Log($"[CharacterMovement] HandleInteraction: Hello hello Interaction animation complete at {stateInfo.normalizedTime:F2}");
                OnInteractionComplete();
            }
        }

    }


    private void OnDrawGizmosSelected()
    {
        if (!drawTargetGizmo || !_hasTarget) return;
        Gizmos.color = run ? Color.red : Color.cyan;
        Vector3 drawPos = targetLocation;
        if (lockZAxis)
            drawPos.z = (_rb != null) ? (Application.isPlaying ? _initialZ : transform.position.z) : transform.position.z;
        Gizmos.DrawWireSphere(drawPos, 0.25f);
    }
}
