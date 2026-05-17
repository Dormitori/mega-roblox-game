using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class EggTimerLabel : MonoBehaviour
{
    private EggHatchingService _hatchingService;
    private TextMeshPro _text;
    private Camera _camera;
    private int _slotIndex;

    public void Setup(EggHatchingService service, int slotIndex)
    {
        _hatchingService = service;
        _slotIndex = slotIndex;
    }

    private void Start()
    {
        _text = GetComponent<TextMeshPro>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_camera != null)
            transform.LookAt(
                transform.position + _camera.transform.rotation * Vector3.forward,
                _camera.transform.rotation * Vector3.up);

        if (_hatchingService == null || _hatchingService.IsSlotEmpty(_slotIndex))
        {
            _text.enabled = false;
            return;
        }

        _text.enabled = true;

        if (_hatchingService.IsHatched(_slotIndex))
        {
            _text.text = "Ready!";
            _text.color = Color.green;
            return;
        }

        _text.color = Color.white;
        _text.text = FormatTime(_hatchingService.GetRemainingSeconds(_slotIndex));
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.CeilToInt(seconds);
        int h = total / 3600;
        int m = (total % 3600) / 60;
        int s = total % 60;
        return h > 0 ? $"{h}:{m:00}:{s:00}" : $"{m}:{s:00}";
    }
}
