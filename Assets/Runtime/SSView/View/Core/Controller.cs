using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SS.View
{
    public abstract class Controller : MonoBehaviour
    {
        [SerializeField] protected Canvas _canvas;

        [SerializeField] protected Camera _camera;

        /// <summary>
        /// When you popup a fullscreen view using SceneManager.Popup(), the system will automatically deactivate the view under it ( for better performance ).
        /// When you close it using SceneManager.Close(), the view which was under it will be activated.
        /// </summary>
        [SerializeField]
        public bool FullScreen;

        /// <summary>
        /// The animation.
        /// </summary>
        [SerializeField]
        public SceneAnimation Animation;

        /// <summary>
        /// Each scene must has an unique scene name.
        /// </summary>
        /// <returns>The name.</returns>
        public abstract string SceneName();

        /// <summary>
        /// This event is raised right after this view becomes active after a call of LoadScene() or ReloadScene() or Popup() of SceneManager.
        /// Same OnEnable but included the data which is transfered from the previous view. (Raised after Awake() and OnEnable())
        /// </summary>
        /// <param name="data">Data.</param>
        public virtual void OnActive(object data)
        {
        }

        /// <summary>
        /// This event is raised right after the above view is hidden.
        /// </summary>
        public virtual void OnReFocus()
        {
        }

        /// <summary>
        /// This event is raised right after this view appears and finishes its show-animation.
        /// </summary>
        public virtual void OnShown()
        {
        }

        /// <summary>
        /// This event is raised right after this view finishes its hide-animation and disappears.
        /// </summary>
        public virtual void OnHidden()
        {
        }

        /// <summary>
        /// This event is raised right after player pushs the ESC button on keyboard or Back button on android devices.
        /// You should assign this method to OnClick event of your Close Buttons.
        /// </summary>
        public virtual void OnKeyBack()
        {
            Manager.Close();
        }

        public Manager.Data Data
        {
            get;
            set;
        }

        public Canvas Canvas
        {
            get
            {
                return _canvas;
            }
            set
            {
                _canvas = value;
            }
        }

        public Camera Camera
        {
            get
            {
                return _camera;
            }
            set
            {
                _camera = value;
            }
        }

        GameObject m_Shield;

        public void Show()
        {
            Animation.StartShow();
        }

        public void Hide()
        {
            Animation.StartHide();
        }

        public void CreateShield()
        {
            return;
            if (m_Shield == null && _canvas.sortingOrder > 0)
            {
                m_Shield = new GameObject("Shield");
                m_Shield.layer = LayerMask.NameToLayer("UI");

                Image image = m_Shield.AddComponent<Image>();
                image.color = Manager.ShieldColor;

                Transform t = m_Shield.transform;
                t.SetParent(_canvas.transform);
                t.SetSiblingIndex(0);
                t.localScale = Vector3.one;
                t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, 0);

                RectTransform rt = t.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMax = new Vector2(2, 2);
                rt.offsetMin = new Vector2(-2, -2);
            }
        }

        public void SetupCanvas(int sortingOrder)
        {
            if (_canvas == null)
            {
                _canvas = transform.GetComponentInChildren<Canvas>(true);
            }
            if (_canvas.worldCamera == null)
            {
                _canvas.sortingOrder = sortingOrder;
                _canvas.worldCamera = Manager.Object.UICamera;
            }
        }
    }
}