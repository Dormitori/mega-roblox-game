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
    private float _currentCameraDistance;

    public void Awake()
    {
        _xRotation = cameraLookConfig.startXRotation;
        _yRotation = cameraLookConfig.startYRotation;
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

        UpdateDistance();

        UpdateCamera();
    }

    private void UpdateCamera()
    {
        var rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        camera.transform.position = orbitTarget.position - rotation * Vector3.forward * _currentCameraDistance;
        camera.transform.LookAt(orbitTarget);
    }

    private void UpdateDistance()
    {
        var direction = camera.transform.position - orbitTarget.position;
        if (Physics.Raycast(orbitTarget.transform.position, direction.normalized, out var hit, direction.magnitude + 3f,
                levelLayer))
        {
            _currentCameraDistance = hit.distance - 0.5f;
            return;
        }

        _currentCameraDistance = cameraLookConfig.cameraDistance;
    }

    private void UpdateCameraRotation()
    {
        var lookDelta = Mouse.current.delta.ReadValue();

        _xRotation -= lookDelta.y * cameraLookConfig.sensitivity * Time.deltaTime;
        _yRotation += lookDelta.x * cameraLookConfig.sensitivity * Time.deltaTime;

        _xRotation = Math.Clamp(_xRotation, cameraLookConfig.xMinClamp, cameraLookConfig.xMaxClamp);
    }
}