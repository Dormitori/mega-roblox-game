using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.Common
{
    public class HammerAnimation : MonoBehaviour
    {
        [Header("Hammer Movement")]
        [Tooltip("Transform of the hammer (usually this object)")]
        public Transform hammerTransform;
        
        [Tooltip("Height the hammer lifts up (local Y offset from start position)")]
        public float liftHeight = 1f;
        
        [Tooltip("Duration of the lift movement (from start to top)")]
        public float liftDuration = 0.3f;
        
        [Tooltip("Duration of the strike movement (from top to anvil)")]
        public float strikeDuration = 0.2f;
        
        [Tooltip("Easing for lift (usually OutQuad or OutCubic for a quick start)")]
        public Ease liftEase = Ease.OutQuad;
        
        [Tooltip("Easing for strike (InQuad or InCubic for accelerating hit)")]
        public Ease strikeEase = Ease.InQuad;
        
        [Header("Impact Squash & Stretch")]
        [Tooltip("Scale multiplier on impact (e.g., 1.2 for squash/stretch)")]
        public float impactScale = 1.2f;
        
        [Tooltip("Duration of the squash effect (how long it takes to return to normal)")]
        public float squashDuration = 0.1f;
        
        [Tooltip("Easing for squash (usually OutBack for a bounce or OutQuad)")]
        public Ease squashEase = Ease.OutBack;
        
        [Header("Timing & Repeat")]
        [Tooltip("Automatically repeat strikes with a delay?")]
        public bool autoRepeat = false;
        
        [Tooltip("Delay between strikes if autoRepeat is true")]
        public float repeatDelay = 1f;
        
        [Tooltip("Random delay variation (adds randomness to repeat timing)")]
        public float randomDelayRange = 0.2f;
        
        [Tooltip("Start animating automatically when the object becomes active")]
        public bool autoStart = true;
        
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Coroutine strikeCoroutine;
        private bool isStriking = false;
        
        private void Awake()
        {
            if (hammerTransform == null)
                hammerTransform = transform;
                
            originalPosition = hammerTransform.localPosition;
            originalScale = hammerTransform.localScale;
        }
        
        private void OnEnable()
        {
            if (autoStart)
                StartStriking();
        }
        
        private void OnDisable()
        {
            StopStriking();
            // Kill any tweens on the hammer to avoid errors when disabled
            if (hammerTransform != null)
            {
                DOTween.Kill(hammerTransform);
                // Reset transform to original state
                hammerTransform.localPosition = originalPosition;
                hammerTransform.localScale = originalScale;
            }
        }
        
        private void OnDestroy()
        {
            if (hammerTransform != null)
                DOTween.Kill(hammerTransform);
            StopStriking();
        }
        
        /// <summary>
        /// Start the hammer striking coroutine (or restart if already running).
        /// </summary>
        public void StartStriking()
        {
            if (strikeCoroutine != null)
                StopCoroutine(strikeCoroutine);
            strikeCoroutine = StartCoroutine(StrikeRoutine());
        }
        
        /// <summary>
        /// Stop the hammer striking coroutine.
        /// </summary>
        public void StopStriking()
        {
            if (strikeCoroutine != null)
            {
                StopCoroutine(strikeCoroutine);
                strikeCoroutine = null;
            }
        }
        
        /// <summary>
        /// Perform a single strike (can be called from other scripts).
        /// Returns a coroutine that can be awaited (if needed).
        /// </summary>
        public IEnumerator SingleStrike()
        {
            yield return StrikeSequence();
        }
        
        private IEnumerator StrikeRoutine()
        {
            while (isActiveAndEnabled && hammerTransform != null)
            {
                yield return StrikeSequence();
                
                if (autoRepeat && isActiveAndEnabled)
                {
                    float delay = repeatDelay + Random.Range(0f, randomDelayRange);
                    yield return new WaitForSeconds(delay);
                }
                else
                {
                    break;
                }
            }
            
            strikeCoroutine = null;
        }
        
        private IEnumerator StrikeSequence()
        {
            if (hammerTransform == null) yield break;
            if (isStriking) yield break; // Prevent overlapping strikes
            
            isStriking = true;
            
            // Ensure we start from the original position and scale
            hammerTransform.localPosition = originalPosition;
            hammerTransform.localScale = originalScale;
            
            // Step 1: Lift the hammer up
            Vector3 liftedPosition = originalPosition + Vector3.up * liftHeight;
            yield return hammerTransform.DOLocalMove(liftedPosition, liftDuration)
                .SetEase(liftEase)
                .WaitForCompletion();
            
            // Step 2: Strike down to original position (impact)
            yield return hammerTransform.DOLocalMove(originalPosition, strikeDuration)
                .SetEase(strikeEase)
                .WaitForCompletion();
            
            // Step 3: Apply squash/stretch effect (scale up and back)
            // You can also add a slight Y offset to simulate bounce, but here we just scale
            Vector3 squashedScale = originalScale * impactScale;
            // Optionally add a small downward offset to sell impact
            // Vector3 impactOffset = originalPosition + Vector3.down * 0.05f;
            
            // Apply squash immediately (quick scale)
            Sequence squashSequence = DOTween.Sequence();
            squashSequence.Append(hammerTransform.DOScale(squashedScale, squashDuration * 0.5f).SetEase(squashEase));
            squashSequence.Append(hammerTransform.DOScale(originalScale, squashDuration * 0.5f).SetEase(Ease.OutQuad));
            // If you also want to bounce position, uncomment:
            // squashSequence.Join(hammerTransform.DOLocalMove(impactOffset, squashDuration * 0.5f).SetEase(Ease.OutQuad));
            // squashSequence.Append(hammerTransform.DOLocalMove(originalPosition, squashDuration * 0.5f).SetEase(Ease.InQuad));
            
            yield return squashSequence.WaitForCompletion();
            
            isStriking = false;
        }
        }
}