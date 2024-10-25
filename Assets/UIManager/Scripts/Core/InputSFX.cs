using UnityEngine.UI;
using TMPro;
namespace DevOpsGuy.GUI
{
    public class InputSFX : UISFXClicker
    {
        private TMP_InputField inputField;

        private void Start()
        {
            inputField = GetComponent<TMP_InputField>();
            inputField.onValueChanged.AddListener((string message) => { Play(); });
        }
    }
}