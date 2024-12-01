using TMPro;
using UnityEngine;

namespace DevOpsGuy.GUI
{
    public class LoadingPopup : Modal
    {
        [SerializeField] private TextMeshProUGUI tmpMessage;

        public void SetMessage(string messsage)
        {
            tmpMessage.text = messsage;
        }
    }
}