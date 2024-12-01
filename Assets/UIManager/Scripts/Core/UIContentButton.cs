using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevOpsGuy.GUI
{
    public class UIContentButton : UIContent
    {
        public Action<string> OnClick;

        [SerializeField] private TextMeshProUGUI tmpLabel;
        [SerializeField] protected Button button;

        public void Setup(string id, string label, Action<string> onClick)
        {
            gameObject.SetActive(true);
            this.id = id;
            OnClick = onClick;
            tmpLabel.text = label;
            button.onClick.AddListener(Click);
        }

        private void Click()
        {
            OnClick?.Invoke(id);
        }
    }
}