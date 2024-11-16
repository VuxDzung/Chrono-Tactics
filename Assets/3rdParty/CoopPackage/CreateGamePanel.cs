using DevOpsGuy.GUI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoopPackage
{
    public class CreateGamePanel : Panel
    {
        public static event Action<string, int> OnCreateRoom;

        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private TMP_InputField roomPWInput;
        [SerializeField] private Button createGameButton;
        [SerializeField] private Button btnBack;

        private void OnEnable()
        {
            createGameButton.onClick.AddListener(CreateGame);
            btnBack.onClick.AddListener(Back);
        }

        private void OnDisable()
        {
            createGameButton.onClick.RemoveListener(CreateGame);
            btnBack.onClick.RemoveListener(Back);
        }

        public void CreateGame()
        {
            //int maxPlayer = Int32.Parse(maxPlayerInput.text);
            string roomName = roomNameInput.text;
            string roomPW = roomPWInput.text;

            if (OnCreateRoom != null) OnCreateRoom(roomName, 2);

            else Debug.LogWarning("Action OnCreateRoom is NULL");
        }

        public void Back()
        {
            UIManager.HideUI<CreateGamePanel>();
        }
    }
}