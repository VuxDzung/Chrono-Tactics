using DevOpsGuy.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudioFadePanel : FadablePanel
{
    [SerializeField] private float delayBeforeChange = 2f;
    [SerializeField] private Image darkBackground;

    private void Start()
    {
        onFadeOutFinished.AddListener(EndFading);
    }

    private void EndFading()
    {
        StartCoroutine(WaitBeforeChange());
    }

    private IEnumerator WaitBeforeChange()
    {
        yield return new WaitForSeconds(delayBeforeChange);
        HideBackground();
        canvasGroup.alpha = 0;
    }

    private void HideBackground()
    {
        darkBackground.enabled = false;
    }
}