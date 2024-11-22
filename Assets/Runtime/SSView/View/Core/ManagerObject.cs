using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

namespace SS.View
{
    public class ManagerObject : Tweener
    {
        public enum State
        {
            SHIELD_OFF,
            SHIELD_ON,
            SHIELD_FADE_IN,
            SHIELD_FADE_OUT,
            SCENE_LOADING
        }

        // Commons
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Camera _uiCamera;

        // Shield & Transition
        [SerializeField] private Image _shield;
        [SerializeField] private EaseType _fadeInEaseType = EaseType.easeInOutExpo;
        [SerializeField] private EaseType _fadeOutEaseType = EaseType.easeInOutExpo;
        [SerializeField] private Color _shieldColor = Color.black;

        private State _state;
        public State GetState() { return _state; }

        private EasingFunction _fadeInEase;
        private EasingFunction _fadeOutEase;

        public Camera UICamera => _uiCamera;

        #region Shield & Transition methods
        public void ShieldOff()
        {
            _state = State.SHIELD_OFF;
            ActiveShield = false;
        }

        public void ShieldOn(Color c = default)
        {
            _state = State.SHIELD_ON;
            ActiveShield = true;

            _shield.color = c;
        }

        public async UniTask FadeInScene()
        {
            if (Manager.SceneFadeDuration == 0)
                ShieldOff();
            else
            {
                ActiveShield = true;

                _animationDuration = Manager.SceneFadeDuration;
                _state = State.SHIELD_FADE_IN;
                await Play();

                ShieldOff();
            }
        }

        public async UniTask FadeOutScene()
        {
            if (Manager.SceneFadeDuration == 0)
                ShieldOff();
            else
            {
                ActiveShield = true;

                _animationDuration = Manager.SceneFadeDuration;
                _state = State.SHIELD_FADE_OUT;
                await Play();

                ShieldOff();
                _state = State.SCENE_LOADING;
            }
        }

        public bool ActiveShield
        {
            get =>  _state != State.SHIELD_OFF;
            protected set => _shield.gameObject.SetActive(value);
        }

        protected override void ApplyProgress(float progress)
        {
            EasingFunction ease;
            float start, end;

            if (_state == State.SHIELD_FADE_IN)
            {
                ease = _fadeInEase;
                start = 1;
                end = 0;
            }
            else
            {
                ease = _fadeOutEase;
                start = 0;
                end = 1;
            }

            Color color = _shieldColor;
            color.a = ease(start, end, progress);
            _shield.color = color;
        }
        #endregion

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject go = new GameObject("EventSystem");
                eventSystem = go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
            }
            DontDestroyOnLoad(eventSystem.gameObject);

            _fadeInEase = GetEasingFunction(_fadeInEaseType);
            _fadeOutEase = GetEasingFunction(_fadeOutEaseType);
        }

        private IEnumerator Start()
        {
            yield return 0;

            if (EventSystem.current != null)
            {
                int defaultValue = EventSystem.current.pixelDragThreshold;
                EventSystem.current.pixelDragThreshold = Mathf.Max(defaultValue, (int)(defaultValue * Screen.dpi / 160f));
            }

            if (_canvas.GetComponent<Canvas>().worldCamera == null)
                _canvas.GetComponent<Canvas>().worldCamera = this.UICamera;

            yield return 0;
            ShieldOff();
        }
    }
}