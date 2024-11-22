using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Gemity.Common;
using Debug = Gemity.Common.Debug;

namespace SS.View
{
    public class Manager
    {
        public class Data
        {
            public object data;
            public Callback onShown;
            public Callback onHidden;
            public Scene scene;
            public LoadSceneMode loadMode;
        }

        public delegate void Callback();

        static Stack<Controller> _controllerStack = new();
        static Queue<Data> m_DataQueue = new();

        static bool m_LoadingActive;

        static string m_MainSceneName;
        static Controller m_MainController;

        static string m_LoadingSceneName;
        static Controller m_LoadingController;

        public static int stackCount
        {
            get
            {
                return _controllerStack.Count;
            }
        }

        public static Controller MainController
        {
            get
            {
                return m_MainController;
            }
        }

        public static Color ShieldColor
        {
            get;
            set;
        }

        public static float SceneFadeDuration
        {
            get;
            set;
        }

        public static float SceneAnimationDuration
        {
            get;
            set;
        }

        public static ManagerObject Object
        {
            get;
            protected set;
        }

        static Manager()
        {
            Application.targetFrameRate = 60;

            ServicesDispatch.Add<SSControllerService>(SSControllerService.SceneLoaded, OnSceneLoaded);

            ShieldColor = new Color(0f, 0f, 0f, 0.45f);
            SceneFadeDuration = 0.15f;
            SceneAnimationDuration = 0.3f;

            Object = ((GameObject)GameObject.Instantiate(Resources.Load("ManagerObject"))).GetComponent<ManagerObject>();
            Object.gameObject.name = "ManagerObject";
        }

        public static async void Load(string sceneName, object data = null)
        {
            if(!CanLoadScene())
                return;

            m_DataQueue.Enqueue(new() { data = data, loadMode = LoadSceneMode.Single});
            m_MainSceneName = sceneName;

            await Object.FadeOutScene();

            if (m_MainController != null)
                m_MainController.OnHidden();

            SceneManager.LoadScene(m_MainSceneName, LoadSceneMode.Single);
        }

        public static async void LoadAsync(string sceneName, object data = null)
        {
            if (!CanLoadScene())
                return;

            m_DataQueue.Enqueue(new() { data = data, loadMode = LoadSceneMode.Single});
            m_MainSceneName = sceneName;

            await Object.FadeOutScene();

            if (m_MainController != null)
                m_MainController.OnHidden();

            await SceneManager.LoadSceneAsync(m_MainSceneName, LoadSceneMode.Single);
        }

        public static async UniTask PreLoadAsync(string sceneName, object data = null, CancellationToken token = default)
        {
            if (!CanLoadScene())
                return;

            m_DataQueue.Enqueue(new() { data = data, loadMode = LoadSceneMode.Single });
            m_MainSceneName = sceneName;

            await Object.FadeOutScene();

            if (m_MainController != null)
                m_MainController.OnHidden();

            var loadOperation = SceneManager.LoadSceneAsync(m_MainSceneName, LoadSceneMode.Single);
            while(!loadOperation.isDone && !token.IsCancellationRequested)
                await UniTask.Yield();

            loadOperation.allowSceneActivation = false;
        }

        public static void Add(string sceneName, object data = null, Callback onShown = null, Callback onHidden = null)
        {
            m_DataQueue.Enqueue(new() { data = data, loadMode = LoadSceneMode.Additive, onShown = onShown, onHidden = onHidden });
            Object.ShieldOn();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public static void Close()
        {
            if (_controllerStack.Count > 0)
            {
                Object.ShieldOn();
                _controllerStack.Peek().Hide();
            }
        }

        public static string LoadingSceneName
        {
            set
            {
                m_LoadingSceneName = value;
                SceneManager.LoadScene(m_LoadingSceneName, LoadSceneMode.Additive);
            }
            get
            {
                return m_LoadingSceneName;
            }
        }

        public static void LoadingAnimation(bool active)
        {
            if (m_LoadingController != null)
            {
                m_LoadingActive = active;
                m_LoadingController.gameObject.SetActive(active);
            }
            else
            {
                Debug.LogError("Loading Controller has not been set");
            }
        }

        internal static void OnShown(Controller controller)
        {
            controller.Data.onShown?.Invoke();
            Object.ShieldOff();
        }

        internal static void OnHidden(Controller controller)
        {
            controller.Data.onHidden?.Invoke();
            SceneManager.UnloadSceneAsync(_controllerStack.Pop().Data.scene);

            if (_controllerStack.Count > 0)
            {
                var currentController = _controllerStack.Peek();
                currentController.OnReFocus();
            }

            Object.ShieldOff();
        }

        public static bool IsActiveShield()
        {
            return Object.ActiveShield || m_LoadingActive;
        }

        private static bool CanLoadScene()
        {
            if (Object.GetState() == ManagerObject.State.SHIELD_FADE_IN) //if scene load shield is active
            {
                Debug.LogWarning("Scene is fading in/out, this load will be ignored");
                return false;
            }

            return true;
        }


        static void OnSceneLoaded(SSControllerService info)
        {
            var controller = info.controller;

            if (controller.SceneName() == LoadingSceneName)
            {
                controller.SetupCanvas(100);
                m_LoadingController = controller;
                GameObject.DontDestroyOnLoad(m_LoadingController.gameObject);
                LoadingAnimation(false);
                return;
            }

            if (m_DataQueue.Count == 0)            
                return;

            var data = m_DataQueue.Dequeue();

            if (data.loadMode == LoadSceneMode.Single)
                _controllerStack.Clear();

            data.scene = info.scene;
            _controllerStack.Push(controller);

            controller.Data = data;
            controller.SetupCanvas(_controllerStack.Count - 1);
            controller.OnActive(data.data);

            if (_controllerStack.Count == 1)
            {
                m_MainController = controller;
                Object.FadeInScene().Forget();
            }
            else
                controller.Show();
        }
    }
}