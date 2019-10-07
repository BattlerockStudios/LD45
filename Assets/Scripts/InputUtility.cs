using UnityEngine;

public static class InputUtility
{

    private static readonly bool s_useMobileControls =
#if UNITY_IOS || UNITY_ANDROID
        true
#else
        false
#endif
    ;

    private static Vector3? s_lastMousePosition = null;

    public static Vector2 GetDrag()
    {
        if (s_useMobileControls)
        {
            if (Input.touchCount == 0)
            {
                return Vector2.zero;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Moved)
            {
                return Vector2.zero;
            }

            return touch.deltaPosition;
        }
        else
        {
            if (!Input.GetMouseButton(0))
            {
                s_lastMousePosition = null;
                return Vector2.zero;
            }

            var currentMousePosition = Input.mousePosition;

            if(!s_lastMousePosition.HasValue)
            {
                s_lastMousePosition = currentMousePosition;
            }

            var delta = s_lastMousePosition.Value - currentMousePosition;
            s_lastMousePosition = currentMousePosition;

            return delta;
        }
    }

    public static bool DidTouchBegin()
    {
        if (s_useMobileControls)
        {
            if (Input.touchCount == 0)
            {
                return false;
            }

            return Input.GetTouch(0).phase == TouchPhase.Began;
        }
        else
        {
            return Input.GetMouseButtonDown(0);
        }
    }

}
