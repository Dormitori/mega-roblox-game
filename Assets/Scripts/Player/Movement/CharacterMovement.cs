using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public CameraLook cameraLook;
    public CharacterController characterController;
    public CharacterMovementConfig moveConfig;
    public Animator animator;
    
    public InputControls inputControls;

    [Tooltip("Если задан — горизонтальное движение и прыжок отключены на время удара киркой.")]
    public MineBlocks mineBlocks;

    private float _upwardVelocity;

    private bool _jumped;

    private bool _jumpIsBuffered;
    private float _jumpBufferTime;

    private bool _hasCoyote;
    private float _coyoteTime;

    private float _modelRotateTime;


    private void Update()
    {
        var value = inputControls.GetMoveDirection();

        cameraLook.GetHorizontalMoveAxes(out var forward, out var right);
        var moveVector = forward * value.y + right * value.x;
        if (mineBlocks != null && mineBlocks.IsMiningAttacking)
            moveVector = Vector3.zero;

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

        var miningBlocksMove = mineBlocks != null && mineBlocks.IsMiningAttacking;
        if (!miningBlocksMove && (characterController.isGrounded || _hasCoyote) && (inputControls.JumpIsPressed() || _jumpIsBuffered))
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

        if (!characterController.isGrounded && (inputControls.JumpedThisFrame()) && !_jumpIsBuffered)
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