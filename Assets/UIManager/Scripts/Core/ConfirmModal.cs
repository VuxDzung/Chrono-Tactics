using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace DevOpsGuy.GUI
{
    public class ConfirmModal : Modal
    {
        [SerializeField]
        private TextMeshProUGUI tmpTitle;
        [SerializeField]
        private TextMeshProUGUI tmpDescription;
        [SerializeField]
        private Button confirmButton;
        [SerializeField]
        private Button cancelButton;
        [SerializeField]
        private string defaultTitle;
        [SerializeField]
        private string defaultDescription;

        private UnityAction onConfirm;

        public override void Show()
        {
            base.Show();
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);
        }

        public override void Hide()
        {
            base.Hide();
            confirmButton.onClick.RemoveListener(Confirm);
            cancelButton.onClick.RemoveListener(Cancel);
            Clear();
        }

        public override void OnShortcutPressed()
        {
            base.OnShortcutPressed();
            Set(defaultTitle, defaultDescription, () => {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                Application.Quit(); 
            });
        }

        public void Set(string title, string description, UnityAction onConfirm)
        {
            tmpTitle.text = title;
            tmpDescription.text = description;
            this.onConfirm = onConfirm;
        }

        private void Clear()
        {
            tmpTitle.text = "";
            tmpDescription.text = "";
            onConfirm = null;
        }

        private void Confirm()
        {
            onConfirm?.Invoke();
        }

        private void Cancel()
        {
            manager.HideUI(this);
        }
    }
}