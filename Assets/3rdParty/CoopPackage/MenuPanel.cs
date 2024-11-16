using DevOpsGuy.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoopPackage
{
    public class MenuPanel : Panel
    {
        public static event Action<string, int> OnCreateRoom;

        private const int maxPlayer = 10;

        [SerializeField] private Button createGameButton;
        [SerializeField] private Button joinGameButton;
        [SerializeField] private Button leaveGameButton;

        private void OnEnable()
        {
            createGameButton.onClick.AddListener(RedirectCreateGame);
            joinGameButton.onClick.AddListener(RedirectJoinGame);
            leaveGameButton.onClick.AddListener(RedirectExit);
        }

        private void OnDisable()
        {
            createGameButton.onClick.RemoveListener(RedirectCreateGame);
            joinGameButton.onClick.RemoveListener(RedirectJoinGame);
            leaveGameButton.onClick.RemoveListener(RedirectExit);
        }

        public void RedirectCreateGame()
        {
            //UIManager.Singleton.ShowUI<CreateGamePanel>();
            //WorldNameGenerator nameGenerator = new();
            //string roomName = nameGenerator.GenerateName();
            UIManager.ShowUI<CreateGamePanel>();
        }

        public void RedirectJoinGame()
        {
            UIManager.ShowUI<JoinGamePanel>();
        }

        public void RedirectExit()
        {
            UIManager.ShowUI<ConfirmModal>();
        }
    }
}