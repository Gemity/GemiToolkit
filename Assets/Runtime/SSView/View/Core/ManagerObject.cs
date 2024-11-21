using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        [SerializeField] EaseType _fadeOutEaseType = EaseType.easeInOutExpo;
        [SerializeField] private Color _shieldColor = Color.black;

        // Shield & Transition Vars
        bool m_Active;
        State m_State;
        public State GetState() { return m_State; }

        EasingFunction m_FadeInEase;
        EasingFunction m_FadeOutEase;

        float m_StartAlpha;
        float m_EndAlpha;
        public Camera UICamera => _uiCamera;

        #region Shield & Transition methods
        public void ShieldOff()
        {
            if (m_State == State.SHIELD_ON)
            {
                m_State = State.SHIELD_OFF;
                Active = false;
            }
        }

        public void ShieldOn()
        {
            if (m_State == State.SHIELD_OFF)
            {
                m_State = State.SHIELD_ON;
                Active = true;

                _shield.color = Color.clear;
            }
        }

        public void ShieldOnColor()
        {
            if (m_State == State.SHIELD_OFF)
            {
                m_State = State.SHIELD_ON;
                Active = true;

                _shield.color = _shieldColor;
            }
        }

        // Scene gradually appear
        public void FadeInScene()
        {
            if (this != null)
            {
                if (Manager.SceneFadeDuration == 0)
                {
                    ShieldOff();
                }
                else
                {
                    Active = true;

                    m_StartAlpha = 1;
                    m_EndAlpha = 0;

                    this.m_AnimationDuration = Manager.SceneFadeDuration;
                    this.Play();

                    m_State = State.SHIELD_FADE_IN;
                }
            }
        }

        // Scene gradually disappear
        public void FadeOutScene()
        {
            if (this != null)
            {
                if (Manager.SceneFadeDuration == 0)
                {
                    OnFadedOut();
                    ShieldOn();
                }
                else
                {
                    Active = true;

                    m_StartAlpha = 0;
                    m_EndAlpha = 1;

                    this.m_AnimationDuration = Manager.SceneFadeDuration;
                    this.Play();

                    m_State = State.SHIELD_FADE_OUT;
                }
            }
        }

        public void OnFadedIn()
        {
            if (this != null)
            {
                m_State = State.SHIELD_OFF;
                Active = false;
                Manager.EndFadedIn();
            }
        }

        public void OnFadedOut()
        {
            m_State = State.SCENE_LOADING;
            Manager.EndFadedOut();
        }

        public bool Active
        {
            get
            {
                return m_Active;
            }
            protected set
            {
                m_Active = value;
                _shield.gameObject.SetActive(m_Active);
            }
        }

        protected override void ApplyProgress(float progress)
        {
            EasingFunction ease = (m_State == State.SHIELD_FADE_IN) ? m_FadeInEase : m_FadeOutEase;

            Color color = _shieldColor;
            color.a = ease(m_StartAlpha, m_EndAlpha, progress);

            _shield.color = color;
        }

        protected override void OnEndAnimation()
        {
            switch (m_State)
            {
                case State.SHIELD_FADE_IN:
                    OnFadedIn();
                    break;
                case State.SHIELD_FADE_OUT:
                    OnFadedOut();
                    break;
            }
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

            m_FadeInEase = GetEasingFunction(_fadeInEaseType);
            m_FadeOutEase = GetEasingFunction(_fadeOutEaseType);
        }

        IEnumerator Start()
        {
            yield return 0;

            if (EventSystem.current != null)
            {
                int defaultValue = EventSystem.current.pixelDragThreshold;
                EventSystem.current.pixelDragThreshold = Mathf.Max(defaultValue, (int)(defaultValue * Screen.dpi / 160f));
            }

            if (_canvas.GetComponent<Canvas>().worldCamera == null)
                _canvas.GetComponent<Canvas>().worldCamera = this.UICamera;

            var canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode != RenderMode.WorldSpace && canvas.GetComponent<DontChangeCanvasCamera>() == null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = this.UICamera;
                }
            }

            yield return 0;
            ShieldOff();
        }
    }
}