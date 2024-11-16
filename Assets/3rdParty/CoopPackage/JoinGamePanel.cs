using DevOpsGuy.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoopPackage
{
    public class JoinGamePanel : Panel
    {
        public static event Action<string> OnJoinRoom;
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button btnBack;

        private void OnEnable()
        {
            joinButton.onClick.AddListener(JoinGame);
            btnBack.onClick.AddListener(Back);
        }

        private void OnDisable()
        {
            joinButton.onClick.RemoveListener(JoinGame);
            btnBack.onClick.RemoveListener(Back);
        }

        public void JoinGame()
        {
            OnJoinRoom(codeInput.text);
        }

        public void Back()
        {
            UIManager.HideUI<JoinGamePanel>();
        }
    }
}