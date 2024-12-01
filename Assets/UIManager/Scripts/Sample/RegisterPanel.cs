using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TRPG;

namespace DevOpsGuy.GUI
{
    public class RegisterPanel : Panel
    {
        [SerializeField]
        private TMP_InputField emailInput;
        [SerializeField]
        private TMP_InputField usernameInput;
        [SerializeField]
        private TMP_InputField passwordInput;
        [SerializeField]
        private TMP_InputField confirmPWInput;
        [SerializeField]
        private Button submitButton;

        private string[] obtainedUnitIdArr = new string[] { 
            "UN_0001_000001",
            "UN_0001_000002",
            "UN_0001_000003",
            "UN_0001_000004",
        };

        private string[] inTeamUnitIdArr = new string[] {
            "",
            "",
            "",
            "",
        };

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
            if (string.IsNullOrEmpty(emailInput.text))
            {
                manager.ShowUI<MessageBox>().SetMessage("Empty Email", "Email is empty");
                return;
            }

            if (string.IsNullOrEmpty (usernameInput.text))
            {
                manager.ShowUI<MessageBox>().SetMessage("Username Password", "Username is empty");
                return;
            }

            if (string.IsNullOrEmpty (passwordInput.text))
            {
                manager.ShowUI<MessageBox>().SetMessage("Empty Password", "Password is empty");
                return;
            }

            UserData userData = new UserData("", usernameInput.text, emailInput.text, passwordInput.text, obtainedUnitIdArr, inTeamUnitIdArr);
            SaveLoadUtil.Save(SaveKeys.UserData, userData);

            manager.HideUI(this);
            manager.ShowUI<StarterPanel>();
        }
    }
}