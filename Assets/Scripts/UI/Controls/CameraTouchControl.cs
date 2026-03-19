using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

[DefaultExecutionOrder(-1)]
public class CameraTouchControl : MonoBehaviour
{
    public Vector2 CameraDelta { get; private set; }
    public bool IsCameraTouchActive { get; private set; }
    public bool IsTouchedThisFrame { get; private set; }

    public List<Transform> ignoredUIRoots = new();

    private readonly List<RaycastResult> _hits = new();
    private Dictionary<int, bool> _blockedTouches = new();
    private List<Touch> _cameraMoveTouches = new();
    private List<int> _tmpRemove = new();
    
    private readonly float _touchTimeThreshold = 0.5f;
    private float _touchTime = 0f;
    
    private readonly float _touchMoveDeltaThreshold = 10f;
    private float _touchCurDelta = 0;
   
    
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        IsTouchedThisFrame = false;
        
        CameraDelta = Vector2.zero;

        var touches = Touch.activeTouches;

        foreach (var touch in touches.Where(touch => touch.phase == TouchPhase.Began))
            _blockedTouches[touch.touchId] = IsOverIgnoredUI(touch.screenPosition);

        _cameraMoveTouches.Clear();
        foreach (var touch in touches)
            if (_blockedTouches.ContainsKey(touch.touchId) && !_blockedTouches[touch.touchId])
                _cameraMoveTouches.Add(touch);

        var activeIds = new HashSet<int>(touches.Select(t => t.touchId));

        _tmpRemove.Clear();
        foreach (var id in _blockedTouches.Keys.Where(id => !activeIds.Contains(id)))
            _tmpRemove.Add(id);

        foreach (var remove in _tmpRemove)
            _blockedTouches.Remove(remove);

        if (_cameraMoveTouches.Count != 1)
        {
            _touchTime = 0f;
            _touchCurDelta = 0f;
            return;
        }

        var camTouch = _cameraMoveTouches[0];
        _touchTime += Time.deltaTime;

        if (camTouch.phase == TouchPhase.Ended && _touchTime < _touchTimeThreshold &&
            _touchCurDelta < _touchMoveDeltaThreshold)
            IsTouchedThisFrame = true;

        IsCameraTouchActive = true;

        if (camTouch.phase == TouchPhase.Moved)
        {
            var d = camTouch.delta;
            _touchCurDelta += d.magnitude;
            CameraDelta = d;
        }
    }

    private bool IsOverIgnoredUI(Vector2 screenPos)
    {
        if (!EventSystem.current) return false;

        var ped = new PointerEventData(EventSystem.current) { position = screenPos };
        _hits.Clear();
        EventSystem.current.RaycastAll(ped, _hits);

        foreach (var hit in _hits)
        {
            var go = hit.gameObject;
            if (!go) continue;

            var t = go.transform;

            foreach (var root in ignoredUIRoots)
                if (root && t.IsChildOf(root))
                    return true;
        }

        return false;
    }
}