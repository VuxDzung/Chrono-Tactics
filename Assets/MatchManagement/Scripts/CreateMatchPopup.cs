using DevOpsGuy.GUI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateMatchPopup : Modal
{
    [SerializeField] private TMP_InputField tmpMatchName;
    [SerializeField] private Toggle usePWToggle;
    [SerializeField] private GameObject matchPasswordInputField;
    [SerializeField] private TMP_InputField tmpMatchPassword;
    [SerializeField] private Button btnCreateMatch;
    [SerializeField] private Button btnClose;

    private void OnEnable()
    {
        usePWToggle.onValueChanged.AddListener(OnToggleChange);
        btnCreateMatch.onClick.AddListener(CreateMatch);
        btnClose.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        usePWToggle.onValueChanged.RemoveListener(OnToggleChange);
        btnCreateMatch.onClick.RemoveListener(CreateMatch);
        btnClose.onClick.RemoveListener(Close);
    }

    private void OnToggleChange(bool isOn)
    {
        if (isOn)
        {
            matchPasswordInputField.SetActive(true);
        }
        else
        {
            matchPasswordInputField.SetActive(false);
        }
    }

    private void CreateMatch()
    {
        string matchName = tmpMatchName.text;
        string password = tmpMatchPassword.text;
        bool usePassword = usePWToggle.isOn;

        if (NetworkGameManager.Singleton.SDK == null)
        {
            Debug.LogError("SDK is Null");
            return;
        }

        if (NetworkGameManager.Singleton.SDK.Lobby == null)
        {
            Debug.LogError("Lobby services is null");
            return;
        }

        NetworkGameManager.Singleton.SDK.Lobby.CreateLobby(matchName, 2);

        
        manager.HideUI(this);
        manager.ShowUI<LoadingPopup>().SetMessage("Creating match");
    }

    private void Close()
    {
        manager.HideUI(this);
    }
}
