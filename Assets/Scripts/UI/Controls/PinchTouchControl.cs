using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class PinchTouchControl : MonoBehaviour
{
    [NonSerialized]
    public float PinchDelta;
    
    public List<Transform> ignoredUIRoots = new();

    private readonly List<RaycastResult> _hits = new();
    private int _firstPinchTouchIdx, _secondPinchTouchIdx;

    private Dictionary<int, bool> _blockedTouches = new();
    private List<Touch> _pinchingTouches = new();
    private List<int> _tmpRemove = new();

    private void OnEnable() => EnhancedTouchSupport.Enable();
    private void OnDisable() => EnhancedTouchSupport.Disable();

    void Update()
    {
        var touches = Touch.activeTouches;

        foreach (var touch in touches.Where(touch => touch.phase == TouchPhase.Began))
            _blockedTouches[touch.touchId] = IsOverIgnoredUI(touch.screenPosition);

        _pinchingTouches.Clear();
        foreach (var touch in touches)
            if (_blockedTouches.ContainsKey(touch.touchId) && !_blockedTouches[touch.touchId])
                _pinchingTouches.Add(touch);

        var activeIds = new HashSet<int>(touches.Select(t => t.touchId));

        _tmpRemove.Clear();
        foreach (var id in _blockedTouches.Keys)
            if (!activeIds.Contains(id))
                _tmpRemove.Add(id);

        for (int i = 0; i < _tmpRemove.Count; i++)
            _blockedTouches.Remove(_tmpRemove[i]);
        
        if (_pinchingTouches.Count != 2)
        {
            PinchDelta = 0;
            return;
        }

        var t0 = _pinchingTouches[0];
        var t1 = _pinchingTouches[1];

        var prevDist = Vector2.Distance(t0.screenPosition - t0.delta, t1.screenPosition - t1.delta);
        var currDist = Vector2.Distance(t0.screenPosition, t1.screenPosition);
        PinchDelta = currDist - prevDist;
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
