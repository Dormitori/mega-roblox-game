using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.Common
{
    public class SwingingAnimation : MonoBehaviour
    {
        [Header("Swing Settings")]
        [Tooltip("Maximum swing angle in degrees")]
        public float swingAngle = 15f;
        
        [Tooltip("Duration of one full swing (from center to one extreme)")]
        public float swingDuration = 1f;
        
        [Tooltip("Axis to rotate around (local space). Use (0,0,1) for Z-axis (2D), (1,0,0) for X-axis (3D hanging).")]
        public Vector3 swingAxis = Vector3.forward;
        
        [Tooltip("Easing curve for the swing motion")]
        public Ease swingEase = Ease.InOutSine;
        
        [Tooltip("Random delay between swings to make it look organic")]
        public float randomDelayRange = 0.2f;
        
        [Tooltip("Start swinging automatically when the object becomes active")]
        public bool autoStart = true;
        
        [Header("Target Transform")]
        [Tooltip("Transform to animate (usually the sign itself)")]
        public Transform targetTransform;
        
        private Coroutine swingCoroutine;
        private Vector3 initialLocalEuler;
        
        private void Awake()
        {
            if (targetTransform == null)
                targetTransform = transform;
                
            initialLocalEuler = targetTransform.localEulerAngles;
        }
        
        private void OnEnable()
        {
            if (autoStart)
                StartSwinging();
        }
        
        private void OnDisable()
        {
            StopSwinging();
            if (targetTransform != null)
                DOTween.Kill(targetTransform);
        }
        
        private void OnDestroy()
        {
            if (targetTransform != null)
                DOTween.Kill(targetTransform);
            StopSwinging();
        }
        
        public void StartSwinging()
        {
            if (swingCoroutine != null)
                StopCoroutine(swingCoroutine);
            swingCoroutine = StartCoroutine(SwingRoutine());
        }
        public void StopSwinging()
        {
            if (swingCoroutine != null)
            {
                StopCoroutine(swingCoroutine);
                swingCoroutine = null;
            }
        }
        
        private IEnumerator SwingRoutine()
        {
            if (targetTransform == null)
                yield break;
            
            targetTransform.localEulerAngles = initialLocalEuler;
            
            Vector3 targetRight = initialLocalEuler + swingAxis * swingAngle;
            Vector3 targetLeft  = initialLocalEuler - swingAxis * swingAngle;
            
            if (randomDelayRange > 0)
            {
                float initialDelay = Random.Range(0f, randomDelayRange);
                yield return new WaitForSeconds(initialDelay);
            }
            
            while (isActiveAndEnabled && targetTransform != null)
            {
                yield return targetTransform.DOLocalRotate(targetRight, swingDuration, RotateMode.Fast)
                    .SetEase(swingEase)
                    .WaitForCompletion();
                
                if (randomDelayRange > 0)
                {
                    float delay = Random.Range(0f, randomDelayRange);
                    yield return new WaitForSeconds(delay);
                }
                
                yield return targetTransform.DOLocalRotate(targetLeft, swingDuration, RotateMode.Fast)
                    .SetEase(swingEase)
                    .WaitForCompletion();
                
                if (randomDelayRange > 0)
                {
                    float delay = Random.Range(0f, randomDelayRange);
                    yield return new WaitForSeconds(delay);
                }
            }
        }
    }
}