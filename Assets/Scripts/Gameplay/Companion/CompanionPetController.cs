using UnityEngine;
using VContainer;

[RequireComponent(typeof(CharacterController))]
public class CompanionPetController : MonoBehaviour
{
    private static readonly int AnimatorSpeed = Animator.StringToHash("Speed");
    
    [SerializeField] private Transform followTarget;
    [SerializeField] private Transform waitAnchor;
    [SerializeField] private Vector3 followLocalOffset = new(0f, 0f, -1.8f);
    
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float stopDistanceNearPlayer = 1.2f;
    [SerializeField] private float stopDistanceNearAnchor = 0.35f;
    [SerializeField] private float gravity = -25f;

    [Header("Failsafe warp")]
    [SerializeField] private bool enableFailsafeWarp = true;
    [SerializeField] private float warpDistance = 14f;
    [SerializeField] private float warpVerticalDelta = 6f;
    [SerializeField] private float warpAfterSeconds = 1.25f;
    [SerializeField] private float warpGroundSnapRay = 6f;
    
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [SerializeField, Range(0.05f, 0.5f)] private float obstacleProbeRadius = 0.25f;
    [SerializeField] private float obstacleProbeDistance = 0.6f;
    [SerializeField, Range(0f, 1f)] private float strafeBlend = 0.65f;
    
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundRayLength = 2.5f;
    [SerializeField] private float groundSkin = 0.08f;
    [SerializeField] private float idleSpeedThreshold = 0.05f;
    
    [SerializeField] private Animator animator;

    private CharacterController _characterController;

    /// <summary>Перепривязка Animator после смены визуала (из CompanionPetVisual).</summary>
    public void BindAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }
    private float _verticalVelocity;
    private float _farTimer;

    [Inject]
    private void Construct(CharacterMovement playerMovement)
    {
        if (followTarget == null && playerMovement != null)
            followTarget = playerMovement.transform;
    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        PlayerTeleportBus.Teleported += OnPlayerTeleported;
    }

    private void OnDisable()
    {
        PlayerTeleportBus.Teleported -= OnPlayerTeleported;
    }

    private void Start()
    {
        if (followTarget != null) return;
        var cm = FindFirstObjectByType<CharacterMovement>();
        if (cm != null)
            followTarget = cm.transform;
    }

    private void Update()
    {
        if (!followTarget)
            return;

        if (enableFailsafeWarp)
            UpdateFailsafeWarp();

        UpdateVerticalVelocity();

        var inMine = CompanionMineState.OverlapCount > 0;
        var targetPos = inMine && waitAnchor != null
            ? waitAnchor.position
            : (inMine ? followTarget.position : GetFollowWorldPosition());
        var stopDist = inMine && waitAnchor != null ? stopDistanceNearAnchor : stopDistanceNearPlayer;

        MoveTowards(targetPos, stopDist);
        UpdateAnimator();
        SnapToGroundIfClose();
    }

    private void OnPlayerTeleported(Vector3 pos, Quaternion rot)
    {
        WarpNearFollowTarget();
    }

    private void UpdateFailsafeWarp()
    {
        if (followTarget == null)
            return;

        var playerPos = followTarget.position;
        var petPos = transform.position;

        var planarDist = Vector3.Distance(new Vector3(petPos.x, 0f, petPos.z), new Vector3(playerPos.x, 0f, playerPos.z));
        var verticalDelta = playerPos.y - petPos.y;

        var tooFar = planarDist > warpDistance || verticalDelta > warpVerticalDelta;
        if (!tooFar)
        {
            _farTimer = 0f;
            return;
        }

        _farTimer += Time.deltaTime;
        if (_farTimer >= warpAfterSeconds)
        {
            _farTimer = 0f;
            WarpNearFollowTarget();
        }
    }

    private void WarpNearFollowTarget()
    {
        if (followTarget == null || _characterController == null)
            return;

        var dst = GetFollowWorldPosition();

        // Snap to ground to avoid warping mid-air.
        if (Physics.Raycast(dst + Vector3.up * 2f, Vector3.down, out var hit, warpGroundSnapRay, groundLayers,
                QueryTriggerInteraction.Ignore))
        {
            dst.y = hit.point.y + groundSkin;
        }

        _characterController.enabled = false;
        transform.SetPositionAndRotation(dst, transform.rotation);
        _characterController.enabled = true;
        _verticalVelocity = -2f;
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        var v = _characterController.velocity;
        v.y = 0f;
        var speed01 = Mathf.Clamp01(v.magnitude / Mathf.Max(0.01f, moveSpeed));
        if (v.magnitude < idleSpeedThreshold)
            speed01 = 0f;
        animator.SetFloat(AnimatorSpeed, speed01);
    }

    private Vector3 GetFollowWorldPosition()
    {
        var t = followTarget;
        var flatForward = Vector3.ProjectOnPlane(t.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 1e-4f)
            flatForward = Vector3.forward;
        var flatRight = Vector3.Cross(Vector3.up, flatForward).normalized;
        var worldOffset = flatRight * followLocalOffset.x + Vector3.up * followLocalOffset.y
                          + flatForward * followLocalOffset.z;
        return t.position + worldOffset;
    }

    private void MoveTowards(Vector3 worldTarget, float stopDistance)
    {
        var pos = transform.position;
        var toTarget = worldTarget - pos;
        toTarget.y = 0f;
        var dist = toTarget.magnitude;
        if (dist <= stopDistance)
        {
            var faceDir = toTarget.sqrMagnitude > 1e-6f
                ? toTarget.normalized
                : Vector3.ProjectOnPlane(followTarget.forward, Vector3.up).normalized;
            FaceAlong(faceDir);
            _characterController.Move(Vector3.up * (_verticalVelocity * Time.deltaTime));
            return;
        }

        var dir = toTarget / dist;
        dir = ApplyObstacleAvoidance(pos, dir);

        FaceAlong(dir);

        var delta = dir * (moveSpeed * Time.deltaTime);
        delta.y = _verticalVelocity * Time.deltaTime;
        _characterController.Move(delta);
    }

    private Vector3 ApplyObstacleAvoidance(Vector3 fromPos, Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 1e-6f)
            return moveDir;

        var origin = fromPos + Vector3.up * (_characterController.height * 0.35f);
        if (!SphereCastIgnoringPlayer(origin, obstacleProbeRadius, moveDir, obstacleProbeDistance, out _))
            return moveDir;

        var up = Vector3.up;
        var right = Vector3.Cross(up, moveDir).normalized;
        if (right.sqrMagnitude < 1e-6f)
            return moveDir;

        var tryRight = (moveDir * (1f - strafeBlend) + right * strafeBlend).normalized;
        var tryLeft = (moveDir * (1f - strafeBlend) - right * strafeBlend).normalized;

        var clearRight = !SphereCastIgnoringPlayer(origin, obstacleProbeRadius * 0.9f, tryRight, obstacleProbeDistance, out _);
        var clearLeft = !SphereCastIgnoringPlayer(origin, obstacleProbeRadius * 0.9f, tryLeft, obstacleProbeDistance, out _);

        if (clearRight && !clearLeft) return tryRight;
        if (clearLeft && !clearRight) return tryLeft;
        if (clearRight && clearLeft) return tryRight;

        return (-moveDir * 0.35f + right * 0.65f).normalized;
    }
    
    private bool SphereCastIgnoringPlayer(Vector3 origin, float radius, Vector3 direction, float maxDistance, out RaycastHit hit)
    {
        var hits = Physics.SphereCastAll(origin, radius, direction, maxDistance, obstacleLayers, QueryTriggerInteraction.Ignore);
        hit = default;
        var bestDist = float.MaxValue;
        var found = false;
        foreach (var h in hits)
        {
            if (h.collider.transform == transform || h.collider.transform.IsChildOf(transform))
                continue;
            if (followTarget != null && BelongsToFollowTarget(h.collider.transform))
                continue;
            if (h.distance < bestDist)
            {
                bestDist = h.distance;
                hit = h;
                found = true;
            }
        }

        return found;
    }

    private bool BelongsToFollowTarget(Transform t)
    {
        if (followTarget == null || t == null) return false;
        return t == followTarget || t.IsChildOf(followTarget);
    }

    private void FaceAlong(Vector3 flatDirection)
    {
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude < 1e-6f)
            return;
        var look = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
    }

    private void UpdateVerticalVelocity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += gravity * Time.deltaTime;
    }

    private void SnapToGroundIfClose()
    {
        var pos = transform.position;
        if (!Physics.Raycast(pos + Vector3.up * 0.25f, Vector3.down, out var hit, groundRayLength, groundLayers,
                QueryTriggerInteraction.Ignore))
            return;

        var floorY = hit.point.y + groundSkin;
        if (_characterController.isGrounded || pos.y >= floorY)
            return;

        var fix = floorY - pos.y;
        if (fix > 0f && fix < 0.5f)
            _characterController.Move(Vector3.up * fix);
    }
}
