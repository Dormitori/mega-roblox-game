using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    public Camera camera;
    public CharacterController characterController;
    public CharacterMovementConfig moveConfig;
    public Transform characterModel;
    public Animator animator;

    private InputAction _move;
    private InputAction _jump;
    private float _upwardVelocity;

    private bool _jumped;

    private bool _jumpIsBuffered;
    private float _jumpBufferTime;

    private bool _hasCoyote;
    private float _coyoteTime;

    private float _modelRotateTime;

    private void Awake()
    {
        _move = InputSystem.actions.FindAction("Move");
        _jump = InputSystem.actions.FindAction("Jump");
    }

    private void Update()
    {
        var value = _move.ReadValue<Vector2>();
        var forward = new Vector3(camera.transform.forward.x, 0f, camera.transform.forward.z).normalized;
        var moveVector = forward * value.y + camera.transform.right * value.x;

        HandleGravity();

        HandleJump();

        if (moveVector.magnitude > Mathf.Epsilon)
        {
            animator.SetFloat("Speed",  moveConfig.speed);
            
            var targetRotation = Quaternion.LookRotation(moveVector);
            characterModel.transform.rotation = Quaternion.Lerp(
                characterModel.transform.rotation,
                targetRotation,
                moveConfig.rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }

        var resultMovement = moveVector * moveConfig.speed + Vector3.up * _upwardVelocity;
        characterController.Move(resultMovement * Time.deltaTime);
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

        if ((characterController.isGrounded || _hasCoyote) && (_jump.IsPressed() || _jumpIsBuffered))
        {
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

        if (!characterController.isGrounded && _jump.WasPressedThisFrame() && !_jumpIsBuffered)
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