using DevOpsGuy.GUI;
using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using TRPG;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StarterPanel : Panel
{
    [SerializeField] private Button btnStart;
    private void OnEnable()
    {
        btnStart.onClick.AddListener(GoToHome);
    }

    private void OnDisable()
    {
        btnStart.onClick.RemoveListener(GoToHome);
    }

    public override void Show()
    {
        base.Show();
        CheckCurrentAccount();
    }

    public void GoToHome()
    {
        manager.Load.LoadScene(SceneConfig.SCENE_MENU);
    }

    [Command]
    public void ClearUserData()
    {
        SaveLoadUtil.Save(SaveKeys.UserData, null);
    }

    private void CheckCurrentAccount()
    {
        UserData data = SaveLoadUtil.Load<UserData>(SaveKeys.UserData);
        if (data == null)
        {
            manager.HideUI(this);
            manager.ShowUI<RegisterPanel>();
        }
        else
        {
            Debug.Log($"PlayerData: {SaveLoadUtil.Get(SaveKeys.UserData)}");
        }
    }
}