using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace DevOpsGuy.GUI
{
    public class ButtonSFX : UISFXClicker
    {
        private Button button;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Play);
        }
    }
}