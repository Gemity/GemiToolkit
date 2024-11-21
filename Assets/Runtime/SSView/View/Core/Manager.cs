using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;

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

            public Data(object data, Callback onShown, Callback onHidden)
            {
                this.data = data;
                this.onShown = onShown;
                this.onHidden = onHidden;
            }
        }

        public delegate void Callback();

        static Stack<Controller> m_ControllerStack = new Stack<Controller>();
        static Queue<Data> m_DataQueue = new Queue<Data>();

        static bool m_SetupCover;
        static bool m_LoadingActive;

        static string m_MainSceneName;
        static Controller m_MainController;

        static string m_LoadingSceneName;
        static Controller m_LoadingController;

        public static int stackCount
        {
            get
            {
                return m_ControllerStack.Count;
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

            SceneManager.sceneLoaded += OnSceneLoaded;

            ShieldColor = new Color(0f, 0f, 0f, 0.45f);
            SceneFadeDuration = 0.15f;
            SceneAnimationDuration = 0.3f;

            Object = ((GameObject)GameObject.Instantiate(Resources.Load("ManagerObject"))).GetComponent<ManagerObject>();
            Object.gameObject.name = "ManagerObject";
        }

        public static void Load(string sceneName, object data = null)
        {
            if (Object.GetState() == ManagerObject.State.SHIELD_FADE_IN) //if scene load shield is active
            {
                Debug.LogWarning("Scene is fading in/out, this load will be ignored");
                return;
            }

            m_DataQueue.Enqueue(new Data(data, null, null));
            m_MainSceneName = sceneName;
            Object.FadeOutScene();
        }

        public static void Add(string sceneName, object data = null, Callback onShown = null, Callback onHidden = null)
        {
            m_DataQueue.Enqueue(new Data(data, onShown, onHidden));
            Object.ShieldOn();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public static void Close()
        {
            if (m_ControllerStack.Count > 0)
            {
                Object.ShieldOn();
                m_ControllerStack.Peek().Hide();
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

        public static void OnShown(Controller controller)
        {
            controller.OnShown();
            controller.Data.onShown?.Invoke();
            Object.ShieldOff();
        }

        public static void OnHidden(Controller controller)
        {
            controller.OnHidden();
            controller.Data.onHidden?.Invoke();

            Unload();

            if (m_ControllerStack.Count > 0)
            {
                var currentController = m_ControllerStack.Peek();
                currentController.OnReFocus();
            }

            Object.ShieldOff();
        }

        public static bool IsActiveShield()
        {
            return Object.Active || m_LoadingActive;
        }

        public static Controller TopController()
        {
            if (m_ControllerStack.Count > 0)
            {
                return m_ControllerStack.Peek();
            }

            return null;
        }

        public static void EndFadedIn()
        {
            m_MainController.OnShown();
        }

        public static void EndFadedOut()
        {
            if (m_MainController != null)
            {
                m_MainController.OnHidden();
            }

            SceneManager.LoadScene(m_MainSceneName, LoadSceneMode.Single);
        }

        static void Unload()
        {
            if (m_ControllerStack.Count > 0)
            {
                SceneManager.UnloadSceneAsync(m_ControllerStack.Pop().Data.scene);
            }
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Get Controller
            var controller = GetController(scene);
            if (controller == null)
            {
                m_ControllerStack.Push(null);
                return;
            }

            // Loading Scene
            if (controller.SceneName() == LoadingSceneName)
            {
                controller.SetupCanvas(98);
                m_LoadingController = controller;
                GameObject.DontDestroyOnLoad(m_LoadingController.gameObject);
                LoadingAnimation(false);

                return;
            }

            // Single Mode automatically destroy all scenes, so we have to clear the stack.
            if (mode == LoadSceneMode.Single)
            {
                m_ControllerStack.Clear();
            }

            // Unload resources and collect GC.
            //Resources.UnloadUnusedAssets();
            //System.GC.Collect();

            // Get Data
            if (m_DataQueue.Count == 0)
            {
                m_DataQueue.Enqueue(new Data(null, null, null));
            }
            var data = m_DataQueue.Dequeue();
            data.scene = scene;

            // Push the current scene to the stack.
            m_ControllerStack.Push(controller);

            // Setup controller
            controller.Data = data;
            controller.SetupCanvas(m_ControllerStack.Count - 1);
            controller.CreateShield();
            controller.OnActive(data.data);

            // Animation
            if (m_ControllerStack.Count == 1)
            {
                // Main Scene
                m_MainController = controller;

                // Fade
                Object.FadeInScene();
            }
            else
            {
                // Popup Scene
                controller.Show();
            }
        }

        static Controller GetController(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var controller = roots[i].GetComponent<Controller>();
                if (controller != null)
                {
                    return controller;
                }
            }
            return null;
        }
    }
}