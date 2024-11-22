// This code is part of the SS-Scene library, released by Anh Pham (anhpt.csit@gmail.com).

using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;

namespace SS.View
{
    public class SceneDefaultAnimation : SceneAnimation
    {
        #region Enum

        protected enum State
        {
            IDLE,
            SHOW,
            HIDE
        }

        protected enum AnimationType
        {
            None,
            Fade,
            Scale,
            SlideFromBottom,
            SlideFromTop,
            SlideFromLeft,
            SlideFromRight,
        }

        #endregion

        #region SerializeField

        [SerializeField] AnimationType m_AnimationType = AnimationType.SlideFromRight;
        [SerializeField] EaseType m_ShowEaseType = EaseType.easeInOutExpo;
        [SerializeField] EaseType m_HideEaseType = EaseType.easeInOutExpo;

        #endregion

        #region Private Variable

        protected Vector2 m_Start;
        protected Vector2 m_End;
        protected RectTransform m_RectTransform;
        protected RectTransform m_CanvasRectTransform;
        protected CanvasGroup m_CanvasGroup;
        protected State m_State = State.IDLE;
        protected EasingFunction m_ShowEase;
        protected EasingFunction m_HideEase;

        #endregion

        public override void Init()
        {
            if (Manager.SceneAnimationDuration > 0)
            {
                _animationDuration = Manager.SceneAnimationDuration;
            }
            else
            {
                _animationDuration = 0.3f;
            }

            m_ShowEase = GetEasingFunction(m_ShowEaseType);
            m_HideEase = GetEasingFunction(m_HideEaseType);
        }

        RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }

                return m_RectTransform;
            }
        }

        CanvasGroup CanvasGroup
        {
            get
            {
                if (m_CanvasGroup == null)
                {
                    m_CanvasGroup = GetComponent<CanvasGroup>();
                }

                return m_CanvasGroup;
            }
        }

        RectTransform CanvasRectTransform
        {
            get
            {
                if (m_CanvasRectTransform == null)
                {
                    Transform p = transform.parent;
                    while (p != null)
                    {
                        if (p.GetComponent<Canvas>() != null)
                        {
                            m_CanvasRectTransform = p.GetComponent<RectTransform>();
                            break;
                        }
                        p = p.parent;
                    }
                }

                return m_CanvasRectTransform;
            }
        }

        public override void HideBeforeShowing()
        {
            switch (m_AnimationType)
            {
                case AnimationType.Fade:
                    CanvasGroup.alpha = 0;
                    break;
                case AnimationType.Scale:
                    RectTransform.localScale = Vector3.zero;
                    break;
                case AnimationType.SlideFromBottom:
                    RectTransform.anchoredPosition = new Vector2(0, -ScreenHeight());
                    break;
                case AnimationType.SlideFromLeft:
                    RectTransform.anchoredPosition = new Vector2(-ScreenWidth(), RectTransform.anchoredPosition.y);
                    break;
                case AnimationType.SlideFromRight:
                    RectTransform.anchoredPosition = new Vector2(ScreenWidth(), RectTransform.anchoredPosition.y);
                    break;
                case AnimationType.SlideFromTop:
                    RectTransform.anchoredPosition = new Vector2(0, ScreenHeight());
                    break;
            }
        }

        public override async UniTask Show()
        {
            if (m_AnimationType == AnimationType.None)
                return;

            switch (m_AnimationType)
            {
                case AnimationType.SlideFromBottom:
                    m_Start = new Vector2(0, -ScreenHeight());
                    m_End = Vector2.zero;
                    break;
                case AnimationType.SlideFromTop:
                    m_Start = new Vector2(0, ScreenHeight());
                    m_End = Vector2.zero;
                    break;
                case AnimationType.SlideFromRight:
                    m_Start = new Vector2(ScreenWidth(), RectTransform.anchoredPosition.y);
                    m_End = new Vector2(0, RectTransform.anchoredPosition.y);
                    break;
                case AnimationType.SlideFromLeft:
                    m_Start = new Vector2(-ScreenWidth(), RectTransform.anchoredPosition.y);
                    m_End = new Vector2(0, RectTransform.anchoredPosition.y);
                    break;
                case AnimationType.Scale:
                    m_Start = Vector2.zero;
                    m_End = Vector2.one;
                    break;
                case AnimationType.Fade:
                    m_Start = Vector2.zero;
                    m_End = Vector2.one;
                    break;
            }

            m_State = State.SHOW;
            await Play();
        }

        public override async UniTask Hide()
        {
            if (m_AnimationType == AnimationType.None)
                return;

            switch (m_AnimationType)
            {
                case AnimationType.SlideFromBottom:
                    m_Start = Vector2.zero;
                    m_End = new Vector2(0, -ScreenHeight());
                    break;
                case AnimationType.SlideFromTop:
                    m_Start = Vector2.zero;
                    m_End = new Vector2(0, ScreenHeight());
                    break;
                case AnimationType.SlideFromRight:
                    m_Start = new Vector2(0, RectTransform.anchoredPosition.y);
                    m_End = new Vector2(ScreenWidth(), RectTransform.anchoredPosition.y);
                    break;
                case AnimationType.SlideFromLeft:
                    m_Start = new Vector2(0, RectTransform.anchoredPosition.y);
                    m_End = new Vector2(-ScreenWidth(), RectTransform.anchoredPosition.y);
                    break;
                case AnimationType.Scale:
                    m_Start = Vector2.one;
                    m_End = Vector2.zero;
                    break;
                case AnimationType.Fade:
                    m_Start = Vector2.one;
                    m_End = Vector2.zero;
                    break;
            }

            m_State = State.HIDE;
            await Play();
        }

        protected override void ApplyProgress(float progress)
        {
            EasingFunction ease = (m_State == State.SHOW) ? m_ShowEase : m_HideEase;

            switch (m_AnimationType)
            {
                case AnimationType.Scale:
                    RectTransform.localScale = new Vector3(ease(m_Start.x, m_End.x, progress), ease(m_Start.y, m_End.y, progress), 1);
                    break;
                case AnimationType.Fade:
                    CanvasGroup.alpha = ease(m_Start.x, m_End.x, progress);
                    break;
                default:
                    float x = ease(m_Start.x, m_End.x, progress);
                    float y = ease(m_Start.y, m_End.y, progress);
                    RectTransform.anchoredPosition = new Vector2(x, y);
                    break;
            }
        }

        private float ScreenHeight()
        {
            if (CanvasRectTransform != null)
            {
                return CanvasRectTransform.sizeDelta.y;
            }
            return Screen.height;
        }

        private float ScreenWidth()
        {
            if (CanvasRectTransform != null)
            {
                return CanvasRectTransform.sizeDelta.x;
            }
            return Screen.width;
        }
    }
}