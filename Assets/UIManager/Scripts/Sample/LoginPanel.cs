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
        private TMP_InputField emailInput;
        [SerializeField]
        private TMP_InputField passwordInput;
        [SerializeField]
        private Button btnSignIn;

        public override void Show()
        {
            base.Show();
            btnSignIn.onClick.AddListener(Login);
        }

        public override void Hide()
        {
            base.Hide();
            btnSignIn.onClick.RemoveListener(Login);
        }

        private void Login()
        {
            string userName = emailInput.text;
            string password = passwordInput.text;

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                manager.HideUI(this);
                manager.ShowUI<StarterPanel>();
            }
            else
            {
                manager.ShowUI<MessageBox>().SetMessage("Empty Fields", "Username/Password is empty!");
            }
            //UIManager.Singleton.Load.LoadScene(SceneConfig.SCENE_MENU, () => { 
            //    Debug.Log("LOAD TO HOME SCENE"); 
            //    //UIManager.Singleton.ShowUI<>
            //});
        }

        public void Register()
        {
            Debug.Log("NAVIGATE:Register");
            manager.HideUI(this);
            manager.ShowUI<RegisterPanel>();
        }

        public void NavigateRegister()
        {

        }

        public void NavigateForgotPW()
        {

        }
    }
}