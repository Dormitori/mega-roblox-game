using UnityEngine;
public sealed class GroundedTrailVfxController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private ParticleSystem trailParticles;

    [Header("Tuning")]
    [SerializeField, Min(0f)] private float fadeSeconds = 0.15f;
    
    [SerializeField, Min(0f)] private float airborneGraceSeconds = 0.05f;

    private float _targetMul = 1f;
    private float _curMul = 1f;
    private float _airborneTime;

    private float _baseRateOverDistanceMul;
    private float _baseRateOverTimeMul;

    private void Reset()
    {
        characterController = GetComponentInParent<CharacterController>();
        if (trailParticles == null)
            trailParticles = GetComponentInChildren<ParticleSystem>(true);
    }

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();

        if (trailParticles == null)
            trailParticles = GetComponentInChildren<ParticleSystem>(true);

        if (trailParticles != null)
        {
            var emission = trailParticles.emission;
            _baseRateOverDistanceMul = emission.rateOverDistanceMultiplier;
            _baseRateOverTimeMul = emission.rateOverTimeMultiplier;
        }
    }

    private void Update()
    {
        if (characterController == null || trailParticles == null)
            return;

        var grounded = characterController.isGrounded;
        if (grounded)
        {
            _airborneTime = 0f;
            _targetMul = 1f;
        }
        else
        {
            _airborneTime += Time.deltaTime;
            if (_airborneTime >= airborneGraceSeconds)
                _targetMul = 0f;
        }

        var t = fadeSeconds <= 1e-4f ? 1f : Time.deltaTime / fadeSeconds;
        _curMul = Mathf.MoveTowards(_curMul, _targetMul, t);

        ApplyEmissionMultiplier(_curMul);
        
        if (_curMul <= 1e-3f)
        {
            if (trailParticles.isPlaying)
                trailParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        else
        {
            if (!trailParticles.isPlaying)
                trailParticles.Play(true);
        }
    }

    private void ApplyEmissionMultiplier(float mul)
    {
        var emission = trailParticles.emission;
        emission.rateOverDistanceMultiplier = _baseRateOverDistanceMul * mul;
        emission.rateOverTimeMultiplier = _baseRateOverTimeMul * mul;
    }
}

