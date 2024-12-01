using DevOpsGuy.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UTJFadePanel : FadablePanel
{
    [SerializeField] private float delayBeforeChange = 2f;
    private void Start()
    {
        //onFadeOutFinished.AddListener(FadeInThis);
        onFadeInFinished.AddListener(ChangeToStudioFadePanel);
    }

    private void FadeInThis()
    {
        FadeIn();
    }

    private void ChangeToStudioFadePanel()
    {
        StartCoroutine(WaitBeforeChange());
    }

    private IEnumerator WaitBeforeChange()
    {
        yield return new WaitForSeconds(delayBeforeChange);
        manager.ShowUI<StudioFadePanel>().FadeOut();
    }
}
