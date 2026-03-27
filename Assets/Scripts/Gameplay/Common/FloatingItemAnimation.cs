using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.Common
{
    public class FloatingItemAnimation : MonoBehaviour
    {
        [Header("Параметры движения")]
        public float bobbingAmount = 0.2f; 
        public float bobbingSpeed = 2f; 

        [Header("Параметры вращения")]
        public float tiltAmount = 10f;    
        public float tiltSpeed = 1.5f;

        private Vector3 _startPos;           
        private Quaternion _startRot;         
        private float _intensity = 1f;        
        private Tween _intensityTween;         
        private Coroutine _animationCoroutine; 

        void OnEnable()
        {
            _startPos = transform.position;
            _startRot = transform.rotation;
            _animationCoroutine = StartCoroutine(AnimationRoutine());
        }
        
        IEnumerator AnimationRoutine()
        {
            while (true)
            {
                if (_intensity > 0f)
                {
                    float time = Time.time;
                    
                    float yOffset = Mathf.Sin(time * bobbingSpeed) * bobbingAmount * _intensity;
                    transform.position = _startPos + new Vector3(0, yOffset, 0);
                    
                    float tiltAngle = Mathf.Sin(time * tiltSpeed) * tiltAmount * _intensity;
                    transform.rotation = _startRot * Quaternion.Euler(0, 0, tiltAngle);
                }
                yield return null;
            }
        }

        private void OnDisable()
        {
          
            if (_intensityTween != null) _intensityTween.Kill();
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        }
    }
}