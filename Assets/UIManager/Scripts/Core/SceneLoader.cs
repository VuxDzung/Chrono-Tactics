using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;

namespace DevOpsGuy
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] GameObject loadingPanel;
        [SerializeField] Slider loadingBar;
        [SerializeField] TextMeshProUGUI loadingText;
        [SerializeField] private Sprite[] backgroundList;

        public void LoadScene(string sceneName)
        {
            LoadScene(sceneName, () => {
                Debug.Log("<color=green>Loaded Complete</color>");
            });
        }

        public void LoadScene(string levelName, Action loadedAction)
        {
            //int bgIndex = UnityEngine.Random.Range(0, backgroundList.Length - 1);
            //loadingPanel.GetComponent<Image>().sprite = backgroundList[bgIndex];
            StartCoroutine(LoadSceneAsync(levelName, loadedAction));
        }
        IEnumerator LoadSceneAsync(string levelName, Action loadedAction)
        {
            loadingPanel.SetActive(true);
            AsyncOperation op = SceneUtils.LoadSceneAsync(levelName);
            while (!op.isDone)
            {
                float progress = Mathf.Clamp01(op.progress / .9f);

                if (loadingText != null)
                    loadingText.text = string.Format("{0:0.0}%", progress * 100f);
                if (loadingBar != null)
                    loadingBar.value = progress;

                yield return null;
            }
            //SceneUtils.onSceneChanged?.Invoke();
            loadedAction?.Invoke();
            loadingPanel.SetActive(false);
        }
    }
}