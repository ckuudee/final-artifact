using UnityEngine;

[RequireComponent(typeof(ModelAnimator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerLaneController : MonoBehaviour
{
    private const string LavaObjectName = "lava";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float crouchMoveSpeed = 3.5f;
    [SerializeField] private float groundAcceleration = 28f;
    [SerializeField] private float airAcceleration = 12f;
    [SerializeField] private float groundDeceleration = 10f;
    [SerializeField] private float airDeceleration = 3f;
    [SerializeField] private float jumpVelocity = 35f;
    [SerializeField] private float fallGravityMultiplier = 15.6f;
    [SerializeField] private float lowJumpGravityMultiplier = 9.1f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float rotationLerpSpeed = 14f;

    [Header("Player Collider")]
    // Collider settings are authored in world units so they remain correct on the 3x-scaled player.
    [SerializeField] private bool autoFitColliderToRenderers = false;
    [SerializeField] private float standingColliderHeight = 4.2f;
    [SerializeField] private float duckingColliderHeight = 2.1f;
    [SerializeField] private float colliderRadius = 0.55f;
    [SerializeField] private Vector3 standingColliderCenter = new Vector3(0f, 2.1f, 0f);
    [SerializeField] private Vector3 duckingColliderCenter = new Vector3(0f, 1.05f, 0f);
    [SerializeField] private float minimumStandingColliderHeight = 4.2f;
    [SerializeField] private float duckingHeightFactor = 0.5f;
    [SerializeField] private float maximumDuckingColliderHeight = 2.1f;

    [Header("Grounding")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckOffset = 0.08f;
    [SerializeField] private float ceilingCheckPadding = 0.04f;

    private Rigidbody _rigidbody;
    private ModelAnimator _animator;
    private CapsuleCollider _rootCollider;
    private Vector2 _moveInput;
    private bool _crouchHeld;
    private bool _isDucking;
    private bool _isGrounded;
    private bool _jumpAnimationActive;
    private bool _isDead;
    private float _lastGroundedTime = float.NegativeInfinity;
    private float _lastJumpPressedTime = float.NegativeInfinity;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<ModelAnimator>();
        _rootCollider = GetComponent<CapsuleCollider>();
        if (_rootCollider == null)
        {
            _rootCollider = gameObject.AddComponent<CapsuleCollider>();
        }

        DisableChildColliders();
        RefreshColliderDimensions();
        ApplyColliderState(false);

        _rigidbody.useGravity = true;
        _rigidbody.linearDamping = 0f;
        _rigidbody.angularDamping = 4f;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        _isGrounded = CheckGrounded();
        if (_isGrounded)
        {
            _lastGroundedTime = Time.time;
        }

        _animator.PlayIdle();
    }

    public void Configure(Vector3 spawnFeetPosition)
    {
        ResetState();
        PlaceFeetAt(spawnFeetPosition);
        _animator.PlayIdle();
    }

    public void ConfigureFromCurrentPosition()
    {
        ResetState();
        RefreshColliderDimensions();
        ApplyColliderState(false);

        _rigidbody.position = transform.position;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _isGrounded = CheckGrounded();
        if (_isGrounded)
        {
            _lastGroundedTime = Time.time;
        }

        _animator.PlayIdle();
    }

    private void Update()
    {
        if (IsGameOver())
        {
            _moveInput = Vector2.zero;
            _crouchHeld = false;
            _isDucking = false;
            ApplyColliderState(false);
            _animator.PlayIdle();
            return;
        }

        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (_moveInput.sqrMagnitude > 1f)
        {
            _moveInput.Normalize();
        }

        if (Input.GetButtonDown("Jump"))
        {
            _lastJumpPressedTime = Time.time;
        }

        _crouchHeld =
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.C);

        _isDucking = _crouchHeld || (_isDucking && !CanStandUp());

        ApplyColliderState(_isDucking);
        UpdateFacingDirection();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        if (IsGameOver())
        {
            Vector3 stoppedVelocity = _rigidbody.linearVelocity;
            stoppedVelocity.x = 0f;
            stoppedVelocity.z = 0f;
            _rigidbody.linearVelocity = stoppedVelocity;
            return;
        }

        _isGrounded = CheckGrounded();
        if (_isGrounded)
        {
            _lastGroundedTime = Time.time;
        }

        if (_isGrounded && _rigidbody.linearVelocity.y <= 0.05f)
        {
            _jumpAnimationActive = false;
        }

        Vector3 velocity = _rigidbody.linearVelocity;

        float targetMaxSpeed = _isDucking ? crouchMoveSpeed : moveSpeed;
        Vector3 desiredHorizontalVelocity = new Vector3(_moveInput.x, 0f, _moveInput.y) * targetMaxSpeed;
        Vector3 currentHorizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float acceleration = desiredHorizontalVelocity.sqrMagnitude > 0.001f
            ? (_isGrounded ? groundAcceleration : airAcceleration)
            : (_isGrounded ? groundDeceleration : airDeceleration);

        currentHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            desiredHorizontalVelocity,
            acceleration * Time.fixedDeltaTime);

        bool hasBufferedJump = (Time.time - _lastJumpPressedTime) <= jumpBufferTime;
        bool canUseGroundedJump = _isGrounded || (Time.time - _lastGroundedTime) <= coyoteTime;
        if (hasBufferedJump && canUseGroundedJump && !_isDucking)
        {
            velocity.y = jumpVelocity;
            _isGrounded = false;
            _jumpAnimationActive = true;
            _lastJumpPressedTime = float.NegativeInfinity;
            _lastGroundedTime = float.NegativeInfinity;
        }

        if (velocity.y < 0f)
        {
            velocity.y += Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (velocity.y > 0f && !Input.GetButton("Jump"))
        {
            velocity.y += Physics.gravity.y * (lowJumpGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        velocity.x = currentHorizontalVelocity.x;
        velocity.z = currentHorizontalVelocity.z;
        _rigidbody.linearVelocity = velocity;
    }

    private void UpdateAnimationState()
    {
        if (_jumpAnimationActive && !_isGrounded)
        {
            _animator.PlayJumping();
            return;
        }

        if (_isDucking)
        {
            _animator.PlayDucking();
            return;
        }

        if (_moveInput.sqrMagnitude > 0.01f)
        {
            _animator.PlayWalking();
            return;
        }

        _animator.PlayIdle();
    }

    private void ApplyColliderState(bool ducking)
    {
        if (_rootCollider == null)
        {
            return;
        }

        GetLocalColliderSettings(
            ducking ? duckingColliderHeight : standingColliderHeight,
            ducking ? GetColliderCenterForHeight(duckingColliderHeight, duckingColliderCenter) : GetColliderCenterForHeight(standingColliderHeight, standingColliderCenter),
            colliderRadius,
            out float localHeight,
            out Vector3 localCenter,
            out float localRadius);

        _rootCollider.direction = 1;
        _rootCollider.isTrigger = false;
        _rootCollider.radius = localRadius;
        _rootCollider.height = localHeight;
        _rootCollider.center = localCenter;
    }

    private void UpdateFacingDirection()
    {
        if (_moveInput.sqrMagnitude < 0.01f)
        {
            return;
        }

        Vector3 lookDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }

    private void RefreshColliderDimensions()
    {
        if (_rootCollider == null || !autoFitColliderToRenderers)
        {
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        bool hasBounds = false;
        Bounds localBounds = default;

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
            {
                continue;
            }

            Bounds worldBounds = renderer.bounds;
            Vector3 extents = worldBounds.extents;
            Vector3 center = worldBounds.center;
            Vector3[] corners =
            {
                center + new Vector3(-extents.x, -extents.y, -extents.z),
                center + new Vector3(-extents.x, -extents.y, extents.z),
                center + new Vector3(-extents.x, extents.y, -extents.z),
                center + new Vector3(-extents.x, extents.y, extents.z),
                center + new Vector3(extents.x, -extents.y, -extents.z),
                center + new Vector3(extents.x, -extents.y, extents.z),
                center + new Vector3(extents.x, extents.y, -extents.z),
                center + new Vector3(extents.x, extents.y, extents.z)
            };

            foreach (Vector3 corner in corners)
            {
                Vector3 localCorner = transform.InverseTransformPoint(corner);
                if (!hasBounds)
                {
                    localBounds = new Bounds(localCorner, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(localCorner);
                }
            }
        }

        if (!hasBounds)
        {
            return;
        }

        float verticalScale = GetVerticalScale();
        float horizontalScale = GetHorizontalScale();
        float feetOffset = localBounds.min.y * verticalScale;
        float diameter = Mathf.Max(localBounds.size.x, localBounds.size.z) * 0.9f * horizontalScale;
        colliderRadius = Mathf.Max(0.2f, diameter * 0.5f);
        standingColliderHeight = Mathf.Max(localBounds.size.y * 0.95f * verticalScale, minimumStandingColliderHeight, colliderRadius * 2f + 0.05f);
        standingColliderCenter = new Vector3(localBounds.center.x * horizontalScale, feetOffset + (standingColliderHeight * 0.5f), localBounds.center.z * horizontalScale);

        float desiredDuckingHeight = Mathf.Min(standingColliderHeight * duckingHeightFactor, maximumDuckingColliderHeight);
        duckingColliderHeight = Mathf.Max(desiredDuckingHeight, colliderRadius * 2f + 0.05f);
        duckingColliderCenter = new Vector3(localBounds.center.x * horizontalScale, feetOffset + (duckingColliderHeight * 0.5f), localBounds.center.z * horizontalScale);
    }

    private bool CheckGrounded()
    {
        if (_rootCollider == null)
        {
            return false;
        }

        GetLocalColliderSettings(
            _isDucking ? duckingColliderHeight : standingColliderHeight,
            _isDucking ? GetColliderCenterForHeight(duckingColliderHeight, duckingColliderCenter) : GetColliderCenterForHeight(standingColliderHeight, standingColliderCenter),
            colliderRadius,
            out float localHeight,
            out Vector3 localCenter,
            out float localRadius);

        GetWorldCapsule(
            localCenter,
            localHeight,
            localRadius,
            out _,
            out Vector3 bottom,
            out float worldRadius);

        float probeRadius = Mathf.Max(groundCheckRadius, worldRadius * 0.92f);
        Vector3 probeTop = bottom + Vector3.up * groundCheckOffset;
        Vector3 probeBottom = bottom + Vector3.down * groundCheckOffset;
        Collider[] hits = Physics.OverlapCapsule(probeTop, probeBottom, probeRadius, ~0, QueryTriggerInteraction.Ignore);
        foreach (Collider hit in hits)
        {
            if (hit == null || hit == _rootCollider)
            {
                continue;
            }

            if (hit.transform.IsChildOf(transform))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private bool CanStandUp()
    {
        if (_rootCollider == null)
        {
            return true;
        }

        GetLocalColliderSettings(
            standingColliderHeight,
            GetColliderCenterForHeight(standingColliderHeight, standingColliderCenter),
            colliderRadius,
            out float localHeight,
            out Vector3 localCenter,
            out float localRadius);

        GetWorldCapsule(
            localCenter,
            localHeight,
            localRadius,
            out Vector3 top,
            out Vector3 bottom,
            out float worldRadius);

        Vector3 ceilingProbeBottom = bottom + Vector3.up * (worldRadius + ceilingCheckPadding);
        Collider[] hits = Physics.OverlapCapsule(
            top,
            ceilingProbeBottom,
            Mathf.Max(0.05f, worldRadius - ceilingCheckPadding),
            ~0,
            QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            if (hit == null || hit == _rootCollider)
            {
                continue;
            }

            if (hit.transform.IsChildOf(transform))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void DisableChildColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                collider.enabled = false;
            }
        }
    }

    private void PlaceFeetAt(Vector3 feetPosition)
    {
        RefreshColliderDimensions();
        ApplyColliderState(false);

        float worldFeetOffset = standingColliderCenter.y - (standingColliderHeight * 0.5f);
        Vector3 worldPosition = new Vector3(
            feetPosition.x,
            feetPosition.y - worldFeetOffset,
            feetPosition.z);

        transform.position = worldPosition;
        _rigidbody.position = worldPosition;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _isGrounded = CheckGrounded();
    }

    private void ResetState()
    {
        _moveInput = Vector2.zero;
        _crouchHeld = false;
        _isDucking = false;
        _jumpAnimationActive = false;
        _isDead = false;
        _lastGroundedTime = float.NegativeInfinity;
        _lastJumpPressedTime = float.NegativeInfinity;
    }

    private Vector3 GetColliderCenterForHeight(float targetHeight, Vector3 sourceCenter)
    {
        float feetOffset = standingColliderCenter.y - (standingColliderHeight * 0.5f);
        return new Vector3(sourceCenter.x, feetOffset + (targetHeight * 0.5f), sourceCenter.z);
    }

    private void GetLocalColliderSettings(
        float worldHeight,
        Vector3 worldCenter,
        float worldRadius,
        out float localHeight,
        out Vector3 localCenter,
        out float localRadius)
    {
        float verticalScale = GetVerticalScale();
        float horizontalScale = GetHorizontalScale();

        localRadius = Mathf.Max(0.05f, worldRadius / horizontalScale);
        localHeight = Mathf.Max(worldHeight / verticalScale, (localRadius * 2f) + 0.05f);
        localCenter = new Vector3(
            worldCenter.x / horizontalScale,
            worldCenter.y / verticalScale,
            worldCenter.z / horizontalScale);
    }

    private float GetVerticalScale()
    {
        return Mathf.Max(0.0001f, Mathf.Abs(transform.lossyScale.y));
    }

    private float GetHorizontalScale()
    {
        Vector3 lossyScale = transform.lossyScale;
        return Mathf.Max(0.0001f, Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z)));
    }

    private void GetWorldCapsule(
        Vector3 localCenter,
        float localHeight,
        float localRadius,
        out Vector3 top,
        out Vector3 bottom,
        out float worldRadius)
    {
        Vector3 lossyScale = transform.lossyScale;
        float verticalScale = Mathf.Abs(lossyScale.y);
        float horizontalScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z));

        worldRadius = Mathf.Max(0.05f, localRadius * horizontalScale);
        float scaledHeight = Mathf.Max(localHeight * verticalScale, worldRadius * 2f);
        float hemisphereOffset = Mathf.Max(0f, (scaledHeight * 0.5f) - worldRadius);

        Vector3 worldCenter = transform.TransformPoint(localCenter);
        top = worldCenter + Vector3.up * hemisphereOffset;
        bottom = worldCenter - Vector3.up * hemisphereOffset;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsLava(collision.gameObject))
        {
            TriggerGameOver();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (IsLava(collision.gameObject))
        {
            TriggerGameOver();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsLava(other.gameObject))
        {
            TriggerGameOver();
        }
    }

    private bool IsLava(GameObject candidate)
    {
        return candidate != null && candidate.name == LavaObjectName;
    }

    private void TriggerGameOver()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;
        _moveInput = Vector2.zero;
        _crouchHeld = false;
        _isDucking = false;
        _animator.PlayIdle();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TriggerGameOver();
        }

        Time.timeScale = 0f;
    }

    private bool IsGameOver()
    {
        return _isDead || Time.timeScale <= 0f;
    }
}
