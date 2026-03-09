using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    public Camera camera;
    public Transform orbitTarget;
    public CameraLookConfig cameraLookConfig;
    public LayerMask levelLayer;

    private bool _isLooking;

    private float _xRotation;
    private float _yRotation;
    private float _targetCameraDistance;
    private float _currentCameraDistance;
    private float _setCameraDistance;

    public void Awake()
    {
        _xRotation = cameraLookConfig.startXRotation;
        _yRotation = cameraLookConfig.startYRotation;
        _setCameraDistance = cameraLookConfig.cameraDistance;
        UpdateCamera();
    }

    public void LateUpdate()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            _isLooking = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            _isLooking = false;
            Cursor.lockState = CursorLockMode.None;
        }

        if (_isLooking)
        {
            UpdateCameraRotation();
        }

        HandleCameraScroll();

        UpdateDistance();

        UpdateCamera();
    }

    private void HandleCameraScroll()
    {
        if (Mathf.Abs(Mouse.current.scroll.ReadValue().y) > Mathf.Epsilon)
        {
            _setCameraDistance -= Mouse.current.scroll.ReadValue().y * cameraLookConfig.cameraStep;
            _setCameraDistance = Mathf.Clamp(_setCameraDistance, cameraLookConfig.cameraMinDistance,
                cameraLookConfig.cameraMaxDistance);
        }
    }

    private void UpdateCamera()
    {
        var rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        if (_targetCameraDistance < _setCameraDistance)
            _currentCameraDistance = _targetCameraDistance;
        else
            _currentCameraDistance = Mathf.Lerp(
                _currentCameraDistance, _targetCameraDistance, Time.deltaTime * cameraLookConfig.cameraAnimationSpeed
                );
        
        camera.transform.position = orbitTarget.position - rotation * Vector3.forward * _currentCameraDistance;
        camera.transform.LookAt(orbitTarget);
    }

    private void UpdateDistance()
    {
        var direction = camera.transform.position - orbitTarget.position;
        var isHit = Physics.Raycast(
            orbitTarget.transform.position,
            direction.normalized,
            out var hit, 
            direction.magnitude + 3f,
            levelLayer);
        if (isHit && hit.distance < _setCameraDistance)
        {
            _targetCameraDistance = hit.distance - 0.5f;
            return;
        }

        _targetCameraDistance = _setCameraDistance;
    }

    private void UpdateCameraRotation()
    {
        var lookDelta = Mouse.current.delta.ReadValue();

        _xRotation -= lookDelta.y * cameraLookConfig.sensitivity * Time.deltaTime;
        _yRotation += lookDelta.x * cameraLookConfig.sensitivity * Time.deltaTime;

        _xRotation = Math.Clamp(_xRotation, cameraLookConfig.xMinClamp, cameraLookConfig.xMaxClamp);
    }
}