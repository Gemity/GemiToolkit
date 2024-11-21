using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gemity.GemiSS
{
    public class SSData
    {
        public int id;
        public string sceneName;
        public Action onShow;
        public Action onHide;

        public Scene scene;
        public object data;
    }

    public static class Manager
    {
        public static float FadeDuration = 0.3f;
        public static float AnimDuration = 0.4f;
        public static string LoadingSceneName;

        private static GameObject _loadingScene;
        private static Stack<SSData> _allScenes = new();

        public static void Load(string sceneName, object sender = null)
        {
            SSData data = new()
            {
                id = DateTime.Now.GetHashCode(),
                sceneName = sceneName,
                data = sender
            };

            _allScenes.Push(data);
            SceneManager.LoadScene(data.sceneName, LoadSceneMode.Single);
        }

        public static SSData PopData()
        {
            if(_allScenes.Count == 0)
                 return null;

            return _allScenes.Pop();
        }
    }
}
