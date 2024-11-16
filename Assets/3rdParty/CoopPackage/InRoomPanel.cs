using DevOpsGuy.GUI;
using FishNet;
using FishNet.Managing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace CoopPackage
{
    /// <summary>
    /// - Display all the players in a room.
    /// - Show the room Id, room code, room password.
    /// </summary>
    public class InRoomPanel : Panel
    {
        public static Action OnStartGame;
        public static Action OnPlayerJoinGame;

        [SerializeField] private Transform playerUIHandler;
        [SerializeField] private UIPlayerInMatch uiPlayerPrefab;

        [SerializeField] private TextMeshProUGUI tmpRoomId;
        [SerializeField] private TextMeshProUGUI tmpRoomCode;
        [SerializeField] private TextMeshProUGUI tmpRoomPW;

        [SerializeField] private Button btnStartGame;
        [SerializeField] private Button btnCancelGame;
        [SerializeField] private Button btnRefresh;

        private List<UIPlayerInMatch> uiPlayerList = new List<UIPlayerInMatch>();

        private NetworkManager _networkManager;

        private void Start()
        {
            _networkManager = InstanceFinder.NetworkManager;
        }

        private void OnEnable()
        {
            btnStartGame.onClick.AddListener(StartGame);
            btnCancelGame.onClick.AddListener(CancelGame);
            btnRefresh.onClick.AddListener(Refresh);

            Refresh();
        }

        private void OnDisable()
        {
            btnStartGame.onClick.RemoveListener(StartGame);
            btnCancelGame.onClick.RemoveListener(CancelGame);
            btnRefresh.onClick.RemoveListener(Refresh);
        }

        private void StartGame()
        {
            OnStartGame?.Invoke();  
        }

        private void CancelGame()
        {

        }

        public void SetRoomInformation(string roomId, string roomCode, string roomPW)
        {
            tmpRoomId.text = roomId;
            tmpRoomCode.text = roomCode;
            tmpRoomPW.text = roomPW;
        }

        public void AddPlayerUI(string playerId, string playerName)
        {
            UIPlayerInMatch uiPlayer = Instantiate(uiPlayerPrefab, playerUIHandler);
            uiPlayer.gameObject.SetActive(true);
            uiPlayer.Setup(playerId, playerName);   
            uiPlayerList.Add(uiPlayer);
        }

        public async void Refresh()
        {
            uiPlayerList.ForEach(content => Destroy(content.gameObject));
            uiPlayerList.Clear();

            List<Player> playerList = await CoopController.Singleton.GetPlayers();
            if (playerList != null || playerList.Count == 0)
            {
                UIManager.ShowUI<MessageBox>().SetMessage("Empty Room", "Room has no player in it");
                return;
            }
            playerList.ForEach(player => AddPlayerUI(player.Id, player.Profile.Name));
        }
    }
}