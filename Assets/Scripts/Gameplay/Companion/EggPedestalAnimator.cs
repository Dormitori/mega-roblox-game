using UnityEngine;

public class EggPedestalAnimator : MonoBehaviour
{
    [SerializeField] private float tiltDegrees = 15f;
    [SerializeField] private float spinSpeed = 30f;
    [SerializeField] private float bobHeight = 0.10f;
    [SerializeField] private float bobFrequency = 0.8f;

    [SerializeField] private float hatchShakeDegrees = 20f;
    [SerializeField] private float hatchShakeFrequency = 5f;
    [SerializeField] private float hatchBobHeight = 0.20f;
    [SerializeField] private float hatchBobFrequency = 2.5f;

    private bool _hatching;
    private float _bobPhase;
    private float _spinOffset;
    private Vector3 _originLocalPos;

    private void Awake()
    {
        _bobPhase = Random.Range(0f, Mathf.PI * 2f);
        _spinOffset = Random.Range(0f, 360f);
        _originLocalPos = transform.localPosition;
    }

    public void SetHatching(bool hatching) => _hatching = hatching;

    private void Update()
    {
        float t = Time.time;
        float bH = _hatching ? hatchBobHeight : bobHeight;
        float bF = _hatching ? hatchBobFrequency : bobFrequency;

        Vector3 pos = _originLocalPos;
        pos.y += Mathf.Sin(t * bF * Mathf.PI * 2f + _bobPhase) * bH;
        transform.localPosition = pos;

        // Spin around world Y — the tilt precesses like a top
        float spin = (t * spinSpeed + _spinOffset) % 360f;
        Quaternion rot = Quaternion.AngleAxis(spin, Vector3.up)
                       * Quaternion.AngleAxis(tiltDegrees, Vector3.right);

        if (_hatching)
        {
            float shake = Mathf.Sin(t * hatchShakeFrequency * Mathf.PI * 2f + _bobPhase) * hatchShakeDegrees;
            rot *= Quaternion.Euler(shake, 0f, shake * 0.5f);
        }

        transform.localRotation = rot;
    }
}
