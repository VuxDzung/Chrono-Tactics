using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevOpsGuy.GUI
{
    public class MessageBox : Modal
    {
        [SerializeField] private TextMeshProUGUI tmpTitle;
        [SerializeField] private TextMeshProUGUI tmpMessage;
        [SerializeField] private Button confirmButton;

        protected virtual void OnEnable()
        {
            if (confirmButton) confirmButton.onClick.AddListener(CloseModal);
        }

        protected virtual void OnDisable()
        {
            if (confirmButton) confirmButton.onClick.RemoveListener(CloseModal);
        }

        public void SetMessage(string title, string message)
        {
            if (tmpTitle) tmpTitle.text = title;
            if (tmpMessage) tmpMessage.text = message;
        }

        protected void CloseModal()
        {
            manager.HideUI(this);
        }
    }
}