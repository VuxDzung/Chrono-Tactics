 using DevOpsGuy;
using DevOpsGuy.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TRPG;
using UnityEngine;
using UnityEngine.UI;

namespace CoopPackage
{
    public class MenuPanel : Panel
    {
        private const int maxPlayer = 10;

        [SerializeField] private TextMeshProUGUI tmpPlayerName;

        [SerializeField] private Button btnCompetition;
        [SerializeField] private Button joinGameButton;
        [SerializeField] private Button leaveGameButton;

        private void Start()
        {
            SoundManager.Singleton.PlayBGMusic(MusicCategory.Normal, true);
        }

        private void OnEnable()
        {
            LoadUserUI();
            btnCompetition.onClick.AddListener(RedirectCreateGame);
            leaveGameButton.onClick.AddListener(RedirectExit);
        }

        private void OnDisable()
        {
            btnCompetition.onClick.RemoveListener(RedirectCreateGame);
            leaveGameButton.onClick.RemoveListener(RedirectExit);
        }

        public void RedirectCreateGame()
        {
            //UIManager.Singleton.ShowUI<CreateGamePanel>();
            //WorldNameGenerator nameGenerator = new();
            //string roomName = nameGenerator.GenerateName();
            manager.ShowUI<ActiveMatchPopup>();
        }

        public void RedirectExit()
        {
            manager.ShowUI<ConfirmModal>();
        }

        private void LoadUserUI()
        {
            UserData userData = SaveLoadUtil.Load<UserData>(SaveKeys.UserData);
            if (userData != null )
            {
                tmpPlayerName.text = userData.UserName;
            }
        }
    }
}