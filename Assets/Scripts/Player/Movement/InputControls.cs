using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputControls : MonoBehaviour
{
    public GameObject controlsPanel;
    
    public ControlsButton jumpButton;
    
    public CameraTouchControl cameraTouchControl;
    public PinchTouchControl pinchTouchControl;
    public CameraLookConfig cameraLookConfig;
    public bool usePhone;
    
    public Joystick joystick;
    
    private InputAction _jumpAction;
    private InputAction _moveAction;

    private bool _isPhone;

    private void Awake()
    {
        _isPhone = Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform || usePhone;

        if (!_isPhone)
            controlsPanel.SetActive(false);
        
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _moveAction = InputSystem.actions.FindAction("Move");
    }

    public bool JumpedThisFrame()
    {
        if (!_isPhone)
            return _jumpAction.WasPressedThisFrame();
        return jumpButton.WasPressedThisFrame;
    }
    
    public bool JumpIsPressed()
    {
        if (!_isPhone)
            return _jumpAction.IsPressed();
        return jumpButton.IsPressed;
    }

    public Vector2 GetMoveDirection()
    {
        if (!_isPhone)
            return _moveAction.ReadValue<Vector2>();
        return joystick.Direction;
    }

    public Vector2 GetLookDelta()
    {
        if (_isPhone)
            return cameraTouchControl.CameraDelta * cameraLookConfig.touchSensitivity;
        return Mouse.current.delta.ReadValue() * cameraLookConfig.mouseSensitivity;
    }

    public bool IsLooking()
    {
        if (_isPhone)
            return cameraTouchControl.IsCameraTouchActive;
        return Mouse.current.rightButton.isPressed;
    }

    public float GetCameraZoomValue()
    {
        if (_isPhone)
            return pinchTouchControl.PinchDelta * cameraLookConfig.cameraScrollMobileStep;
        return Mouse.current.scroll.ReadValue().y * cameraLookConfig.cameraScrollWheelStep;
    }

    public bool MineIsPressed()
    {
        if (_isPhone)
            return cameraTouchControl.IsTouchedThisFrame;
        if (EventSystem.current.IsPointerOverGameObject())
            return false;
        return Mouse.current.leftButton.isPressed;
    }
}