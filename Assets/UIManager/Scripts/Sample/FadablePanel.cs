using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DevOpsGuy.GUI
{
    public enum FisrtFadeType
    {
        In,
        Out,
    }

    [RequireComponent(typeof(CanvasGroup))]
    public class FadablePanel : Panel
    {
        [SerializeField] private bool runFirst;
        [SerializeField] protected FisrtFadeType fadeTypeAwake;
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected float lerpSpeed = 10;
        [SerializeField] protected float fadeDuration = 2;

        public UnityEvent onFadeInStarted;
        public UnityEvent onFadeInFinished;

        public UnityEvent onFadeOutStarted;
        public UnityEvent onFadeOutFinished;

        [SerializeField] protected List<Image> fadeElementList = new List<Image>();

        public override void Setup(UIManager manager)
        {
            base.Setup(manager);
            canvasGroup = GetComponent<CanvasGroup>();

            if (runFirst)
            {
                switch (fadeTypeAwake)
                {
                    case FisrtFadeType.In:
                        FadeIn();
                        break;
                    case FisrtFadeType.Out:
                        FadeOut();
                        break;
                }
            }

        }

        public void FadeIn()
        {
            StartCoroutine(FadeCoroutine(true));
        }

        public void FadeOut()
        {
            StartCoroutine(FadeCoroutine(false));
        }

        private bool AllHasFade(bool isFadeIn)
        {
            if (fadeElementList.Count > 0)
            {
                if (isFadeIn)
                {
                    foreach (Image image in fadeElementList)
                        if (image.color.a != 0)
                            return false;
                    return true;
                }
                else
                {
                    foreach (Image image in fadeElementList)
                        if (image.color.a < 255)
                            return false;
                    return true;

                }
            }
            return false;
        }

        IEnumerator FadeCoroutine(bool fadeAway)
        {
            // fade from opaque to transparent
            if (fadeAway)
            {
                onFadeInStarted?.Invoke();
                while (canvasGroup.alpha > 0.0f)
                {
                    canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, -0.1f, Time.deltaTime * lerpSpeed);
                    yield return null;

                }
                onFadeInFinished?.Invoke();
            }
            // fade from transparent to opaque
            else
            {
                onFadeOutStarted?.Invoke();
                
                while (canvasGroup.alpha < 1f)
                {
                    canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1.1f, Time.deltaTime * lerpSpeed);
                    yield return null;

                }
                onFadeOutFinished?.Invoke();
            }
        }
    }
}