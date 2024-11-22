using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Gemity.Common;

namespace SS.View
{
    internal class SSControllerService : BaseService<SSControllerService>
    {
        internal const string SceneLoaded = "SceneLoaded";
        internal Controller controller;
        internal Scene scene;
    }

    public abstract class Controller : MonoBehaviour
    {
        [SerializeField] protected Canvas _canvas;
        [SerializeField] protected Camera _camera;

        /// <summary>
        /// When you popup a fullscreen view using SceneManager.Popup(), the system will automatically deactivate the view under it ( for better performance ).
        /// When you close it using SceneManager.Close(), the view which was under it will be activated.
        /// </summary>
        public bool FullScreen;

        /// <summary>
        /// The animation.
        /// </summary>
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

        internal async void Show()
        {
            await Animation.Show();
            Manager.OnShown(this);
            OnShown();
        }

        internal async void Hide()
        {
            await Animation.Hide();
            Manager.OnHidden(this);
            OnHidden();
        }

        public Manager.Data Data
        {
            get;
            internal set;
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

        public virtual void Awake()
        {
            Animation.Init();
            Animation.HideBeforeShowing();
            ServicesDispatch.Execute<SSControllerService>(SSControllerService.SceneLoaded, new() { controller = this, scene = gameObject.scene });
        }

        private void Reset()
        {
            Animation = GetComponentInChildren<SceneAnimation>();
        }
    }
}