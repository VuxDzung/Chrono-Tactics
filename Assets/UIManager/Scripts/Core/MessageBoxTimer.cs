using System.Collections;
using UnityEngine;

namespace DevOpsGuy.GUI
{
    public class MessageBoxTimer : MessageBox
    {
        [SerializeField] private float hideTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(HideCoroutine());
        }

        private IEnumerator HideCoroutine()
        {
            yield return new WaitForSeconds(hideTime);
            CloseModal();
        }
    }
}