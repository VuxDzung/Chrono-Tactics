using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoopPackage
{
    public class UIPlayerInMatch : MonoBehaviour
    {
        public static Action<string> OnRemovePlayer;

        [SerializeField] private TextMeshProUGUI tmpPlayerName;
        [SerializeField] private Button btnRemove;

        private string playerId;

        private void Start()
        {
            btnRemove.onClick.AddListener(RemovePlayer);
        }

        public virtual void Setup(string playerId, string playerName)
        {
            this.playerId = playerId;
            tmpPlayerName.text = playerName;
        }

        private void RemovePlayer()
        {
            OnRemovePlayer?.Invoke(playerId);
        }
    }
}