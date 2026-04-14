using Core.Audio;
using UnityEngine;
using VContainer;

public class CharacterMovement : MonoBehaviour
{
    public CameraLook cameraLook;
    public CharacterController characterController;
    public CharacterMovementConfig moveConfig;
    public Animator animator;
    
    public ControlsProvider controlsProvider;

    public MineBlocks mineBlocks;

    [SerializeField] private bool playFootsteps = true;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.5f;
    [SerializeField] private float footstepPitchMin = 0.5f;
    [SerializeField] private float footstepPitchMax = 1.05f;
    [SerializeField] private float footstepMinInterval = 0.3f;
    [SerializeField, Range(0f, 1.5f)] private float footstepDelayAfterJump = 0.28f;

    [SerializeField] private bool playJumpLandSounds = true;
    [SerializeField, Range(0f, 1f)] private float jumpVolume = 0.75f;
    [SerializeField, Range(0f, 1f)] private float landVolume = 0.65f;
    [SerializeField, Range(0f, 0.2f)] private float jumpPitchJitterHalfRange = 0.06f;
    [SerializeField, Range(0f, 0.2f)] private float landPitchJitterHalfRange = 0.08f;

    private IAudioService _audioService;
    private float _lastFootstepTime = -999f;
    private float _footstepAllowedAfterTime = -999f;

    private float _upwardVelocity;

    private bool _jumped;

    private bool _jumpIsBuffered;
    private float _jumpBufferTime;

    private bool _hasCoyote;
    private float _coyoteTime;

    private float _modelRotateTime;

    [Inject]
    private void Construct(IAudioService audioService)
    {
        _audioService = audioService;
    }
    

    private void Update()
    {
        var groundedBeforeMove = characterController.isGrounded;

        var value = controlsProvider.Controls.GetMoveDirection();

        cameraLook.GetHorizontalMoveAxes(out var forward, out var right);
        if (controlsProvider.AutoMiningEnabled)
        {
            // игнорируем вращение камеры при автомайнинге
            right = Vector3.right;
            forward = Vector3.forward;
        }

        var moveVector = forward * value.y + right * value.x;

        HandleGravity();

        HandleJump();

        if (moveVector.magnitude > Mathf.Epsilon)
        {
            animator.SetFloat("Speed",  moveConfig.speed);
            
            var targetRotation = Quaternion.LookRotation(moveVector);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                moveConfig.rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }

        TryPlayFootstep(moveVector);

        var resultMovement = moveVector * moveConfig.speed + Vector3.up * _upwardVelocity;
        characterController.Move(resultMovement * Time.deltaTime);

        if (playJumpLandSounds && _audioService != null && characterController.isGrounded && !groundedBeforeMove)
            _audioService.PlaySfx(SoundId.Land, landVolume, landPitchJitterHalfRange);
    }

    private void TryPlayFootstep(Vector3 moveVector)
    {
        if (!playFootsteps || _audioService == null)
            return;
        if (!characterController.isGrounded)
            return;
        if (moveVector.sqrMagnitude <= Mathf.Epsilon)
            return;
        if (Time.time < _footstepAllowedAfterTime)
            return;
        if (Time.time - _lastFootstepTime < footstepMinInterval)
            return;

        var pMin = Mathf.Min(footstepPitchMin, footstepPitchMax);
        var pMax = Mathf.Max(footstepPitchMin, footstepPitchMax);
        _audioService.PlaySfx(SoundId.Footstep, footstepVolume, pMin, pMax);
        _lastFootstepTime = Time.time;
    }

    private void HandleGravity()
    {
        if (!characterController.isGrounded)
        {
            animator.SetBool("IsGrounded", false);
            if (_upwardVelocity > 0)
            {
                animator.SetBool("IsFalling", false);
                _upwardVelocity -= moveConfig.UpwardGravity * Time.deltaTime;
            }
            else
            {
                animator.SetBool("IsFalling", true);
                _upwardVelocity -= moveConfig.DownwardGravity * Time.deltaTime;
            }
        }
        else
        {
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsFalling", false);
        }
    }

    private void HandleJump()
    {
        if (characterController.isGrounded)
        {
            _jumped = false;
            _coyoteTime = 0f;
            if (_upwardVelocity < -2f)
                _upwardVelocity = -2f;
        }

        HandleJumpBuffer();
        HandleCoyoteTime();

        if ((characterController.isGrounded || _hasCoyote) && (controlsProvider.Controls.JumpIsPressed() || _jumpIsBuffered))
        {
            if (playJumpLandSounds && _audioService != null)
                _audioService.PlaySfx(SoundId.Jump, jumpVolume, jumpPitchJitterHalfRange);

            _footstepAllowedAfterTime = Time.time + footstepDelayAfterJump;

            _upwardVelocity = moveConfig.JumpVelocity;
            _jumpIsBuffered = false;
            _hasCoyote = false;
            _jumped = true;
        }
    }

    private void HandleJumpBuffer()
    {
        if (_jumpIsBuffered)
        {
            _jumpBufferTime += Time.deltaTime;
            if (_jumpBufferTime > moveConfig.jumpBufferTime)
                _jumpIsBuffered = false;
        }

        if (!characterController.isGrounded && (controlsProvider.Controls.JumpedThisFrame()) && !_jumpIsBuffered)
        {
            _jumpIsBuffered = true;
            _jumpBufferTime = 0;
        }
    }

    private void HandleCoyoteTime()
    {
        if (_hasCoyote)
        {
            _coyoteTime += Time.deltaTime;
            if (_coyoteTime > moveConfig.coyoteTime)
                _hasCoyote = false;
        }

        if (!characterController.isGrounded && !_jumped && !_hasCoyote && _coyoteTime <= moveConfig.coyoteTime)
        {
            _hasCoyote = true;
            _coyoteTime = 0;
        }
    }
}