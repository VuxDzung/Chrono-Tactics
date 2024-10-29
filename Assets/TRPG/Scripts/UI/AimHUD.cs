using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevOpsGuy.GUI;
using UnityEngine.UI;
using System;
using TMPro;

namespace TRPG 
{
    public class AimHUD : Modal
    {
        public static Action OnFire;
        public static Action OnCancel;
        [SerializeField] private TextMeshProUGUI tmpEnemyName;
        [SerializeField] private TextMeshProUGUI tmpHitRate;
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
            ClearData();
        }

        public void ClickFire()
        {
            OnFire?.Invoke();
        }

        public void ClickCancel()
        {
            OnCancel?.Invoke();
        }

        public void SetEnemyName(string  enemyName)
        {
            tmpEnemyName.text = enemyName;
        }

        public void SetHitRate(float hitRate)
        {
            tmpHitRate.text = hitRate.ToString();
        }

        private void ClearData()
        {
            tmpEnemyName.text = "";
            tmpHitRate.text = "";
        }
    }
}