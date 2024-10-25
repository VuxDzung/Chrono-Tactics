using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DevOpsGuy.GUI
{
    public class LoginPanel : Panel
    {
        [SerializeField]
        private TMP_InputField usernameInput;
        [SerializeField]
        private TMP_InputField passwordInput;
        [SerializeField]
        private Button submitButton;
        [SerializeField]
        private Button registerButton;

        public override void Show()
        {
            base.Show();

            submitButton.onClick.AddListener(Login);
            registerButton.onClick.AddListener(Register);
        }

        public override void Hide()
        {
            base.Hide();

            submitButton.onClick.RemoveListener(Login);
            registerButton.onClick.RemoveListener(Register);
        }

        private void Login()
        {
            UIManager.Singleton.Load.LoadScene("Home", () => { 
                Debug.Log("LOAD TO HOME SCENE"); 
                //UIManager.Singleton.ShowUI<>
            });
        }

        private void Register()
        {
            Debug.Log("NAVIGATE:Register");
            UIManager.HideAll();
            UIManager.ShowUI<RegisterPanel>();
        }
    }
}