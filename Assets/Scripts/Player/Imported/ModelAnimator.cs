using UnityEngine;

/// <summary>
/// Procedural animation driver for the imported player model.
/// The built-in Animator is disabled so it cannot fight the rigidbody/controller.
/// </summary>
public class ModelAnimator : MonoBehaviour
{
    public enum AnimationMode
    {
        Idle,
        Rotation,
        Walking,
        Ducking,
        Jumping
    }

    [Header("Body Part References")]
    public GameObject body;
    public Transform head;
    public Transform chest;
    public Transform leftShoulder;
    public Transform rightShoulder;
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftLeg;
    public Transform rightLeg;

    [Header("Animation Settings")]
    public float rotateSpeed = 50f;
    public float walkSpeed = 5f;
    public float jumpHeight = 0.5f;
    [SerializeField] private float crouchDepth = 0.34f;
    [SerializeField] private float walkLegSwing = 22f;
    [SerializeField] private float walkBounce = 0.025f;
    [SerializeField] private float walkArmSwing = 18f;
    [SerializeField] private float walkTorsoCounterSwing = 3.5f;
    [SerializeField] private float armRestDrop = -86f;
    [SerializeField] private float elbowRestBend = 18f;

    [Header("Startup")]
    [SerializeField] private AnimationMode startMode = AnimationMode.Idle;

    private AnimationMode _mode = AnimationMode.Idle;
    private Animator _humanoidAnimator;
    private Transform _visualRoot;
    private Transform _hips;
    private Transform _leftForeArm;
    private Transform _rightForeArm;
    private Transform _leftLowerLeg;
    private Transform _rightLowerLeg;
    private float _walkTimer;
    private float _rotationAngle;
    private bool _poseCached;
    private bool _canOffsetVisualRoot;

    private Vector3 _visualRootLocalPosition;
    private Quaternion _visualRootLocalRotation;
    private Quaternion _hipsRotation;
    private Quaternion _headRotation;
    private Quaternion _chestRotation;
    private Quaternion _leftShoulderRotation;
    private Quaternion _rightShoulderRotation;
    private Quaternion _leftArmRotation;
    private Quaternion _rightArmRotation;
    private Quaternion _leftForeArmRotation;
    private Quaternion _rightForeArmRotation;
    private Quaternion _leftLegRotation;
    private Quaternion _rightLegRotation;
    private Quaternion _leftLowerLegRotation;
    private Quaternion _rightLowerLegRotation;

    public AnimationMode CurrentMode => _mode;

    private void Awake()
    {
        ResolveRig();
        CachePose();
    }

    private void Start()
    {
        if (startMode != AnimationMode.Idle)
        {
            SetMode(startMode, true);
        }
    }

    private void LateUpdate()
    {
        CachePose();
        ResetPose();

        switch (_mode)
        {
            case AnimationMode.Idle:
                AnimateIdle();
                break;
            case AnimationMode.Rotation:
                AnimateRotation();
                break;
            case AnimationMode.Walking:
                AnimateWalking();
                break;
            case AnimationMode.Ducking:
                AnimateDucking();
                break;
            case AnimationMode.Jumping:
                AnimateJumping();
                break;
        }
    }

    public void PlayIdle() => SetMode(AnimationMode.Idle);
    public void PlayRotation() => SetMode(AnimationMode.Rotation);
    public void PlayWalking() => SetMode(AnimationMode.Walking);
    public void PlayDucking() => SetMode(AnimationMode.Ducking);
    public void PlayJumping() => SetMode(AnimationMode.Jumping);

    public void ToggleRotation() => ToggleMode(AnimationMode.Rotation);
    public void ToggleWalking() => ToggleMode(AnimationMode.Walking);
    public void ToggleDucking() => ToggleMode(AnimationMode.Ducking);
    public void ToggleJumping() => ToggleMode(AnimationMode.Jumping);

    public void StopAllAnimations()
    {
        SetMode(AnimationMode.Idle);
    }

    private void ToggleMode(AnimationMode mode)
    {
        SetMode(_mode == mode ? AnimationMode.Idle : mode);
    }

    private void SetMode(AnimationMode mode, bool force = false)
    {
        ResolveRig();
        CachePose();
        if (!force && _mode == mode)
        {
            return;
        }

        _mode = mode;
        _walkTimer = 0f;
        _rotationAngle = 0f;
        ResetPose();
    }

    private void ResolveRig()
    {
        if (body == null)
        {
            body = transform.childCount > 0 ? transform.GetChild(0).gameObject : gameObject;
        }

        _visualRoot = body != null ? body.transform : transform;
        _humanoidAnimator = GetComponentInChildren<Animator>();
        if (_humanoidAnimator != null)
        {
            if (_visualRoot == null || _visualRoot == transform)
            {
                _visualRoot = _humanoidAnimator.transform;
                body = _visualRoot.gameObject;
            }

            if (_humanoidAnimator.avatar != null && _humanoidAnimator.avatar.isValid && _humanoidAnimator.isHuman)
            {
                if (head == null)
                {
                    head = _humanoidAnimator.GetBoneTransform(HumanBodyBones.Head);
                }

                if (chest == null)
                {
                    chest = _humanoidAnimator.GetBoneTransform(HumanBodyBones.UpperChest);
                }

                if (chest == null)
                {
                    chest = _humanoidAnimator.GetBoneTransform(HumanBodyBones.Chest);
                }

                if (chest == null)
                {
                    chest = _humanoidAnimator.GetBoneTransform(HumanBodyBones.Spine);
                }

                if (leftShoulder == null)
                {
                    leftShoulder = _humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                }

                if (rightShoulder == null)
                {
                    rightShoulder = _humanoidAnimator.GetBoneTransform(HumanBodyBones.RightShoulder);
                }

                if (leftArm == null)
                {
                    leftArm = _humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                }

                if (rightArm == null)
                {
                    rightArm = _humanoidAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                }

                if (leftLeg == null)
                {
                    leftLeg = _humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                }

                if (rightLeg == null)
                {
                    rightLeg = _humanoidAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                }

                _hips = _humanoidAnimator.GetBoneTransform(HumanBodyBones.Hips);
                _leftForeArm = _humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                _rightForeArm = _humanoidAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                _leftLowerLeg = _humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                _rightLowerLeg = _humanoidAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            }

            _humanoidAnimator.enabled = false;
        }

        _canOffsetVisualRoot = _visualRoot != null && _visualRoot != transform;
    }

    private void CachePose()
    {
        if (_poseCached)
        {
            return;
        }

        ResolveRig();

        if (_visualRoot == null)
        {
            _visualRoot = transform;
            _canOffsetVisualRoot = false;
        }

        _visualRootLocalPosition = _visualRoot.localPosition;
        _visualRootLocalRotation = _visualRoot.localRotation;
        _hipsRotation = _hips != null ? _hips.localRotation : Quaternion.identity;
        _headRotation = head != null ? head.localRotation : Quaternion.identity;
        _chestRotation = chest != null ? chest.localRotation : Quaternion.identity;
        _leftShoulderRotation = leftShoulder != null ? leftShoulder.localRotation : Quaternion.identity;
        _rightShoulderRotation = rightShoulder != null ? rightShoulder.localRotation : Quaternion.identity;
        _leftArmRotation = leftArm != null ? leftArm.localRotation : Quaternion.identity;
        _rightArmRotation = rightArm != null ? rightArm.localRotation : Quaternion.identity;
        _leftForeArmRotation = _leftForeArm != null ? _leftForeArm.localRotation : Quaternion.identity;
        _rightForeArmRotation = _rightForeArm != null ? _rightForeArm.localRotation : Quaternion.identity;
        _leftLegRotation = leftLeg != null ? leftLeg.localRotation : Quaternion.identity;
        _rightLegRotation = rightLeg != null ? rightLeg.localRotation : Quaternion.identity;
        _leftLowerLegRotation = _leftLowerLeg != null ? _leftLowerLeg.localRotation : Quaternion.identity;
        _rightLowerLegRotation = _rightLowerLeg != null ? _rightLowerLeg.localRotation : Quaternion.identity;
        _poseCached = true;
    }

    private void AnimateIdle()
    {
        ApplyNaturalArmPose(
            0f,
            0f,
            elbowRestBend,
            elbowRestBend,
            armRestDrop);
        ApplyRotation(chest, _chestRotation, new Vector3(2f, 0f, 0f));
        ApplyRotation(head, _headRotation, new Vector3(-1f, 0f, 0f));
    }

    private void AnimateRotation()
    {
        if (_visualRoot == null)
        {
            return;
        }

        _rotationAngle += rotateSpeed * Time.deltaTime;
        _visualRoot.localRotation = _visualRootLocalRotation * Quaternion.Euler(0f, _rotationAngle, 0f);
    }

    private void AnimateWalking()
    {
        _walkTimer = Mathf.Repeat(_walkTimer + (Time.deltaTime * walkSpeed * 0.24f), 1f);

        float leftPhase = _walkTimer;
        float rightPhase = Mathf.Repeat(leftPhase + 0.5f, 1f);

        float leftUpperLegPitch = SampleWalkPose(leftPhase, walkLegSwing, walkLegSwing * 0.6f, -4f, -walkLegSwing * 0.9f);
        float rightUpperLegPitch = SampleWalkPose(rightPhase, walkLegSwing, walkLegSwing * 0.6f, -4f, -walkLegSwing * 0.9f);
        float leftLowerLegPitch = SampleWalkPose(leftPhase, 8f, 10f, 18f, 28f);
        float rightLowerLegPitch = SampleWalkPose(rightPhase, 8f, 10f, 18f, 28f);

        float leftArmForwardSwing = SampleWalkPose(rightPhase, walkArmSwing, walkArmSwing * 0.55f, 0f, -walkArmSwing);
        float rightArmForwardSwing = SampleWalkPose(leftPhase, walkArmSwing, walkArmSwing * 0.55f, 0f, -walkArmSwing);
        float leftForeArmBend = SampleWalkPose(rightPhase, elbowRestBend + 4f, elbowRestBend + 8f, elbowRestBend + 2f, elbowRestBend - 2f);
        float rightForeArmBend = SampleWalkPose(leftPhase, elbowRestBend + 4f, elbowRestBend + 8f, elbowRestBend + 2f, elbowRestBend - 2f);

        float hipRoll = SampleWalkPose(leftPhase, -2f, -1f, 1f, 2f);
        float torsoRoll = -hipRoll * walkTorsoCounterSwing * 0.75f;
        float headRoll = -hipRoll;
        float bounce = SampleWalkPose(leftPhase, walkBounce * 0.15f, -walkBounce * 0.6f, 0f, walkBounce);

        ApplyVisualRootOffset(new Vector3(0f, bounce, 0f));
        ApplyNaturalArmPose(
            leftArmForwardSwing,
            rightArmForwardSwing,
            leftForeArmBend,
            rightForeArmBend,
            armRestDrop);
        ApplyRotation(_hips, _hipsRotation, new Vector3(0f, 0f, hipRoll));
        ApplyRotation(chest, _chestRotation, new Vector3(4f, 0f, torsoRoll));
        ApplyRotation(head, _headRotation, new Vector3(-2f, 0f, headRoll));
        ApplyRotation(leftLeg, _leftLegRotation, new Vector3(leftUpperLegPitch, 0f, 0f));
        ApplyRotation(rightLeg, _rightLegRotation, new Vector3(rightUpperLegPitch, 0f, 0f));
        ApplyRotation(_leftLowerLeg, _leftLowerLegRotation, new Vector3(leftLowerLegPitch, 0f, 0f));
        ApplyRotation(_rightLowerLeg, _rightLowerLegRotation, new Vector3(rightLowerLegPitch, 0f, 0f));
    }

    private void AnimateJumping()
    {
        float armForwardLift = Mathf.Clamp(jumpHeight * 12f, 8f, 16f);

        ApplyNaturalArmPose(
            armForwardLift,
            armForwardLift,
            elbowRestBend + 6f,
            elbowRestBend + 6f,
            armRestDrop + 14f);
        ApplyRotation(_hips, _hipsRotation, new Vector3(8f, 0f, 0f));
        ApplyRotation(chest, _chestRotation, new Vector3(-10f, 0f, 0f));
        ApplyRotation(head, _headRotation, new Vector3(8f, 0f, 0f));
        ApplyRotation(leftLeg, _leftLegRotation, new Vector3(18f, 0f, 0f));
        ApplyRotation(rightLeg, _rightLegRotation, new Vector3(18f, 0f, 0f));
        ApplyRotation(_leftLowerLeg, _leftLowerLegRotation, new Vector3(-24f, 0f, 0f));
        ApplyRotation(_rightLowerLeg, _rightLowerLegRotation, new Vector3(-24f, 0f, 0f));
    }

    private void AnimateDucking()
    {
        ApplyVisualRootOffset(new Vector3(0f, -crouchDepth, 0f));
        ApplyNaturalArmPose(
            -6f,
            -6f,
            elbowRestBend + 12f,
            elbowRestBend + 12f,
            armRestDrop + 4f);
        ApplyRotation(_hips, _hipsRotation, new Vector3(12f, 0f, 0f));
        ApplyRotation(chest, _chestRotation, new Vector3(18f, 0f, 0f));
        ApplyRotation(head, _headRotation, new Vector3(-10f, 0f, 0f));
        ApplyRotation(leftLeg, _leftLegRotation, new Vector3(-38f, 0f, 0f));
        ApplyRotation(rightLeg, _rightLegRotation, new Vector3(-38f, 0f, 0f));
        ApplyRotation(_leftLowerLeg, _leftLowerLegRotation, new Vector3(52f, 0f, 0f));
        ApplyRotation(_rightLowerLeg, _rightLowerLegRotation, new Vector3(52f, 0f, 0f));
    }

    private void ApplyNaturalArmPose(
        float leftArmForwardSwing,
        float rightArmForwardSwing,
        float leftForeArmBend,
        float rightForeArmBend,
        float armDrop)
    {
        ApplyRotation(leftShoulder, _leftShoulderRotation, Vector3.zero);
        ApplyRotation(rightShoulder, _rightShoulderRotation, Vector3.zero);
        ApplyArmSwingRotation(leftArm, _leftArmRotation, armDrop, leftArmForwardSwing, true);
        ApplyArmSwingRotation(rightArm, _rightArmRotation, armDrop, rightArmForwardSwing, false);
        ApplyForeArmBend(_leftForeArm, _leftForeArmRotation, leftForeArmBend);
        ApplyForeArmBend(_rightForeArm, _rightForeArmRotation, rightForeArmBend);
    }

    private void ApplyVisualRootOffset(Vector3 localOffset)
    {
        if (_canOffsetVisualRoot && _visualRoot != null)
        {
            _visualRoot.localPosition = _visualRootLocalPosition + localOffset;
        }
    }

    private static void ApplyRotation(Transform target, Quaternion baseRotation, Vector3 eulerOffset)
    {
        if (target == null)
        {
            return;
        }

        target.localRotation = baseRotation * Quaternion.Euler(eulerOffset);
    }

    private static void ApplyArmSwingRotation(
        Transform target,
        Quaternion baseRotation,
        float dropAngle,
        float forwardSwing,
        bool isLeftArm)
    {
        if (target == null)
        {
            return;
        }

        float mirroredSwing = isLeftArm ? forwardSwing : -forwardSwing;
        Quaternion poseOffset =
            Quaternion.AngleAxis(dropAngle, Vector3.right) *
            Quaternion.AngleAxis(mirroredSwing, Vector3.forward);

        target.localRotation = baseRotation * poseOffset;
    }

    private static void ApplyForeArmBend(
        Transform target,
        Quaternion baseRotation,
        float bendAngle)
    {
        if (target == null)
        {
            return;
        }

        target.localRotation = baseRotation * Quaternion.AngleAxis(-bendAngle, Vector3.right);
    }

    private static float SampleWalkPose(float phase, float contact, float down, float passing, float up)
    {
        phase = Mathf.Repeat(phase, 1f);

        if (phase < 0.25f)
        {
            return Mathf.Lerp(contact, down, SmoothCycle(phase / 0.25f));
        }

        if (phase < 0.5f)
        {
            return Mathf.Lerp(down, passing, SmoothCycle((phase - 0.25f) / 0.25f));
        }

        if (phase < 0.75f)
        {
            return Mathf.Lerp(passing, up, SmoothCycle((phase - 0.5f) / 0.25f));
        }

        return Mathf.Lerp(up, contact, SmoothCycle((phase - 0.75f) / 0.25f));
    }

    private static float SmoothCycle(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - (2f * t));
    }

    private void ResetPose()
    {
        if (_visualRoot != null)
        {
            _visualRoot.localRotation = _visualRootLocalRotation;
            if (_canOffsetVisualRoot)
            {
                _visualRoot.localPosition = _visualRootLocalPosition;
            }
        }

        if (_hips != null)
        {
            _hips.localRotation = _hipsRotation;
        }

        if (head != null)
        {
            head.localRotation = _headRotation;
        }

        if (chest != null)
        {
            chest.localRotation = _chestRotation;
        }

        if (leftShoulder != null)
        {
            leftShoulder.localRotation = _leftShoulderRotation;
        }

        if (rightShoulder != null)
        {
            rightShoulder.localRotation = _rightShoulderRotation;
        }

        if (leftArm != null)
        {
            leftArm.localRotation = _leftArmRotation;
        }

        if (rightArm != null)
        {
            rightArm.localRotation = _rightArmRotation;
        }

        if (_leftForeArm != null)
        {
            _leftForeArm.localRotation = _leftForeArmRotation;
        }

        if (_rightForeArm != null)
        {
            _rightForeArm.localRotation = _rightForeArmRotation;
        }

        if (leftLeg != null)
        {
            leftLeg.localRotation = _leftLegRotation;
        }

        if (rightLeg != null)
        {
            rightLeg.localRotation = _rightLegRotation;
        }

        if (_leftLowerLeg != null)
        {
            _leftLowerLeg.localRotation = _leftLowerLegRotation;
        }

        if (_rightLowerLeg != null)
        {
            _rightLowerLeg.localRotation = _rightLowerLegRotation;
        }
    }
}
