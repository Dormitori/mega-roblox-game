using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class CameraLook : MonoBehaviour
{
    public Camera playerCamera;
    public Transform orbitTarget;
    public CameraLookConfig cameraLookConfig;
    public InputControls inputControls;
    public List<LayerMask> obstacleLayers;

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
        _targetCameraDistance = _setCameraDistance;
        _currentCameraDistance = _setCameraDistance;
        ApplyCameraTransform();
    }

    public void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (inputControls.IsLooking())
        {
            UpdateCameraRotation();
        }

        HandleCameraScroll();

        UpdateDistance();

        LerpCameraDistance();
    }

    private void LateUpdate()
    {
        ApplyCameraTransform();
    }

    private void HandleCameraScroll()
    {
        var zoomValue = inputControls.GetCameraZoomValue();
        if (Mathf.Abs(zoomValue) > Mathf.Epsilon)
        {
            _setCameraDistance -= zoomValue;
            _setCameraDistance = Mathf.Clamp(_setCameraDistance, cameraLookConfig.cameraMinDistance,
                cameraLookConfig.cameraMaxDistance);
        }
    }

    private void LerpCameraDistance()
    {
        if (_targetCameraDistance < _setCameraDistance)
            _currentCameraDistance = _targetCameraDistance;
        else
            _currentCameraDistance = Mathf.Lerp(
                _currentCameraDistance, _targetCameraDistance, Time.deltaTime * cameraLookConfig.cameraAnimationSpeed
            );
    }

    private void ApplyCameraTransform()
    {
        var rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        playerCamera.transform.position = orbitTarget.position - rotation * Vector3.forward * _currentCameraDistance;
        playerCamera.transform.LookAt(orbitTarget);
    }

    private void UpdateDistance()
    {
        var rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        
        var direction = -(rotation * Vector3.forward);
        var maxDistance = _setCameraDistance + 3f;
        foreach (var layer in obstacleLayers)
        {
            var isHit = Physics.Raycast(
                orbitTarget.position,
                direction,
                out var hit,
                maxDistance,
                layer);
            if (isHit && hit.distance < _setCameraDistance)
            {
                _targetCameraDistance = hit.distance - 0.5f;
                return;
            }
        }

        _targetCameraDistance = _setCameraDistance;
    }
    
    public void GetHorizontalMoveAxes(out Vector3 forward, out Vector3 right)
    {
        var raw = Quaternion.Euler(_xRotation, _yRotation, 0f) * Vector3.forward;
        forward = new Vector3(raw.x, 0f, raw.z);
        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.forward;
        else
            forward.Normalize();

        right = Quaternion.Euler(0f, 90f, 0f) * forward;
    }

    private void UpdateCameraRotation()
    {
        var lookDelta = inputControls.GetLookDelta();

        _xRotation -= lookDelta.y * Time.deltaTime;
        _yRotation += lookDelta.x * Time.deltaTime;

        _xRotation = Math.Clamp(_xRotation, cameraLookConfig.xMinClamp, cameraLookConfig.xMaxClamp);
    }
}