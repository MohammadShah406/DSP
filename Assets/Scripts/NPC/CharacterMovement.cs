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
    [SerializeField] private bool lockZAxis = true;

    public Vector3 targetLocation;
    public bool run = false;

    private Rigidbody _rb;
    private NavMeshAgent _agent;
    private bool _hasTarget;
    private float _initialZ;

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
        if (!_hasTarget) {
            // Even idle we enforce Z lock (in case other systems moved it)
            if (lockZAxis && Mathf.Abs(transform.position.z - _initialZ) > 0.0001f)
                ForceZLock();
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

        if (lockZAxis)
            ForceZLock();
    }

    // External call to set movement target (full world pos, Z will be locked).
    public void SetTarget(Vector3 worldPos, bool runFlag = false)
    {
        run = runFlag;

        if (lockZAxis)
            worldPos.z = _initialZ;

        targetLocation = worldPos;
        _hasTarget = true;

        if (useNavMeshAgent && _agent != null)
        {
            NavMeshHit hit;
            // Sample navmesh (Locked Z)
            if (NavMesh.SamplePosition(worldPos, out hit, 2f, NavMesh.AllAreas))
            {
                Vector3 navPos = hit.position;
                if (lockZAxis)
                    navPos.z = _initialZ;
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

        if (_agent.remainingDistance <= Mathf.Max(_agent.stoppingDistance, stopDistance))
        {
            ClearTarget();
            return;
        }

        if (_agent.isOnOffMeshLink)
        {
            _agent.CompleteOffMeshLink();
        }
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
        if (_rb != null)
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
