using UnityEngine.SceneManagement;
using System;
using UnityEngine;
namespace Utils
{
    public class SceneUtils
    {
        public static Action onSceneChanged;
        public static int GetCurrentSceneIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        public static string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }


        public static void ChangeToScene(int sceneIndex)
        {
            ChangeToScene(sceneIndex, LoadSceneMode.Single);
        }
        public static void ChangeToScene(string sceneName)
        {
            ChangeToScene(sceneName, LoadSceneMode.Single);
        }

        public static void ChangeToScene(string sceneName, LoadSceneMode loadSceneMode)
        {
            SceneManager.LoadScene(sceneName, loadSceneMode);
            onSceneChanged?.Invoke();
        }
        public static void ChangeToScene(int sceneIndex, LoadSceneMode loadSceneMode)
        {
            SceneManager.LoadScene(sceneIndex, loadSceneMode);
            onSceneChanged?.Invoke();
        }

        public static AsyncOperation LoadSceneAsync(string sceneName)
        {
            return SceneManager.LoadSceneAsync(sceneName);
        }
    }
}