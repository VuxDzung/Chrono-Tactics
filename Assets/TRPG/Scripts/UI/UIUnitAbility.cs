using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevOpsGuy.GUI;
using UnityEngine.UI;
using System;

namespace TRPG.Unit
{
    public class UIUnitAbility : UIContent
    {
        public Action<string> OnSelectAbility;

        [SerializeField] private Button button;

        public void Setup(string aId, Action<string> onSelectAbility)
        {
            id = aId;
            OnSelectAbility = onSelectAbility;
        }

        private void Start()
        {
            button.onClick.AddListener(Trigger);
        }

        public void Trigger()
        {
            OnSelectAbility?.Invoke(id);
        }
    }
}