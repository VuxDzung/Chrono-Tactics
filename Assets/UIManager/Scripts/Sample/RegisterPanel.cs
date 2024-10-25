using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DevOpsGuy.GUI
{
    public class RegisterPanel : Panel
    {
        [SerializeField]
        private TMP_InputField usernameInput;
        [SerializeField]
        private TMP_InputField emailInput;
        [SerializeField]
        private TMP_InputField passwordInput;
        [SerializeField]
        private TMP_InputField confirmPWInput;
        [SerializeField]
        private Button submitButton;


        public override void Show()
        {
            base.Show();
            submitButton.onClick.AddListener(Register);
        }

        public override void Hide()
        {
            base.Hide();
            submitButton.onClick.RemoveListener(Register);
        }

        private void Register()
        {
            Debug.Log($"REGISTER:\nEmail:{emailInput.text}\nUsername:{usernameInput.text}\nPassword:{passwordInput.text}");
        }
    }
}