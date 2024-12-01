using DevOpsGuy.GUI;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;


public class ActiveMatchPopup : Modal
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private UIContentButton contentPrefab;
    [SerializeField] private Button btnAddMatch;
    [SerializeField] private Button btnClose;

    private List<UIContentButton> contentList = new List<UIContentButton>();

    private void OnEnable()
    {
        LoadAvailableMatches();
        btnAddMatch.onClick.AddListener(AddMatch);
        btnClose.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        ClearAllUIMatches();

        btnAddMatch.onClick.RemoveListener(AddMatch);
        btnClose.onClick.RemoveListener(Close);
    }

    /// <summary>
    /// Load all matches which have 1 player in them.
    /// </summary>
    public async void LoadAvailableMatches()
    {
        if (!gameObject.activeSelf) return;

        if (NetworkGameManager.Singleton.SDK == null) return;

        ClearAllUIMatches();

        List<Lobby> lobbyList = await NetworkGameManager.Singleton.SDK.Lobby.GetLobbies();

        if (lobbyList == null || lobbyList.Count == 0) return;

        lobbyList.ForEach(lobby => {
            UIContentButton _content = Instantiate(contentPrefab, contentParent);
            _content.Setup(lobby.Id, lobby.Name, JoinLobby);
            contentList.Add(_content);
        });
    }

    private void ClearAllUIMatches()
    {
        contentList.ForEach(content => Destroy(content.gameObject));
        contentList.Clear();
    }

    private void Close()
    {
        manager.HideUI(this);
    }

    private void AddMatch()
    {
        manager.HideUI(this);
        manager.ShowUI<CreateMatchPopup>();
    }

    private void JoinLobby(string lobbyId)
    {
        NetworkGameManager.Singleton.SDK.Lobby.JoinLobbyById(lobbyId);
    }
}
