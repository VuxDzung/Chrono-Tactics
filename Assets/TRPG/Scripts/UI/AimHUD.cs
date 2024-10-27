using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevOpsGuy.GUI;
using UnityEngine.UI;
using System;

namespace TRPG 
{
    public class AimHUD : Modal
    {
        public static Action OnFire;
        public static Action OnCancel;

        [SerializeField] private Button btnFire;
        [SerializeField] private Button btnCancel;

        private void OnEnable()
        {
            btnFire.onClick.AddListener(ClickFire);
            btnCancel.onClick.AddListener(ClickCancel);
        }

        private void OnDisable()
        {
            btnFire.onClick.RemoveListener(ClickFire);
            btnCancel.onClick.RemoveListener(ClickCancel);
        }

        public void ClickFire()
        {
            OnFire?.Invoke();
        }

        public void ClickCancel()
        {
            OnCancel?.Invoke();
        }
    }
}