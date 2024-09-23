using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gemity.GemiTouch
{
    public enum TouchType
    {
        Mouse = 1, Finger = 2
    }
    public enum TouchPhase
    {
        Began, Moved, Stationary, Ended, Canceled
    }

    public class GemiTouch : MonoBehaviour
    {
        private const int MOUSE_FINGER_INDEX = -1;
        private const int DEFAULT_REFERENCE_DPI = 200;
        private const int DEFAULT_GUI_LAYERS = 1 << 5;
        private const float DEFAULT_TAP_THRESHOLD = 0.2f;
        private const float DEFAULT_SWIPE_THRESHOLD = 100.0f;
        private const float DEFAULT_RECORD_LIMIT = 10.0f;

        private static GemiTouch _instance;
        private static List<RaycastResult> tempRaycastResults = new List<RaycastResult>(10);
        private static PointerEventData tempPointerEventData;
        private static EventSystem tempEventSystem;
        private static TouchType Type;

        public static bool EnableFinger { get; set; }

        public static event Action<GemiFinger> OnFingerDown;
        public static event Action<GemiFinger> OnFingerUp;
        public static event Action<GemiFinger> OnFingerSwipe;
        public static event Action<GemiFinger> OnFingerTap;
        public static event Action<GemiFinger> OnFingerDoubleTap;

        private static List<GemiFinger> _fingers = new();
        private Camera _camera;

        static GemiTouch()
        {
            _instance = new GameObject("GemiTouch", typeof(GemiTouch)).GetComponent<GemiTouch>();
            DontDestroyOnLoad(_instance.gameObject);
        }

        public Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = Camera.main;

                return _camera;
            }
        }

        private void Awake()
        {
            if (Input.touchSupported)
                Type = TouchType.Finger;
            else
                Type = TouchType.Mouse;

#if UNITY_WEG_GL
            Type = TouchType.Mouse;
#endif
        }

        private void Update()
        {
            BeginFingers();
            UpdateFingers();
        }

        private void BeginFingers()
        {
            for (int i = _fingers.Count - 1; i >= 0; i--)
            {
                GemiFinger finger = _fingers[i];
                if (finger.Phase == TouchPhase.Canceled)
                    ReleaseFinger(finger);
                else
                {
                    finger.Timer += Time.deltaTime;
                    if (finger.Phase == TouchPhase.Ended && finger.Timer > DEFAULT_TAP_THRESHOLD)
                    {
                        if (!OverSwipeThreshold(finger.LastScreenPosition, finger.StartScreenPosition))
                            OnFingerTap?.Invoke(finger);
                        finger.ChangePhase(TouchPhase.Canceled);
                    }
                }
            }
        }

        private void UpdateFingers()
        {
            if (!EnableFinger)
            {
                _fingers.ForEach(finger => finger.Release());
                _fingers.Clear();
                return;
            }

            if(Type == TouchType.Finger)
            {
                foreach(var touch in Input.touches)
                {
                    if(touch.phase == UnityEngine.TouchPhase.Began)
                    {
                        FingerDownHandle(touch.fingerId, touch.position);
                    }
                    else if (touch.phase == UnityEngine.TouchPhase.Moved || touch.phase == UnityEngine.TouchPhase.Stationary)
                    {
                        var finger = FindActiveFingerById(touch.fingerId);
                        finger.LastScreenPosition = finger.CurrentScreenPosition;
                        finger.CurrentScreenPosition = touch.position;
                        finger.ChangePhase((TouchPhase)touch.phase);
                    }
                    else if(touch.phase == UnityEngine.TouchPhase.Ended)
                    {
                        var finger = FindActiveFingerById(touch.fingerId);
                        finger.CurrentScreenPosition = finger.LastScreenPosition = touch.position;
                        finger.ChangePhase(TouchPhase.Ended);
                        OnFingerUp?.Invoke(finger);

                        var doubleTapFinger = FindDoubleTapFinger(touch.position);
                        if(doubleTapFinger != null) 
                        {
                            OnFingerDoubleTap?.Invoke(doubleTapFinger);
                            ReleaseFinger(doubleTapFinger);
                            ReleaseFinger(finger);
                        }
                    }
                }
            }
            else if(Type == TouchType.Mouse)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    FingerDownHandle(0, Input.mousePosition);
                }
                else if (Input.GetMouseButton(0))
                {
                    var finger = FindActiveFingerById(0);
                    finger.LastScreenPosition = finger.CurrentScreenPosition;
                    finger.CurrentScreenPosition = Input.mousePosition;

                    TouchPhase phase = OverSwipeThreshold(finger.LastScreenPosition, finger.CurrentScreenPosition)? TouchPhase.Stationary : TouchPhase.Moved;
                    finger.ChangePhase(phase);
                }
                else if(Input.GetMouseButtonUp(0))
                {
                    var finger = FindActiveFingerById(0);
                    finger.CurrentScreenPosition = finger.LastScreenPosition = Input.mousePosition;
                    finger.ChangePhase(TouchPhase.Ended);
                    OnFingerUp?.Invoke(finger);

                    var doubleTapFinger = FindDoubleTapFinger(Input.mousePosition);
                    if (doubleTapFinger != null)
                    {
                        OnFingerDoubleTap?.Invoke(finger);
                        ReleaseFinger(finger);
                        ReleaseFinger(doubleTapFinger);
                    }
                }
            }
        }

        private void FingerDownHandle(int id, Vector2 screenPosition)
        {
            var finger = GemiFinger.Pop();

            finger.Id = id;
            finger.BeganFinger(screenPosition);
            OnFingerDown?.Invoke(finger);

            _fingers.Add(finger);
        }

        private GemiFinger FindDoubleTapFinger(Vector2 screenPosition)
        {
            return _fingers.FirstOrDefault(x => x.Phase == TouchPhase.Ended && x.Timer > 0f && !OverSwipeThreshold(x.StartScreenPosition, screenPosition));
        }

        private void ReleaseFinger(GemiFinger finger)
        {
            _fingers.Remove(finger);
            finger.Release();
        }

        private static GemiFinger FindActiveFingerById(int id)
        {
            return _fingers.FirstOrDefault(x => x.Id == id && x.Phase != TouchPhase.Ended && x.Phase != TouchPhase.Canceled);
        }

        private bool OverSwipeThreshold(Vector2 x, Vector2 y) => Vector2.SqrMagnitude(x - y) > DEFAULT_SWIPE_THRESHOLD * DEFAULT_SWIPE_THRESHOLD;

        internal static bool PointOverGui(Vector2 screenPosition)
        {
            return RaycastGui(screenPosition, (LayerMask)DEFAULT_GUI_LAYERS).Count > 0;
        }

        private static List<RaycastResult> RaycastGui(Vector2 screenPosition, LayerMask layerMask)
        {
            tempRaycastResults.Clear();

            var currentEventSystem = EventSystem.current;
            if (currentEventSystem != null)
            {
                if (currentEventSystem != tempEventSystem)
                {
                    tempEventSystem = currentEventSystem;
                    if (tempPointerEventData == null)
                        tempPointerEventData = new PointerEventData(tempEventSystem);
                    else
                        tempPointerEventData.Reset();
                }

                tempPointerEventData.position = screenPosition;
                currentEventSystem.RaycastAll(tempPointerEventData, tempRaycastResults);
                if (tempRaycastResults.Count > 0)
                {
                    for (var i = tempRaycastResults.Count - 1; i >= 0; i--)
                    {
                        var raycastResult = tempRaycastResults[i];
                        var raycastLayer = 1 << raycastResult.gameObject.layer;

                        if ((raycastLayer & layerMask) == 0)
                        {
                            tempRaycastResults.RemoveAt(i);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to RaycastGui because your scene doesn't have an event system! To add one, go to: GameObject/UI/EventSystem");
            }

            return tempRaycastResults;
        }

        /// <summary>If you multiply this value with any other pixel delta (e.g. ScreenDelta), then it will become device resolution independent relative to the device DPI.</summary>
        public static float ScalingFactor
        {
            get
            {
                var dpi = Screen.dpi;
                return dpi <= 0 ? 1f : DEFAULT_REFERENCE_DPI / dpi;
            }
        }

        /// <summary>If you multiply this value with any other pixel delta (e.g. ScreenDelta), then it will become device resolution independent relative to the screen pixel size.</summary>
        public static float ScreenFactor
        {
            get
            {
                var size = Mathf.Min(Screen.width, Screen.height);
                return size <= 0 ? 1f : 1f / size;
            }
        }

        public static bool FingerDownThisFrame(bool startOverGui = false)
        {
            return _fingers.Where(x => x.Phase == TouchPhase.Began && (startOverGui || !x.StartOverGui)).Count() > 0;
        }

        public static bool FingerHoldThisFrame(bool startOverGui = false)
        {
            return _fingers.Where(x => x.Phase == TouchPhase.Stationary || x.Phase == TouchPhase.Moved && (startOverGui || !x.StartOverGui)).Count() > 0;
        }

        public static bool FingerUpThisFrame(bool startOverGui = false)
        {
            return _fingers.Where(x => x.Phase == TouchPhase.Ended && x.Timer == 0 && (startOverGui || !x.StartOverGui)).Count() > 0;
        }

        public Vector2 ScreenSwipeDelta
        {
            get
            {
                var finger = _fingers.Where(x => x.Phase == TouchPhase.Moved).OrderByDescending(x => x.Timer).Last();
                return finger.CurrentScreenPosition - finger.LastScreenPosition;
            }
        }
    }
}